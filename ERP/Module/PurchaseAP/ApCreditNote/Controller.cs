using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APCreditNote;

[ApiController]
[Route("[controller]")]
public class APCreditNoteController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IAPCreditNoteJournalService _journalService;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public APCreditNoteController(MyDbContext db, IMapper mapper, IAPCreditNoteJournalService journalService, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    { _db = db; _mapper = mapper; _journalService = journalService; _approvalGate = approvalGate; _printLayout = printLayout; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<APCreditNoteResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();
        return Ok(_mapper.Map<APCreditNoteResponse>(cn));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APCreditNoteRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var cn = _mapper.Map<APCreditNote>(dto);
        foreach (var line in cn.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // A Draft credit note is saved as-is, with no GL movement yet — that
        // only happens when it's posted (here, if not a draft, or later via
        // POST /APCreditNote/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        cn.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<APCreditNote>().Add(cn);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<APCreditNote>()
                .Include(x => x.Vendor)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == cn.Id);

            return CreatedAtAction(nameof(GetById), new { id = cn.Id },
                _mapper.Map<APCreditNoteResponse>(draftCreated));
        }

        // Save the document + its Journal Entry in one transaction (reverse of AP Invoice).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APCreditNote>().Add(cn);
            await _db.SaveChangesAsync();
            await PostCreditNoteAsync(cn, vendor);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == cn.Id);
        return CreatedAtAction(nameof(GetById), new { id = cn.Id },
            _mapper.Map<APCreditNoteResponse>(created));
    }

    // ── POST /APCreditNote/{id}/Post ─── promotes a Draft credit note to
    // Open, running the same Journal Entry posting Create runs for a
    // non-draft credit note.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();
        if (cn.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {cn.Status}).");

        var gate = await _approvalGate.EvaluateAsync("APCreditNote", cn.Id, cn.DocNum, cn.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            cn.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = cn.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int? jeId = null; string? jeNo = null;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostCreditNoteAsync(cn, cn.Vendor);
            cn.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AP Credit Note posted", docNum = cn.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /APCreditNote/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();
        if (cn.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {cn.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APCreditNote", cn.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int? jeId = null; string? jeNo = null;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostCreditNoteAsync(cn, cn.Vendor);
            cn.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AP Credit Note approved and posted", docNum = cn.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /APCreditNote/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var cn = await _db.Set<APCreditNote>().FindAsync(id);
        if (cn == null) return NotFound();
        if (cn.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {cn.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APCreditNote", cn.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        cn.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AP Credit Note rejected", docNum = cn.DocNum });
    }

    /// <summary>Creates the AP Credit Note's Journal Entry. Shared by Create
    /// (non-draft) and Post (draft → open) so both post identically.</summary>
    private async Task<(int? jeId, string? jeNo)> PostCreditNoteAsync(APCreditNote cn, BPEntity vendor)
    {
        var je = _journalService.CreateJournalEntry(cn, vendor);
        je.BaseDocEntry = cn.Id;
        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APCreditNoteUpdateRequest dto)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(cn.Items);
        _mapper.Map(dto, cn);
        foreach (var line in cn.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("APCreditNote", cn, cn.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APCreditNotePdfDocument(cn, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{cn.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cn = await _db.Set<APCreditNote>().FindAsync(id);
        if (cn == null) return NotFound();
        _db.Set<APCreditNote>().Remove(cn);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
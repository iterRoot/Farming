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

namespace FarmingApi.Modules.PurchaseAP.APReserveInvoice;

[ApiController]
[Route("[controller]")]
public class APReserveInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IAPReserveInvoiceJournalService _journalService;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public APReserveInvoiceController(MyDbContext db, IMapper mapper, IAPReserveInvoiceJournalService journalService, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    { _db = db; _mapper = mapper; _journalService = journalService; _approvalGate = approvalGate; _printLayout = printLayout; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<APReserveInvoiceResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inv = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (inv == null) return NotFound();
        return Ok(_mapper.Map<APReserveInvoiceResponse>(inv));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APReserveInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var inv = _mapper.Map<APReserveInvoice>(dto);
        foreach (var line in inv.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // A Draft invoice is saved as-is, with no GL movement yet — that only
        // happens when it's posted (here, if not a draft, or later via
        // POST /APReserveInvoice/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        inv.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<APReserveInvoice>().Add(inv);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<APReserveInvoice>()
                .Include(x => x.Vendor)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == inv.Id);

            return CreatedAtAction(nameof(GetById), new { id = inv.Id },
                _mapper.Map<APReserveInvoiceResponse>(draftCreated));
        }

        // Save the document + its Journal Entry in one transaction (DR Purchase Expense / DR Input VAT / CR Accounts Payable).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APReserveInvoice>().Add(inv);
            await _db.SaveChangesAsync();
            await PostReserveInvoiceAsync(inv, vendor);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == inv.Id);
        return CreatedAtAction(nameof(GetById), new { id = inv.Id },
            _mapper.Map<APReserveInvoiceResponse>(created));
    }

    // ── POST /APReserveInvoice/{id}/Post ─── promotes a Draft invoice to
    // Open, running the same Journal Entry posting Create runs for a
    // non-draft invoice.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var inv = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        if (inv.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {inv.Status}).");

        var gate = await _approvalGate.EvaluateAsync("APReserveInvoice", inv.Id, inv.DocNum, inv.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            inv.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = inv.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int? jeId = null; string? jeNo = null;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostReserveInvoiceAsync(inv, inv.Vendor);
            inv.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AP Reserve Invoice posted", docNum = inv.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /APReserveInvoice/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var inv = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        if (inv.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {inv.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APReserveInvoice", inv.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int? jeId = null; string? jeNo = null;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostReserveInvoiceAsync(inv, inv.Vendor);
            inv.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AP Reserve Invoice approved and posted", docNum = inv.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /APReserveInvoice/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var inv = await _db.Set<APReserveInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        if (inv.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {inv.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APReserveInvoice", inv.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        inv.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AP Reserve Invoice rejected", docNum = inv.DocNum });
    }

    /// <summary>Creates the AP Reserve Invoice's Journal Entry. Shared by
    /// Create (non-draft) and Post (draft → open) so both post identically.</summary>
    private async Task<(int? jeId, string? jeNo)> PostReserveInvoiceAsync(APReserveInvoice inv, BPEntity vendor)
    {
        var je = _journalService.CreateJournalEntry(inv, vendor);
        je.BaseDocEntry = inv.Id;
        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APReserveInvoiceUpdateRequest dto)
    {
        var inv = await _db.Set<APReserveInvoice>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (inv == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(inv.Items);
        _mapper.Map(dto, inv);
        foreach (var line in inv.Items)
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
        var inv = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("APReserveInvoice", inv, inv.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APReserveInvoicePdfDocument(inv, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{inv.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inv = await _db.Set<APReserveInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        _db.Set<APReserveInvoice>().Remove(inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
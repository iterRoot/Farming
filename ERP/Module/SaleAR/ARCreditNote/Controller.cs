using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using FarmingApi.Services;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.ARCreditNote;

[ApiController]
[Route("[controller]")]          // ✅ route = /ARCreditNote
public class ARCreditNoteController : ControllerBase  // ✅ renamed
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IARCreditNoteJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public ARCreditNoteController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IARCreditNoteJournalService journalService,
        IInventoryPostingService inventoryPosting,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _docNumber = docNumber; _journalService = journalService;
        _inventoryPosting = inventoryPosting;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<ARCreditNoteListResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cn = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();
        return Ok(_mapper.Map<ARCreditNoteListResponse>(cn));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARCreditNoteListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var cn = _mapper.Map<ARCreditNote>(dto);

        // Auto-assign the document number (server-side, race-safe).
        try { cn.DocNum = _docNumber.Next("ArCreditNote"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in cn.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            // Blank warehouse falls back to the default; an unknown one is rejected.
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // A Draft credit note is saved as-is, with no stock/GL movement yet —
        // those only happen when it's posted (here, if not a draft, or later
        // via POST /ARCreditNote/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        cn.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<ARCreditNote>().Add(cn);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<ARCreditNote>()
                .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == cn.Id);

            return CreatedAtAction(nameof(GetById), new { id = cn.Id },
                _mapper.Map<ARCreditNoteListResponse>(draftCreated));
        }

        // Save the credit note + its Journal Entry in one transaction —
        // reverse of AR Invoice: DR Revenue / DR Output VAT / CR Accounts Receivable.
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<ARCreditNote>().Add(cn);
            await _db.SaveChangesAsync();           // cn.Id populated

            (jeId, jeNo) = await PostCreditNoteAsync(cn, customer);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == cn.Id);

        var response = _mapper.Map<ARCreditNoteListResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;

        return CreatedAtAction(nameof(GetById), new { id = cn.Id }, response);
    }

    // ── POST /ARCreditNote/{id}/Post ─── promotes a Draft credit note to
    // Open, running the same inventory-return + Journal Entry posting Create
    // runs for a non-draft credit note.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var cn = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();
        if (cn.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {cn.Status}).");

        var gate = await _approvalGate.EvaluateAsync("ARCreditNote", cn.Id, cn.DocNum, cn.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            cn.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = cn.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostCreditNoteAsync(cn, cn.Customer);
            cn.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AR Credit Note posted", docNum = cn.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /ARCreditNote/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var cn = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();
        if (cn.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {cn.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARCreditNote", cn.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostCreditNoteAsync(cn, cn.Customer);
            cn.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AR Credit Note approved and posted", docNum = cn.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /ARCreditNote/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var cn = await _db.Set<ARCreditNote>().FindAsync(id);
        if (cn == null) return NotFound();
        if (cn.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {cn.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARCreditNote", cn.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        cn.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AR Credit Note rejected", docNum = cn.DocNum });
    }

    /// <summary>
    /// Creates the AR Credit Note's Journal Entry and brings returned goods
    /// back into stock for lines not already received by a prior Return.
    /// Shared by Create (non-draft) and Post (draft → open) so both post
    /// identically.
    /// </summary>
    private async Task<(int? jeId, string? jeNo)> PostCreditNoteAsync(ARCreditNote cn, BPEntity customer)
    {
        var je = _journalService.CreateJournalEntry(cn, customer);
        je.BaseDocEntry = cn.Id;

        // Bring the returned goods back into stock — but not for lines copied
        // from a Return, which already received them. Returns come back at the
        // item's current average cost, so the average is left undisturbed.
        var stockDate = cn.PostingDate ?? DateTime.UtcNow;
        foreach (var line in cn.Items.Where(l => !IsBasedOnReturn(l)))
        {
            await _inventoryPosting.PostReturnInAsync(
                line.ItemCode!, line.WhsCode!, line.Quantity,
                "AR Credit Note", "ARCreditNote", cn.Id, cn.DocNum, stockDate,
                customer.Code, customer.CardName, batches: line.Batches);
        }

        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARCreditNoteUpdateRequest dto)
    {
        var cn = await _db.Set<ARCreditNote>().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        _db.RemoveRange(cn.Items);
        _mapper.Map(dto, cn);
        foreach (var line in cn.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// True when the line was copied from a Return. The Return already brought
    /// the goods back into stock, so the credit note must not post the movement
    /// again — otherwise the same goods re-enter inventory twice.
    /// </summary>
    private static bool IsBasedOnReturn(ARCreditNoteLine line) =>
        line.BaseEntry.HasValue
        && string.Equals(line.BaseType, "Return", StringComparison.OrdinalIgnoreCase);

    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var cn = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("ARCreditNote", cn, cn.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ARCreditNotePdfDocument(cn, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{cn.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cn = await _db.Set<ARCreditNote>().FindAsync(id);
        if (cn == null) return NotFound();
        _db.Set<ARCreditNote>().Remove(cn);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APInvoice;

[ApiController]
[Route("[controller]")]   // ✅ route = /APInvoice
public class APInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IAPInvoiceJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public APInvoiceController(
        MyDbContext db,
        IMapper mapper,
        IAPInvoiceJournalService journalService,
        IInventoryPostingService inventoryPosting,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _journalService = journalService;
        _inventoryPosting = inventoryPosting;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /APInvoice ─────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<APInvoiceResponse>>(list));
    }

    // ── GET /APInvoice/{id} ────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inv = await _db.Set<APInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        return Ok(_mapper.Map<APInvoiceResponse>(inv));
    }

    // ── POST /APInvoice ────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var inv = _mapper.Map<APInvoice>(dto);

        foreach (var line in inv.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            // Blank warehouse falls back to the default; an unknown one is rejected.
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // A Draft invoice is saved as-is, with no stock/GL movement yet —
        // those only happen when it's posted (here, if not a draft, or later
        // via POST /APInvoice/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        inv.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<APInvoice>().Add(inv);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<APInvoice>()
                .Include(x => x.Vendor)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == inv.Id);

            return CreatedAtAction(nameof(GetById), new { id = inv.Id },
                _mapper.Map<APInvoiceResponse>(draftCreated));
        }

        // Save the document + its Journal Entry in one transaction (DR Purchase Expense / DR Input VAT / CR Accounts Payable).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APInvoice>().Add(inv);
            await _db.SaveChangesAsync();
            await PostAPInvoiceAsync(inv, vendor);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<APInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == inv.Id);

        return CreatedAtAction(nameof(GetById), new { id = inv.Id },
            _mapper.Map<APInvoiceResponse>(created));
    }

    // ── POST /APInvoice/{id}/Post ─── promotes a Draft invoice to Open,
    // running the same inventory + Journal Entry posting Create runs for a
    // non-draft invoice.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var inv = await _db.Set<APInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        if (inv.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {inv.Status}).");

        var gate = await _approvalGate.EvaluateAsync("APInvoice", inv.Id, inv.DocNum, inv.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            inv.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = inv.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var (jeId, jeNo) = await PostAPInvoiceAsync(inv, inv.Vendor);
            inv.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "AP Invoice posted", docNum = inv.DocNum, journalEntryId = jeId, journalNo = jeNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ── POST /APInvoice/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var inv = await _db.Set<APInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        if (inv.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {inv.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APInvoice", inv.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var (jeId, jeNo) = await PostAPInvoiceAsync(inv, inv.Vendor);
            inv.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "AP Invoice approved and posted", docNum = inv.DocNum, journalEntryId = jeId, journalNo = jeNo });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ── POST /APInvoice/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var inv = await _db.Set<APInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        if (inv.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {inv.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APInvoice", inv.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        inv.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AP Invoice rejected", docNum = inv.DocNum });
    }

    /// <summary>
    /// Creates the AP Invoice's Journal Entry and posts stock in for lines
    /// not already received by a prior Goods Receipt PO. Shared by Create
    /// (non-draft) and Post (draft → open) so both post identically.
    /// </summary>
    private async Task<(int? jeId, string? jeNo)> PostAPInvoiceAsync(APInvoice inv, BPEntity vendor)
    {
        var je = _journalService.CreateJournalEntry(inv, vendor);
        je.BaseDocEntry = inv.Id;

        var stockDate = inv.PostingDate ?? DateTime.UtcNow;
        foreach (var line in inv.Items.Where(l => !IsBasedOnGoodsReceipt(l)))
        {
            await _inventoryPosting.PostInAsync(
                line.ItemCode!, line.WhsCode!, line.Quantity, line.Price,
                "AP Invoice", "ApInvoice", inv.Id, inv.DocNum, stockDate,
                vendor.Code, vendor.CardName, batches: line.Batches);
        }

        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    // ── PUT /APInvoice/{id} ────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APInvoiceUpdateRequest dto)
    {
        var inv = await _db.Set<APInvoice>()
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

            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// True when the line was copied from a Goods Receipt PO. The receipt already
    /// brought the goods into stock, so the invoice must not post the movement
    /// again — otherwise the same goods enter inventory twice.
    /// </summary>
    private static bool IsBasedOnGoodsReceipt(APInvoiceLine line) =>
        line.BaseEntry.HasValue
        && string.Equals(line.BaseType, "GoodsReceiptPO", StringComparison.OrdinalIgnoreCase);

    // ── GET /APInvoice/{id}/Pdf ─────────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var inv = await _db.Set<APInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("APInvoice", inv, inv.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APInvoicePdfDocument(inv, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{inv.DocNum}.pdf");
    }

    // ── DELETE /APInvoice/{id} ─────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inv = await _db.Set<APInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        _db.Set<APInvoice>().Remove(inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
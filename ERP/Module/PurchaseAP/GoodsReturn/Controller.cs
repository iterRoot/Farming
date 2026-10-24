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

namespace FarmingApi.Modules.PurchaseAP.GoodsReturn;

[ApiController]
[Route("[controller]")]
public class GoodsReturnController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IGoodsReturnJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IApprovalGateService _approvalGate;

    public GoodsReturnController(
        MyDbContext db,
        IMapper mapper,
        IGoodsReturnJournalService journalService,
        IInventoryPostingService inventoryPosting,
        IApprovalGateService approvalGate)
    { _db = db; _mapper = mapper; _journalService = journalService; _inventoryPosting = inventoryPosting; _approvalGate = approvalGate; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<GoodsReturnResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (gr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReturnResponse>(gr));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReturnRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var gr = _mapper.Map<GoodsReturn>(dto);
        foreach (var line in gr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            // Blank warehouse falls back to the default; an unknown one is rejected.
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // A Draft return is saved as-is, with no stock/GL movement yet —
        // those only happen when it's posted (here, if not a draft, or later
        // via POST /GoodsReturn/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        gr.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<GoodsReturn>().Add(gr);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<GoodsReturn>()
                .Include(x => x.Vendor)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == gr.Id);

            return CreatedAtAction(nameof(GetById), new { id = gr.Id },
                _mapper.Map<GoodsReturnResponse>(draftCreated));
        }

        // Save the goods return + its Journal Entry in one transaction —
        // reverse of Goods Receipt PO: DR GR/IR Clearing / CR Inventory.
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<GoodsReturn>().Add(gr);
            await _db.SaveChangesAsync();           // gr.Id populated

            (jeId, jeNo) = await PostGoodsReturnAsync(gr, vendor);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == gr.Id);

        var response = _mapper.Map<GoodsReturnResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;

        return CreatedAtAction(nameof(GetById), new { id = gr.Id }, response);
    }

    // ── POST /GoodsReturn/{id}/Post ─── promotes a Draft return to Open,
    // running the same inventory-out + Journal Entry posting Create runs for
    // a non-draft return.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        if (gr.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {gr.Status}).");

        var gate = await _approvalGate.EvaluateAsync("GoodsReturn", gr.Id, gr.DocNum, gr.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            gr.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = gr.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostGoodsReturnAsync(gr, gr.Vendor);
            gr.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "Goods Return posted", docNum = gr.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /GoodsReturn/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        if (gr.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {gr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("GoodsReturn", gr.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostGoodsReturnAsync(gr, gr.Vendor);
            gr.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "Goods Return approved and posted", docNum = gr.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /GoodsReturn/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var gr = await _db.Set<GoodsReturn>().FindAsync(id);
        if (gr == null) return NotFound();
        if (gr.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {gr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("GoodsReturn", gr.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        gr.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Return rejected", docNum = gr.DocNum });
    }

    /// <summary>
    /// Creates the Goods Return's Journal Entry and posts stock out for
    /// every line (goods physically go back to the vendor). Shared by
    /// Create (non-draft) and Post (draft → open) so both post identically.
    /// </summary>
    private async Task<(int? jeId, string? jeNo)> PostGoodsReturnAsync(GoodsReturn gr, BPEntity vendor)
    {
        var je = _journalService.CreateJournalEntry(gr, vendor);
        je.BaseDocEntry = gr.Id;

        var stockDate = gr.PostingDate ?? DateTime.UtcNow;
        foreach (var line in gr.Items)
        {
            await _inventoryPosting.PostOutAsync(
                line.ItemCode!, line.WhsCode!, line.Quantity,
                "Goods Return", "GoodsReturn", gr.Id, gr.DocNum, stockDate,
                vendor.Code, vendor.CardName, batches: line.Batches, allowExpired: true);
        }

        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReturnUpdateRequest dto)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (gr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(gr.Items);
        _mapper.Map(dto, gr);
        foreach (var line in gr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new GoodsReturnPdfDocument(gr, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{gr.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gr = await _db.Set<GoodsReturn>().FindAsync(id);
        if (gr == null) return NotFound();
        _db.Set<GoodsReturn>().Remove(gr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
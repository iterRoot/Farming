using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Administration.DocumentNumberRange;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.PurchaseAP.LandingCost;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

[ApiController]
[Route("[controller]")]   // ✅ route = /GoodsReceiptPO
public class GoodsReceiptPOController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IGoodsReceiptPOJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;
    private readonly IDocumentNumberRangeRepository _docNums;

    public GoodsReceiptPOController(
        MyDbContext db,
        IMapper mapper,
        IGoodsReceiptPOJournalService journalService,
        IInventoryPostingService inventoryPosting,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout,
        IDocumentNumberRangeRepository docNums)
    {
        _docNums = docNums;
        _db = db; _mapper = mapper; _journalService = journalService;
        _inventoryPosting = inventoryPosting;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /GoodsReceiptPO ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<GoodsReceiptPOResponse>>(list));
    }

    // ── GET /GoodsReceiptPO/{id} ────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReceiptPOResponse>(gr));
    }

    // ── POST /GoodsReceiptPO ────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReceiptPORequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var gr = _mapper.Map<GoodsReceiptPO>(dto);

        // The screen reserves a number via /DocumentNumberRange/Next and sends it;
        // any other caller sends none, and DocNum has a unique index, so assign one.
        if (string.IsNullOrWhiteSpace(gr.DocNum))
        {
            try { gr.DocNum = _docNums.GenerateNextNumber("GoodsReceiptPO").DocNo; }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // LineNum has a unique (GoodsReceiptPOId, LineNum) index but the request
        // DTO carries no such field — every line defaulted to 0 and any receipt
        // with more than one line crashed on save. Assign it here, same as
        // ApprovalTemplate does for its own Parameters collection.
        int lineNum = 1;
        foreach (var line in gr.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
            line.LineNum  = lineNum++;
        }

        // A Draft receipt is saved as-is, with no stock/GL movement yet —
        // those only happen when it's posted (here, if not a draft, or later
        // via POST /GoodsReceiptPO/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        gr.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<GoodsReceiptPO>().Add(gr);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<GoodsReceiptPO>()
                .Include(x => x.Vendor)
                .Include(x => x.Lines).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == gr.Id);

            return CreatedAtAction(nameof(GetById), new { id = gr.Id },
                new { goodsReceiptPO = _mapper.Map<GoodsReceiptPOResponse>(draftCreated) });
        }

        // ══════════════════════════════════════════════════════════
        // BEGIN TRANSACTION — Goods Receipt PO + Journal Entry
        // If ANYTHING fails, BOTH are rolled back.
        // ══════════════════════════════════════════════════════════
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // ── Step 1: Save the goods receipt ────────────────────
            _db.Set<GoodsReceiptPO>().Add(gr);
            await _db.SaveChangesAsync();
            // gr.Id is now populated

            var (je, _, _) = await PostGoodsReceiptAsync(gr, vendor);

            // ── Step 4: Commit — both GRPO + JE are final ─────────
            await transaction.CommitAsync();

            var created = await _db.Set<GoodsReceiptPO>()
                .Include(x => x.Vendor)
                .Include(x => x.Lines).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == gr.Id);

            return CreatedAtAction(nameof(GetById), new { id = gr.Id }, new
            {
                goodsReceiptPO = _mapper.Map<GoodsReceiptPOResponse>(created),
                journalEntryId = je.Id,
                journalNo      = je.JrnlNo,
            });
        }
        catch (InvalidOperationException ex)
        {
            // ── ROLLBACK — neither GRPO nor JE is saved ───────────
            await transaction.RollbackAsync();

            return BadRequest(new
            {
                error   = "Goods Receipt PO creation failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure InventoryAccount and InventoryOffset."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                error   = "Unexpected error — transaction rolled back",
                details = ex.Message,
            });
        }
    }

    // ── POST /GoodsReceiptPO/{id}/Post ─── promotes a Draft receipt to
    // Open, running the same inventory-in + Journal Entry posting Create
    // runs for a non-draft receipt.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        if (gr.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {gr.Status}).");

        var gate = await _approvalGate.EvaluateAsync("GoodsReceiptPO", gr.Id, gr.DocNum, gr.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            gr.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = gr.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var (je, jeId, jeNo) = await PostGoodsReceiptAsync(gr, gr.Vendor);
            gr.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Goods Receipt PO posted", docNum = gr.DocNum, journalEntryId = jeId, journalNo = jeNo });
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new
            {
                error   = "Goods Receipt PO posting failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure InventoryAccount and InventoryOffset."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = "Unexpected error — transaction rolled back", details = ex.Message });
        }
    }

    // ── POST /GoodsReceiptPO/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        if (gr.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {gr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("GoodsReceiptPO", gr.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var (je, jeId, jeNo) = await PostGoodsReceiptAsync(gr, gr.Vendor);
            gr.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "Goods Receipt PO approved and posted", docNum = gr.DocNum, journalEntryId = jeId, journalNo = jeNo });
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new
            {
                error   = "Goods Receipt PO posting failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure InventoryAccount and InventoryOffset."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = "Unexpected error — transaction rolled back", details = ex.Message });
        }
    }

    // ── POST /GoodsReceiptPO/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var gr = await _db.Set<GoodsReceiptPO>().FindAsync(id);
        if (gr == null) return NotFound();
        if (gr.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {gr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("GoodsReceiptPO", gr.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        gr.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Receipt PO rejected", docNum = gr.DocNum });
    }

    /// <summary>
    /// Creates the Goods Receipt PO's Journal Entry and posts stock in for
    /// every line at the default warehouse, at the line's price. Shared by
    /// Create (non-draft) and Post (draft → open) so both post identically.
    /// </summary>
    private async Task<(JournalEntry je, int? jeId, string? jeNo)> PostGoodsReceiptAsync(GoodsReceiptPO gr, BPEntity vendor)
    {
        var je = _journalService.CreateJournalEntry(gr, vendor);
        je.BaseDocEntry = gr.Id;

        var whsCode = await _inventoryPosting.GetDefaultWarehouseCodeAsync();
        var stockDate = gr.PostingDate ?? gr.DocDate ?? DateTime.UtcNow;
        foreach (var line in gr.Lines)
        {
            await _inventoryPosting.PostInAsync(
                line.ItemCode!, whsCode, line.Quantity, line.Price,
                "Goods Receipt PO", "GoodsReceiptPO", gr.Id, gr.DocNum, stockDate,
                vendor.Code, vendor.CardName, batches: line.Batches);
        }

        await _db.SaveChangesAsync();
        return (je, je.Id, je.JrnlNo);
    }

    // ── PUT /GoodsReceiptPO/{id} ────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReceiptPOUpdateRequest dto)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(gr.Lines);
        _mapper.Map(dto, gr);

        int lineNum = 1;
        foreach (var line in gr.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
            line.LineNum  = lineNum++;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /GoodsReceiptPO/{id}/Pdf ─────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("GoodsReceiptPO", gr, gr.Lines);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new GoodsReceiptPOPdfDocument(gr, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{gr.DocNum}.pdf");
    }

    // ── DELETE /GoodsReceiptPO/{id} ─────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>().FindAsync(id);
        if (gr == null) return NotFound();

        // LandedCostBaseDoc holds a Restrict-behavior FK to this receipt (added
        // with the Landed Cost feature) — deleting a receipt a landed cost has
        // already allocated across would otherwise throw an unhandled
        // DbUpdateException, the same bug class fixed on User/ApprovalProcess delete.
        var hasLandedCost = await _db.Set<LandedCostBaseDoc>().AnyAsync(x => x.GoodsReceiptPOId == id);
        if (hasLandedCost)
            return BadRequest("Cannot delete a receipt that a Landed Cost has allocated across — delete the Landed Cost first.");

        _db.Set<GoodsReceiptPO>().Remove(gr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;  // ✅ your namespace
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using FarmingApi.Services;
using QuestPDF.Fluent;

using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.Delivery;

[ApiController]
[Route("[controller]")]         // ✅ route = /Delivery
public class DeliveryController : ControllerBase  // ✅ DeliveryController
{
    private readonly MyDbContext            _db;
    private readonly IMapper                _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IDeliveryJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IApprovalGateService   _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public DeliveryController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IDeliveryJournalService journalService,
        IInventoryPostingService inventoryPosting,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
        _journalService = journalService;
        _inventoryPosting = inventoryPosting;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /Delivery ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var responses = _mapper.Map<List<DeliveryListResponse>>(list);

        // Attach the auto-created Journal Entry (KADV) to each delivery.
        var ids = list.Select(d => d.Id).ToList();
        var jeMap = await _db.Set<JournalEntry>()
            .Where(j => j.BaseDocType == "KADV" && j.BaseDocEntry != null && ids.Contains(j.BaseDocEntry.Value))
            .ToDictionaryAsync(j => j.BaseDocEntry!.Value, j => new { j.Id, j.JrnlNo });
        foreach (var r in responses)
            if (jeMap.TryGetValue(r.Id, out var je)) { r.JournalEntryId = je.Id; r.JournalNo = je.JrnlNo; }

        return Ok(responses);
    }

    // ── GET /Delivery/{id} ────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();

        var response = _mapper.Map<DeliveryListResponse>(delivery);
        var je = await _db.Set<JournalEntry>()
            .FirstOrDefaultAsync(j => j.BaseDocType == "KADV" && j.BaseDocEntry == id);
        response.JournalEntryId = je?.Id;
        response.JournalNo       = je?.JrnlNo;

        return Ok(response);
    }

    // ── POST /Delivery ────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DeliveryListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var delivery = _mapper.Map<Delivery>(dto);

        // Auto-assign the document number (server-side, race-safe)
        try { delivery.DocNum = _docNumber.Next("Delivery"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in delivery.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            // Blank warehouse falls back to the default; an unknown one is rejected.
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // A Draft delivery is saved as-is, with no stock/GL movement yet — those
        // only happen when it's posted (either here, if not a draft, or later
        // via POST /Delivery/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        delivery.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<Delivery>().Add(delivery);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<Delivery>()
                .Include(x => x.Customer)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == delivery.Id);

            return CreatedAtAction(nameof(GetById), new { id = delivery.Id },
                _mapper.Map<DeliveryListResponse>(draftCreated));
        }

        // Save the delivery and its inventory Journal Entry in one transaction —
        // DR Cost of Goods Sold, CR Inventory (at item cost).
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<Delivery>().Add(delivery);
            await _db.SaveChangesAsync();           // delivery.Id populated

            (jeId, jeNo) = await PostDeliveryAsync(delivery, customer);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == delivery.Id);

        var response = _mapper.Map<DeliveryListResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;

        return CreatedAtAction(nameof(GetById), new { id = delivery.Id }, response);
    }

    // ── POST /Delivery/{id}/Post ─── promotes a Draft delivery to Open,
    // running the same inventory-out + Journal Entry posting Create runs for
    // a non-draft delivery.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();
        if (delivery.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {delivery.Status}).");

        var gate = await _approvalGate.EvaluateAsync("Delivery", delivery.Id, delivery.DocNum, delivery.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            delivery.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = delivery.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostDeliveryAsync(delivery, delivery.Customer);
            delivery.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "Delivery posted", docNum = delivery.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /Delivery/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();
        if (delivery.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {delivery.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("Delivery", delivery.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostDeliveryAsync(delivery, delivery.Customer);
            delivery.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "Delivery approved and posted", docNum = delivery.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /Delivery/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var delivery = await _db.Set<Delivery>().FindAsync(id);
        if (delivery == null) return NotFound();
        if (delivery.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {delivery.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("Delivery", delivery.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        delivery.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Delivery rejected", docNum = delivery.DocNum });
    }

    /// <summary>
    /// Posts stock out of each line's warehouse at moving-average cost and
    /// creates the DR-COGS/CR-Inventory Journal Entry. Shared by Create
    /// (non-draft) and Post (draft → open) so both post identically.
    /// </summary>
    private async Task<(int? jeId, string? jeNo)> PostDeliveryAsync(Delivery delivery, BPEntity customer)
    {
        var je = _journalService.CreateJournalEntry(delivery);
        if (je != null) je.BaseDocEntry = delivery.Id;

        var stockDate = delivery.PostingDate ?? delivery.DeliveryDate ?? DateTime.UtcNow;
        foreach (var line in delivery.Items)
        {
            await _inventoryPosting.PostOutAsync(
                line.ItemCode!, line.WhsCode!, line.Quantity,
                "Delivery", "Delivery", delivery.Id, delivery.DocNum, stockDate,
                customer.Code, customer.CardName, batches: line.Batches);
        }

        await _db.SaveChangesAsync();           // assigns je.Id

        return je != null ? (je.Id, je.JrnlNo) : (null, null);
    }

    // ── PUT /Delivery/{id} ────────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DeliveryUpdateRequest dto)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(delivery.Items);
        _mapper.Map(dto, delivery);

        foreach (var line in delivery.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /Delivery/{id}/Pdf ────────────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("Delivery", delivery, delivery.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new DeliveryPdfDocument(delivery, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{delivery.DocNum}.pdf");
    }

    // ── DELETE /Delivery/{id} ─────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var delivery = await _db.Set<Delivery>().FindAsync(id);
        if (delivery == null) return NotFound();

        _db.Set<Delivery>().Remove(delivery);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
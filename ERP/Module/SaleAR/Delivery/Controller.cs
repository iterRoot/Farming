using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
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

    public DeliveryController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IDeliveryJournalService journalService,
        IInventoryPostingService inventoryPosting)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
        _journalService = journalService;
        _inventoryPosting = inventoryPosting;
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

        // Save the delivery and its inventory Journal Entry in one transaction —
        // DR Cost of Goods Sold, CR Inventory (at item cost).
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<Delivery>().Add(delivery);
            await _db.SaveChangesAsync();           // delivery.Id populated

            var je = _journalService.CreateJournalEntry(delivery);
            if (je != null) je.BaseDocEntry = delivery.Id;

            // Post stock out of each line's own warehouse, at the item's
            // current moving-average cost.
            var stockDate = delivery.PostingDate ?? delivery.DeliveryDate ?? DateTime.UtcNow;
            foreach (var line in delivery.Items)
            {
                await _inventoryPosting.PostOutAsync(
                    line.ItemCode!, line.WhsCode!, line.Quantity,
                    "Delivery", "Delivery", delivery.Id, delivery.DocNum, stockDate,
                    customer.Code, customer.CardName);
            }

            await _db.SaveChangesAsync();           // assigns je.Id

            if (je != null)
            {
                jeId = je.Id;
                jeNo = je.JrnlNo;
            }

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
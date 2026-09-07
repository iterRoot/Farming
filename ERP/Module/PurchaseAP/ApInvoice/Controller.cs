using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
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

    public APInvoiceController(
        MyDbContext db,
        IMapper mapper,
        IAPInvoiceJournalService journalService,
        IInventoryPostingService inventoryPosting)
    {
        _db = db; _mapper = mapper; _journalService = journalService;
        _inventoryPosting = inventoryPosting;
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

        // Save the document + its Journal Entry in one transaction (DR Purchase Expense / DR Input VAT / CR Accounts Payable).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APInvoice>().Add(inv);
            await _db.SaveChangesAsync();
            var je = _journalService.CreateJournalEntry(inv, vendor);
            je.BaseDocEntry = inv.Id;

            // Post stock in — only for lines NOT based on a Goods Receipt PO.
            // A GRPO already received the goods, so posting here too would
            // double-count them. Lines booked directly on the invoice (a
            // purchase with no prior receipt) are the ones that bring stock in,
            // at the line price, which is the purchase cost.
            var stockDate = inv.PostingDate ?? DateTime.UtcNow;
            foreach (var line in inv.Items.Where(l => !IsBasedOnGoodsReceipt(l)))
            {
                await _inventoryPosting.PostInAsync(
                    line.ItemCode!, line.WhsCode!, line.Quantity, line.Price,
                    "AP Invoice", "ApInvoice", inv.Id, inv.DocNum, stockDate,
                    vendor.Code, vendor.CardName);
            }

            await _db.SaveChangesAsync();
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
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

namespace FarmingApi.Modules.PurchaseAP.GoodsReturn;

[ApiController]
[Route("[controller]")]
public class GoodsReturnController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IGoodsReturnJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;

    public GoodsReturnController(
        MyDbContext db,
        IMapper mapper,
        IGoodsReturnJournalService journalService,
        IInventoryPostingService inventoryPosting)
    { _db = db; _mapper = mapper; _journalService = journalService; _inventoryPosting = inventoryPosting; }

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

        // Save the goods return + its Journal Entry in one transaction —
        // reverse of Goods Receipt PO: DR GR/IR Clearing / CR Inventory.
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<GoodsReturn>().Add(gr);
            await _db.SaveChangesAsync();           // gr.Id populated

            var je = _journalService.CreateJournalEntry(gr, vendor);
            je.BaseDocEntry = gr.Id;

            // Goods go back to the vendor, so stock leaves the warehouse. This is
            // the only other Purchase A/P document that moves stock — AP Invoice
            // and AP Credit Memo are financial-only.
            var stockDate = gr.PostingDate ?? DateTime.UtcNow;
            foreach (var line in gr.Items)
            {
                await _inventoryPosting.PostOutAsync(
                    line.ItemCode!, line.WhsCode!, line.Quantity,
                    "Goods Return", "GoodsReturn", gr.Id, gr.DocNum, stockDate,
                    vendor.Code, vendor.CardName);
            }

            await _db.SaveChangesAsync();
            jeId = je.Id;
            jeNo = je.JrnlNo;

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
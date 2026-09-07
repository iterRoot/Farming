using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
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

    public APReserveInvoiceController(MyDbContext db, IMapper mapper, IAPReserveInvoiceJournalService journalService)
    { _db = db; _mapper = mapper; _journalService = journalService; }

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

        // Save the document + its Journal Entry in one transaction (DR Purchase Expense / DR Input VAT / CR Accounts Payable).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APReserveInvoice>().Add(inv);
            await _db.SaveChangesAsync();
            var je = _journalService.CreateJournalEntry(inv, vendor);
            je.BaseDocEntry = inv.Id;
            await _db.SaveChangesAsync();
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
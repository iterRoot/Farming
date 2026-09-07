using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Services;
using QuestPDF.Fluent;

// ✅ Alias for BP entity
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.ARReserveInvoice;

[ApiController]
[Route("[controller]")]
public class ARReserveInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IARReserveInvoiceJournalService _journalService;

    public ARReserveInvoiceController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IARReserveInvoiceJournalService journalService)
    {
        _db     = db;
        _mapper = mapper;
        _docNumber = docNumber;
        _journalService = journalService;
    }

    // ── GET /ARReserveInvoice ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ARReserveInvoiceResponse>>(list));
    }

    // ── GET /ARReserveInvoice/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<ARReserveInvoiceResponse>(invoice));
    }

    // ── POST /ARReserveInvoice ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARReserveInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var invoice = _mapper.Map<ARReserveInvoice>(dto);

        // Auto-assign the document number (server-side, race-safe).
        try { invoice.DocNum = _docNumber.Next("ArReserveInvoice"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // Auto-fill ItemCode / ItemName from ItemsMaster
        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // Save the invoice + its Journal Entry in one transaction —
        // DR Accounts Receivable / CR Revenue / CR Output VAT.
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<ARReserveInvoice>().Add(invoice);
            await _db.SaveChangesAsync();           // invoice.Id populated

            var je = _journalService.CreateJournalEntry(invoice, customer);
            je.BaseDocEntry = invoice.Id;
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

        var created = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == invoice.Id);

        var response = _mapper.Map<ARReserveInvoiceResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, response);
    }

    // ── PUT /ARReserveInvoice/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARReserveInvoiceUpdateRequest dto)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();

        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(invoice.Items);
        _mapper.Map(dto, invoice);

        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /ARReserveInvoice/{id}/Pdf ────────────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ARReserveInvoicePdfDocument(invoice, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{invoice.DocNum}.pdf");
    }

    // ── DELETE /ARReserveInvoice/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();

        _db.Set<ARReserveInvoice>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
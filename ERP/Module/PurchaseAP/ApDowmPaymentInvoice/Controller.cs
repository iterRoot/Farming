using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

[ApiController]
[Route("[controller]")]   // ✅ route = /APDownPaymentInvoice
public class APDownPaymentInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IAPDownPaymentInvoiceJournalService _journalService;

    public APDownPaymentInvoiceController(MyDbContext db, IMapper mapper, IAPDownPaymentInvoiceJournalService journalService)
    {
        _db = db; _mapper = mapper; _journalService = journalService;
    }

    // ── GET /APDownPaymentInvoice ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<APDownPaymentInvoiceResponse>>(list));
    }

    // ── GET /APDownPaymentInvoice/{id} ────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        return Ok(_mapper.Map<APDownPaymentInvoiceResponse>(inv));
    }

    // ── POST /APDownPaymentInvoice ────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APDownPaymentInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var inv = _mapper.Map<APDownPaymentInvoice>(dto);

        // Save the document + its Journal Entry in one transaction (DR Vendor Deposit / CR Accounts Payable).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APDownPaymentInvoice>().Add(inv);
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

        var created = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstAsync(x => x.Id == inv.Id);

        return CreatedAtAction(nameof(GetById), new { id = inv.Id },
            _mapper.Map<APDownPaymentInvoiceResponse>(created));
    }

    // ── PUT /APDownPaymentInvoice/{id} ────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APDownPaymentInvoiceUpdateRequest dto)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _mapper.Map(dto, inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /APDownPaymentInvoice/{id}/Pdf ────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APDownPaymentInvoicePdfDocument(inv, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{inv.DocNum}.pdf");
    }

    // ── DELETE /APDownPaymentInvoice/{id} ─────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        _db.Set<APDownPaymentInvoice>().Remove(inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
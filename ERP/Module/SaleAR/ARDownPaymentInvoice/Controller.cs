using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Services;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.ARDownPaymentInvoice;

[ApiController]
[Route("[controller]")]          // ✅ route = /ARDownPaymentInvoice
public class ARDownPaymentInvoiceController : ControllerBase  // ✅ renamed
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IARDonwPaymentInvoiceJournalService _journalService;

    public ARDownPaymentInvoiceController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IARDonwPaymentInvoiceJournalService journalService)
    {
        _db = db; _mapper = mapper; _docNumber = docNumber; _journalService = journalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<ARDownPaymentListResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dp == null) return NotFound();
        return Ok(_mapper.Map<ARDownPaymentListResponse>(dp));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARDownPaymentListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var dp = _mapper.Map<ARDownPaymentInvoice>(dto);

        // Auto-assign the document number (server-side, race-safe).
        try { dp.DocNum = _docNumber.Next("ArDownPaymentInvoice"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in dp.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // Save the invoice + its Journal Entry in one transaction —
        // DR Accounts Receivable / CR Customer Deposit (down payment on account).
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<ARDownPaymentInvoice>().Add(dp);
            await _db.SaveChangesAsync();           // dp.Id populated

            var je = _journalService.CreateJournalEntry(dp, customer);
            je.BaseDocEntry = dp.Id;
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

        var created = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == dp.Id);

        var response = _mapper.Map<ARDownPaymentListResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;

        return CreatedAtAction(nameof(GetById), new { id = dp.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARDownPaymentUpdateRequest dto)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (dp == null) return NotFound();
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        _db.RemoveRange(dp.Items);
        _mapper.Map(dto, dp);
        foreach (var line in dp.Items)
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
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ARDownPaymentInvoicePdfDocument(dp, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{dp.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().FindAsync(id);
        if (dp == null) return NotFound();
        _db.Set<ARDownPaymentInvoice>().Remove(dp);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
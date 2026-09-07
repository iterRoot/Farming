using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APCreditNote;

[ApiController]
[Route("[controller]")]
public class APCreditNoteController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IAPCreditNoteJournalService _journalService;

    public APCreditNoteController(MyDbContext db, IMapper mapper, IAPCreditNoteJournalService journalService)
    { _db = db; _mapper = mapper; _journalService = journalService; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<APCreditNoteResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();
        return Ok(_mapper.Map<APCreditNoteResponse>(cn));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APCreditNoteRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var cn = _mapper.Map<APCreditNote>(dto);
        foreach (var line in cn.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // Save the document + its Journal Entry in one transaction (reverse of AP Invoice).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APCreditNote>().Add(cn);
            await _db.SaveChangesAsync();
            var je = _journalService.CreateJournalEntry(cn, vendor);
            je.BaseDocEntry = cn.Id;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == cn.Id);
        return CreatedAtAction(nameof(GetById), new { id = cn.Id },
            _mapper.Map<APCreditNoteResponse>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APCreditNoteUpdateRequest dto)
    {
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(cn.Items);
        _mapper.Map(dto, cn);
        foreach (var line in cn.Items)
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
        var cn = await _db.Set<APCreditNote>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cn == null) return NotFound();

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APCreditNotePdfDocument(cn, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{cn.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cn = await _db.Set<APCreditNote>().FindAsync(id);
        if (cn == null) return NotFound();
        _db.Set<APCreditNote>().Remove(cn);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
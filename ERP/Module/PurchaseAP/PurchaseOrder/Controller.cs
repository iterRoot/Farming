using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using QuestPDF.Fluent;
using FarmingApi.Modules.Administration.DocumentNumberRange;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.PurchaseOrder;

[ApiController]
[Route("[controller]")]   // ✅ route = /PurchaseOrder
public class PurchaseOrderController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberRangeRepository _docNums;

    public PurchaseOrderController(MyDbContext db, IMapper mapper,
                                   IDocumentNumberRangeRepository docNums)
    {
        _db = db; _mapper = mapper; _docNums = docNums;
    }

    // ── GET /PurchaseOrder ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<PurchaseOrder>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<PurchaseOrderResponse>>(list));
    }

    // ── GET /PurchaseOrder/{id} ────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var po = await _db.Set<PurchaseOrder>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (po == null) return NotFound();
        return Ok(_mapper.Map<PurchaseOrderResponse>(po));
    }

    // ── POST /PurchaseOrder ────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseOrderRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var po = _mapper.Map<PurchaseOrder>(dto);

        // The document number is assigned here, never taken from the client.
        // The screen only ever shows a peeked preview; reserving on save is
        // what stops two orders claiming the same number.
        try
        {
            po.DocNum = _docNums.GenerateNextNumber("PurchaseOrder").DocNo;
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        foreach (var line in po.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<PurchaseOrder>().Add(po);
        await _db.SaveChangesAsync();

        var created = await _db.Set<PurchaseOrder>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == po.Id);

        return CreatedAtAction(nameof(GetById), new { id = po.Id },
            _mapper.Map<PurchaseOrderResponse>(created));
    }

    // ── PUT /PurchaseOrder/{id} ────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseOrderUpdateRequest dto)
    {
        var po = await _db.Set<PurchaseOrder>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (po == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(po.Lines);
        _mapper.Map(dto, po);

        foreach (var line in po.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /PurchaseOrder/{id}/Pdf ─────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var po = await _db.Set<PurchaseOrder>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (po == null) return NotFound();

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new PurchaseOrderPdfDocument(po, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{po.DocNum}.pdf");
    }

    // ── DELETE /PurchaseOrder/{id} ─────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var po = await _db.Set<PurchaseOrder>().FindAsync(id);
        if (po == null) return NotFound();
        _db.Set<PurchaseOrder>().Remove(po);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
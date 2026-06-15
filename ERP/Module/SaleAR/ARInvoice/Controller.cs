using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Services;

// ✅ Alias for BP entity
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

[ApiController]
[Route("[controller]")]
public class ARInvoiceController : ControllerBase
{
    private readonly MyDbContext           _db;
    private readonly IMapper               _mapper;
    private readonly IDocumentNumberService _docNumber; // ✅ auto-numbering

    public ARInvoiceController(
        MyDbContext            db,
        IMapper                mapper,
        IDocumentNumberService docNumber)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
    }

    // ── GET /ARInvoice ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ARInvoiceListResponse>>(list));
    }

    // ── GET /ARInvoice/{id} ───────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<ARInvoiceListResponse>(invoice));
    }

    // ── POST /ARInvoice ───────────────────────────────────────────
    // ✅ DocNum auto-assigned from DocumentNumberRange
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARInvoiceListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        // ✅ Auto-number
        string docNum;
        try { docNum = _docNumber.Next("ArInvoice"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        var invoice    = _mapper.Map<ARInvoice>(dto);
        invoice.DocNum = docNum;   // ✅ override whatever came from request

        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<ARInvoice>().Add(invoice);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == invoice.Id);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
            _mapper.Map<ARInvoiceListResponse>(created));
    }

    // ── PUT /ARInvoice/{id} ───────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARInvoiceUpdateRequest dto)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (invoice == null) return NotFound();

        if (invoice.Status == "C")
            return BadRequest("Cannot edit a Closed AR Invoice");

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var existingDocNum = invoice.DocNum;   // ✅ preserve original DocNum
        _db.RemoveRange(invoice.Items);
        _mapper.Map(dto, invoice);
        invoice.DocNum = existingDocNum;        // ✅ never overwrite

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

    // ── POST /ARInvoice/{id}/Close ────────────────────────────────
    // ✅ Called when AR Credit Note is created from this invoice
    [HttpPost("{id:int}/Close")]
    public async Task<IActionResult> Close(int id)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == "C") return BadRequest("AR Invoice already closed");

        invoice.Status    = "C";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "AR Invoice closed", docNum = invoice.DocNum });
    }

    // ── DELETE /ARInvoice/{id} ────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == "C")
            return BadRequest("Cannot delete a Closed AR Invoice");

        _db.Set<ARInvoice>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
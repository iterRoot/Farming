using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;

// ✅ Alias for BP entity
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

[ApiController]
[Route("[controller]")]
public class ARInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ARInvoiceController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /ARInvoice ────────────────────────────────────────────────────
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

    // ── GET /ARInvoice/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<ARInvoiceListResponse>(invoice));
    }

    // ── POST /ARInvoice ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARInvoiceListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var invoice = _mapper.Map<ARInvoice>(dto);

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

        _db.Set<ARInvoice>().Add(invoice);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == invoice.Id);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
            _mapper.Map<ARInvoiceListResponse>(created));
    }

    // ── PUT /ARInvoice/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARInvoiceUpdateRequest dto)
    {
        var invoice = await _db.Set<ARInvoice>()
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

    // ── DELETE /ARInvoice/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();

        _db.Set<ARInvoice>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
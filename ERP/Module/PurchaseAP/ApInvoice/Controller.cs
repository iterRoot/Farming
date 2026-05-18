using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APInvoice;

[ApiController]
[Route("[controller]")]   // ✅ route = /APInvoice
public class APInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public APInvoiceController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
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
        }

        _db.Set<APInvoice>().Add(inv);
        await _db.SaveChangesAsync();

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
        }

        await _db.SaveChangesAsync();
        return NoContent();
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
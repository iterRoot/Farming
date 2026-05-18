using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.PurchaseQuotation;

[ApiController]
[Route("[controller]")]   // ✅ route = /PurchaseQuotation
public class PurchaseQuotationController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public PurchaseQuotationController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    // ── GET /PurchaseQuotation ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<PurchaseQuotation>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<PurchaseQuotationResponse>>(list));
    }

    // ── GET /PurchaseQuotation/{id} ────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pq = await _db.Set<PurchaseQuotation>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pq == null) return NotFound();
        return Ok(_mapper.Map<PurchaseQuotationResponse>(pq));
    }

    // ── POST /PurchaseQuotation ────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseQuotationRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var pq = _mapper.Map<PurchaseQuotation>(dto);

        foreach (var line in pq.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<PurchaseQuotation>().Add(pq);
        await _db.SaveChangesAsync();

        var created = await _db.Set<PurchaseQuotation>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == pq.Id);

        return CreatedAtAction(nameof(GetById), new { id = pq.Id },
            _mapper.Map<PurchaseQuotationResponse>(created));
    }

    // ── PUT /PurchaseQuotation/{id} ────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseQuotationUpdateRequest dto)
    {
        var pq = await _db.Set<PurchaseQuotation>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pq == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(pq.Items);
        _mapper.Map(dto, pq);

        foreach (var line in pq.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /PurchaseQuotation/{id} ─────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pq = await _db.Set<PurchaseQuotation>().FindAsync(id);
        if (pq == null) return NotFound();
        _db.Set<PurchaseQuotation>().Remove(pq);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
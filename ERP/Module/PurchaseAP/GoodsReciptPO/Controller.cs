using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

[ApiController]
[Route("[controller]")]   // ✅ route = /GoodsReceiptPO
public class GoodsReceiptPOController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public GoodsReceiptPOController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    // ── GET /GoodsReceiptPO ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<GoodsReceiptPOResponse>>(list));
    }

    // ── GET /GoodsReceiptPO/{id} ────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReceiptPOResponse>(gr));
    }

    // ── POST /GoodsReceiptPO ────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReceiptPORequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var gr = _mapper.Map<GoodsReceiptPO>(dto);

        foreach (var line in gr.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<GoodsReceiptPO>().Add(gr);
        await _db.SaveChangesAsync();

        var created = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == gr.Id);

        return CreatedAtAction(nameof(GetById), new { id = gr.Id },
            _mapper.Map<GoodsReceiptPOResponse>(created));
    }

    // ── PUT /GoodsReceiptPO/{id} ────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReceiptPOUpdateRequest dto)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(gr.Lines);
        _mapper.Map(dto, gr);

        foreach (var line in gr.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /GoodsReceiptPO/{id} ─────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>().FindAsync(id);
        if (gr == null) return NotFound();
        _db.Set<GoodsReceiptPO>().Remove(gr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
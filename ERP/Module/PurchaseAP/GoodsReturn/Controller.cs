using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReturn;

[ApiController]
[Route("[controller]")]
public class GoodsReturnController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public GoodsReturnController(MyDbContext db, IMapper mapper)
    { _db = db; _mapper = mapper; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<GoodsReturnResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (gr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReturnResponse>(gr));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReturnRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var gr = _mapper.Map<GoodsReturn>(dto);
        foreach (var line in gr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<GoodsReturn>().Add(gr);
        await _db.SaveChangesAsync();

        var created = await _db.Set<GoodsReturn>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == gr.Id);
        return CreatedAtAction(nameof(GetById), new { id = gr.Id },
            _mapper.Map<GoodsReturnResponse>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReturnUpdateRequest dto)
    {
        var gr = await _db.Set<GoodsReturn>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (gr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(gr.Items);
        _mapper.Map(dto, gr);
        foreach (var line in gr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gr = await _db.Set<GoodsReturn>().FindAsync(id);
        if (gr == null) return NotFound();
        _db.Set<GoodsReturn>().Remove(gr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
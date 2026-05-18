using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReturnRequest;

[ApiController]
[Route("[controller]")]
public class GoodsReturnRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public GoodsReturnRequestController(MyDbContext db, IMapper mapper)
    { _db = db; _mapper = mapper; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<GoodsReturnRequestResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var grr = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (grr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReturnRequestResponse>(grr));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReturnRequestRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var grr = _mapper.Map<GoodsReturnRequest>(dto);
        foreach (var line in grr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<GoodsReturnRequest>().Add(grr);
        await _db.SaveChangesAsync();

        var created = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == grr.Id);
        return CreatedAtAction(nameof(GetById), new { id = grr.Id },
            _mapper.Map<GoodsReturnRequestResponse>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReturnRequestUpdateRequest dto)
    {
        var grr = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (grr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(grr.Items);
        _mapper.Map(dto, grr);
        foreach (var line in grr.Items)
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
        var grr = await _db.Set<GoodsReturnRequest>().FindAsync(id);
        if (grr == null) return NotFound();
        _db.Set<GoodsReturnRequest>().Remove(grr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
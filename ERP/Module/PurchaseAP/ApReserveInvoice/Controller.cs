using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APReserveInvoice;

[ApiController]
[Route("[controller]")]
public class APReserveInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public APReserveInvoiceController(MyDbContext db, IMapper mapper)
    { _db = db; _mapper = mapper; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<APReserveInvoiceResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inv = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (inv == null) return NotFound();
        return Ok(_mapper.Map<APReserveInvoiceResponse>(inv));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APReserveInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var inv = _mapper.Map<APReserveInvoice>(dto);
        foreach (var line in inv.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<APReserveInvoice>().Add(inv);
        await _db.SaveChangesAsync();

        var created = await _db.Set<APReserveInvoice>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == inv.Id);
        return CreatedAtAction(nameof(GetById), new { id = inv.Id },
            _mapper.Map<APReserveInvoiceResponse>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APReserveInvoiceUpdateRequest dto)
    {
        var inv = await _db.Set<APReserveInvoice>()
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inv = await _db.Set<APReserveInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        _db.Set<APReserveInvoice>().Remove(inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
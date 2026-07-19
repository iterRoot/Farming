using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.SaleAR.SaleQuotion;

[ApiController]
[Route("[controller]")]        // /SaleQuotion  (legacy spelling)
[Route("SaleQuotation")]       // /SaleQuotation (alias used by the Sale-AR UI)
public class SaleQuotionController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public SaleQuotionController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /SaleQuotion ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleQuotionListResponse>>(list));
    }

    // ── GET /SaleQuotion/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();
        return Ok(_mapper.Map<SaleQuotionListResponse>(order));
    }

    // ── POST /SaleQuotion ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleQuotionListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var order = _mapper.Map<SaleQuotion>(dto);

        // Auto-fill ItemCode / ItemName from ItemsMaster
        foreach (var line in order.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<SaleQuotion>().Add(order);
        await _db.SaveChangesAsync();

        var created = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            _mapper.Map<SaleQuotionListResponse>(created));
    }

    // ── PUT /SaleQuotion/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleQuotionUpdateRequest dto)
    {
        var order = await _db.Set<SaleQuotion>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();

        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(order.Items);
        _mapper.Map(dto, order);

        foreach (var line in order.Items)
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

    // ── DELETE /SaleQuotion/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Set<SaleQuotion>().FindAsync(id);
        if (order == null) return NotFound();

        _db.Set<SaleQuotion>().Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
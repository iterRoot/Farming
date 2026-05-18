using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;  // ✅ your namespace

using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.Return;

[ApiController]
[Route("[controller]")]         // ✅ route = /Return
public class ReturnController : ControllerBase  // ✅ ReturnController
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ReturnController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /Return ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ReturnListResponse>>(list));
    }

    // ── GET /Return/{id} ────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();
        return Ok(_mapper.Map<ReturnListResponse>(Return));
    }

    // ── POST /Return ────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var Return = _mapper.Map<Return>(dto);

        foreach (var line in Return.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<Return>().Add(Return);
        await _db.SaveChangesAsync();

        var created = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == Return.Id);

        return CreatedAtAction(nameof(GetById), new { id = Return.Id },
            _mapper.Map<ReturnListResponse>(created));
    }

    // ── PUT /Return/{id} ────────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReturnUpdateRequest dto)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(Return.Items);
        _mapper.Map(dto, Return);

        foreach (var line in Return.Items)
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

    // ── DELETE /Return/{id} ─────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var Return = await _db.Set<Return>().FindAsync(id);
        if (Return == null) return NotFound();

        _db.Set<Return>().Remove(Return);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
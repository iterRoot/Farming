using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ArDownPaymentRequest;

[ApiController]
[Route("[controller]")]   // ✅ route = /ArDownPaymentRequest
public class ArDownPaymentRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ArDownPaymentRequestController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    // ── GET /ArDownPaymentRequest ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ArDownPaymentRequestResponse>>(list));
    }

    // ── GET /ArDownPaymentRequest/{id} ────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();
        return Ok(_mapper.Map<ArDownPaymentRequestResponse>(dp));
    }

    // ── POST /ArDownPaymentRequest ────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArDownPaymentRequestRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var dp = _mapper.Map<ArDownPaymentRequest>(dto);

        foreach (var line in dp.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<ArDownPaymentRequest>().Add(dp);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == dp.Id);

        return CreatedAtAction(nameof(GetById), new { id = dp.Id },
            _mapper.Map<ArDownPaymentRequestResponse>(created));
    }

    // ── PUT /ArDownPaymentRequest/{id} ────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ArDownPaymentRequestUpdateRequest dto)
    {
        var dp = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();

        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        _db.RemoveRange(dp.Items);
        _mapper.Map(dto, dp);

        foreach (var line in dp.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /ArDownPaymentRequest/{id} ─────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>().FindAsync(id);
        if (dp == null) return NotFound();
        _db.Set<ArDownPaymentRequest>().Remove(dp);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;

// ✅ Alias for BP entity
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ReturnRequest;

[ApiController]
[Route("[controller]")]
public class ReturnRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ReturnRequestController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /ReturnRequest ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ReturnRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ReturnRequestListResponse>>(list));
    }

    // ── GET /ReturnRequest/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ReturnRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<ReturnRequestListResponse>(invoice));
    }

    // ── POST /ReturnRequest ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnRequestListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var invoice = _mapper.Map<ReturnRequest>(dto);

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

        _db.Set<ReturnRequest>().Add(invoice);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ReturnRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == invoice.Id);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
            _mapper.Map<ReturnRequestListResponse>(created));
    }

    // ── PUT /ReturnRequest/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReturnRequestUpdateRequest dto)
    {
        var invoice = await _db.Set<ReturnRequest>()
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

    // ── DELETE /ReturnRequest/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ReturnRequest>().FindAsync(id);
        if (invoice == null) return NotFound();

        _db.Set<ReturnRequest>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
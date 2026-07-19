using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;  // ✅ your namespace
using FarmingApi.Services;

using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.Delivery;

[ApiController]
[Route("[controller]")]         // ✅ route = /Delivery
public class DeliveryController : ControllerBase  // ✅ DeliveryController
{
    private readonly MyDbContext            _db;
    private readonly IMapper                _mapper;
    private readonly IDocumentNumberService _docNumber;

    public DeliveryController(MyDbContext db, IMapper mapper, IDocumentNumberService docNumber)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
    }

    // ── GET /Delivery ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<DeliveryListResponse>>(list));
    }

    // ── GET /Delivery/{id} ────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();
        return Ok(_mapper.Map<DeliveryListResponse>(delivery));
    }

    // ── POST /Delivery ────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DeliveryListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var delivery = _mapper.Map<Delivery>(dto);

        // Auto-assign the document number (server-side, race-safe)
        try { delivery.DocNum = _docNumber.Next("Delivery"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in delivery.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<Delivery>().Add(delivery);
        await _db.SaveChangesAsync();

        var created = await _db.Set<Delivery>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == delivery.Id);

        return CreatedAtAction(nameof(GetById), new { id = delivery.Id },
            _mapper.Map<DeliveryListResponse>(created));
    }

    // ── PUT /Delivery/{id} ────────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DeliveryUpdateRequest dto)
    {
        var delivery = await _db.Set<Delivery>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (delivery == null) return NotFound();

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(delivery.Items);
        _mapper.Map(dto, delivery);

        foreach (var line in delivery.Items)
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

    // ── DELETE /Delivery/{id} ─────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var delivery = await _db.Set<Delivery>().FindAsync(id);
        if (delivery == null) return NotFound();

        _db.Set<Delivery>().Remove(delivery);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
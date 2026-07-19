using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Services;

namespace FarmingApi.Modules.SaleAR.SaleOrder;

[ApiController]
[Route("[controller]")]
public class SaleOrderController : ControllerBase
{
    private readonly MyDbContext            _db;
    private readonly IMapper                _mapper;
    private readonly IDocumentNumberService _docNumber;

    public SaleOrderController(MyDbContext db, IMapper mapper, IDocumentNumberService docNumber)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
    }

    // ── GET /SaleOrder ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleOrderListResponse>>(list));
    }

    // ── GET /SaleOrder/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();
        return Ok(_mapper.Map<SaleOrderListResponse>(order));
    }

    // ── POST /SaleOrder ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleOrderListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var order = _mapper.Map<SaleOrder>(dto);

        // Auto-assign the document number (server-side, race-safe)
        try { order.DocNum = _docNumber.Next("SaleOrder"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

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

        _db.Set<SaleOrder>().Add(order);
        await _db.SaveChangesAsync();

        var created = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            _mapper.Map<SaleOrderListResponse>(created));
    }

    // ── PUT /SaleOrder/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleOrderUpdateRequest dto)
    {
        var order = await _db.Set<SaleOrder>()
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

    // ── DELETE /SaleOrder/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Set<SaleOrder>().FindAsync(id);
        if (order == null) return NotFound();

        _db.Set<SaleOrder>().Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
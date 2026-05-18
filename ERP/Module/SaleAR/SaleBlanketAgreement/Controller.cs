using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

[ApiController]
[Route("[controller]")]
public class SaleBlanketAgreementController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper _mapper;

    public SaleBlanketAgreementController(MyDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    // ── GET /SaleBlanketAgreement ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleBlanketAgreementListResponse>>(list));
    }

    // ── GET /SaleBlanketAgreement/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();
        return Ok(_mapper.Map<SaleBlanketAgreementListResponse>(order));
    }

    // ── POST /SaleBlanketAgreement ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleBlanketAgreementListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var order = _mapper.Map<SaleBlanketAgreement>(dto);

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

        _db.Set<SaleBlanketAgreement>().Add(order);
        await _db.SaveChangesAsync();

        var created = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            _mapper.Map<SaleBlanketAgreementListResponse>(created));
    }

    // ── PUT /SaleBlanketAgreement/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleBlanketAgreementUpdateRequest dto)
    {
        var order = await _db.Set<SaleBlanketAgreement>()
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

    // ── DELETE /SaleBlanketAgreement/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Set<SaleBlanketAgreement>().FindAsync(id);
        if (order == null) return NotFound();

        _db.Set<SaleBlanketAgreement>().Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
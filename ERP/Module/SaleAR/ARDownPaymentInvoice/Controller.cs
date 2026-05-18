using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARDownPaymentInvoice;

[ApiController]
[Route("[controller]")]          // ✅ route = /ARDownPaymentInvoice
public class ARDownPaymentInvoiceController : ControllerBase  // ✅ renamed
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ARDownPaymentInvoiceController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<ARDownPaymentListResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dp == null) return NotFound();
        return Ok(_mapper.Map<ARDownPaymentListResponse>(dp));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARDownPaymentListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var dp = _mapper.Map<ARDownPaymentInvoice>(dto);
        foreach (var line in dp.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }
        _db.Set<ARDownPaymentInvoice>().Add(dp);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == dp.Id);
        return CreatedAtAction(nameof(GetById), new { id = dp.Id }, _mapper.Map<ARDownPaymentListResponse>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARDownPaymentUpdateRequest dto)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().FindAsync(id);
        if (dp == null) return NotFound();
        _db.Set<ARDownPaymentInvoice>().Remove(dp);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
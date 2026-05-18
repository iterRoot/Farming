using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARCreditNote;

[ApiController]
[Route("[controller]")]          // ✅ route = /ARCreditNote
public class ARCreditNoteController : ControllerBase  // ✅ renamed
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ARCreditNoteController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<ARCreditNoteListResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cn = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();
        return Ok(_mapper.Map<ARCreditNoteListResponse>(cn));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARCreditNoteListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var cn = _mapper.Map<ARCreditNote>(dto);
        foreach (var line in cn.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }
        _db.Set<ARCreditNote>().Add(cn);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ARCreditNote>()
            .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == cn.Id);
        return CreatedAtAction(nameof(GetById), new { id = cn.Id }, _mapper.Map<ARCreditNoteListResponse>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARCreditNoteUpdateRequest dto)
    {
        var cn = await _db.Set<ARCreditNote>().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (cn == null) return NotFound();
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        _db.RemoveRange(cn.Items);
        _mapper.Map(dto, cn);
        foreach (var line in cn.Items)
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
        var cn = await _db.Set<ARCreditNote>().FindAsync(id);
        if (cn == null) return NotFound();
        _db.Set<ARCreditNote>().Remove(cn);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
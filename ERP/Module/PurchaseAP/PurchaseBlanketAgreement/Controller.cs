using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement;

[ApiController]
[Route("[controller]")]   // ✅ route = /PurchaseBlanketAgreement
public class PurchaseBlanketAgreementController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public PurchaseBlanketAgreementController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    // ── GET /PurchaseBlanketAgreement ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<PurchaseBlanketAgreement>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<PurchaseBlanketAgreementResponse>>(list));
    }

    // ── GET /PurchaseBlanketAgreement/{id} ────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pba == null) return NotFound();
        return Ok(_mapper.Map<PurchaseBlanketAgreementResponse>(pba));
    }

    // ── POST /PurchaseBlanketAgreement ────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseBlanketAgreementRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var pba = _mapper.Map<PurchaseBlanketAgreement>(dto);

        foreach (var line in pba.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<PurchaseBlanketAgreement>().Add(pba);
        await _db.SaveChangesAsync();

        var created = await _db.Set<PurchaseBlanketAgreement>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == pba.Id);

        return CreatedAtAction(nameof(GetById), new { id = pba.Id },
            _mapper.Map<PurchaseBlanketAgreementResponse>(created));
    }

    // ── PUT /PurchaseBlanketAgreement/{id} ────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseBlanketAgreementUpdateRequest dto)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pba == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(pba.Items);
        _mapper.Map(dto, pba);

        foreach (var line in pba.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /PurchaseBlanketAgreement/{id} ─────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>().FindAsync(id);
        if (pba == null) return NotFound();
        _db.Set<PurchaseBlanketAgreement>().Remove(pba);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

[ApiController]
[Route("[controller]")]   // ✅ route = /APDownPaymentInvoice
public class APDownPaymentInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public APDownPaymentInvoiceController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    // ── GET /APDownPaymentInvoice ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<APDownPaymentInvoiceResponse>>(list));
    }

    // ── GET /APDownPaymentInvoice/{id} ────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        return Ok(_mapper.Map<APDownPaymentInvoiceResponse>(inv));
    }

    // ── POST /APDownPaymentInvoice ────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APDownPaymentInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var inv = _mapper.Map<APDownPaymentInvoice>(dto);

        _db.Set<APDownPaymentInvoice>().Add(inv);
        await _db.SaveChangesAsync();

        var created = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstAsync(x => x.Id == inv.Id);

        return CreatedAtAction(nameof(GetById), new { id = inv.Id },
            _mapper.Map<APDownPaymentInvoiceResponse>(created));
    }

    // ── PUT /APDownPaymentInvoice/{id} ────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APDownPaymentInvoiceUpdateRequest dto)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _mapper.Map(dto, inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /APDownPaymentInvoice/{id} ─────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        _db.Set<APDownPaymentInvoice>().Remove(inv);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
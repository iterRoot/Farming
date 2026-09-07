using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentRequest;

[ApiController]
[Route("[controller]")]   // ✅ route = /APDownPaymentRequest
public class APDownPaymentRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public APDownPaymentRequestController(MyDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    // ── GET /APDownPaymentRequest ─────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<APDownPaymentRequest>()
            .Include(x => x.Vendor)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<APDownPaymentRequestResponse>>(list));
    }

    // ── GET /APDownPaymentRequest/{id} ────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var req = await _db.Set<APDownPaymentRequest>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (req == null) return NotFound();
        return Ok(_mapper.Map<APDownPaymentRequestResponse>(req));
    }

    // ── POST /APDownPaymentRequest ────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] APDownPaymentRequestRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var req = _mapper.Map<APDownPaymentRequest>(dto);

        _db.Set<APDownPaymentRequest>().Add(req);
        await _db.SaveChangesAsync();

        var created = await _db.Set<APDownPaymentRequest>()
            .Include(x => x.Vendor)
            .FirstAsync(x => x.Id == req.Id);

        return CreatedAtAction(nameof(GetById), new { id = req.Id },
            _mapper.Map<APDownPaymentRequestResponse>(created));
    }

    // ── PUT /APDownPaymentRequest/{id} ────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] APDownPaymentRequestUpdateRequest dto)
    {
        var req = await _db.Set<APDownPaymentRequest>()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (req == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _mapper.Map(dto, req);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /APDownPaymentRequest/{id}/Pdf ────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var req = await _db.Set<APDownPaymentRequest>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (req == null) return NotFound();

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APDownPaymentRequestPdfDocument(req, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{req.DocNum}.pdf");
    }

    // ── DELETE /APDownPaymentRequest/{id} ─────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var req = await _db.Set<APDownPaymentRequest>().FindAsync(id);
        if (req == null) return NotFound();
        _db.Set<APDownPaymentRequest>().Remove(req);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
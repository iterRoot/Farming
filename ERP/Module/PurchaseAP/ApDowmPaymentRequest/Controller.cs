using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
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
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public APDownPaymentRequestController(MyDbContext db, IMapper mapper, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _approvalGate = approvalGate; _printLayout = printLayout;
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

    // ── POST /APDownPaymentRequest/{id}/Post ─── promotes a Draft request
    // to Open. No GL side effect here — the down payment INVOICE (a separate
    // document) is what posts a Journal Entry; this is just the status door.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var req = await _db.Set<APDownPaymentRequest>().FindAsync(id);
        if (req == null) return NotFound();
        if (req.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {req.Status}).");

        var gate = await _approvalGate.EvaluateAsync("APDownPaymentRequest", req.Id, req.DocNum, req.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            req.Status    = "P";
            req.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = req.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        req.Status    = "O";
        req.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Down Payment Request posted", docNum = req.DocNum });
    }

    // ── POST /APDownPaymentRequest/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var req = await _db.Set<APDownPaymentRequest>().FindAsync(id);
        if (req == null) return NotFound();
        if (req.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {req.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APDownPaymentRequest", req.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        req.Status    = "O";
        req.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Down Payment Request approved and posted", docNum = req.DocNum });
    }

    // ── POST /APDownPaymentRequest/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var req = await _db.Set<APDownPaymentRequest>().FindAsync(id);
        if (req == null) return NotFound();
        if (req.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {req.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APDownPaymentRequest", req.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        req.Status    = "D";
        req.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Down Payment Request rejected", docNum = req.DocNum });
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

        var customHtml = await _printLayout.TryRenderAsync("APDownPaymentRequest", req);
        if (customHtml != null) return Content(customHtml, "text/html");

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
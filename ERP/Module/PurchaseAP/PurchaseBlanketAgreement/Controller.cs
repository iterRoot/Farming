using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement;

[ApiController]
[Route("[controller]")]   // ✅ route = /PurchaseBlanketAgreement
public class PurchaseBlanketAgreementController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public PurchaseBlanketAgreementController(MyDbContext db, IMapper mapper, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _approvalGate = approvalGate; _printLayout = printLayout;
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

    // ── POST /PurchaseBlanketAgreement/{id}/Post ─── promotes a Draft
    // agreement to Open. No GL/inventory side effect — posting is the status
    // transition.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>().FindAsync(id);
        if (pba == null) return NotFound();
        if (pba.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {pba.Status}).");

        var gate = await _approvalGate.EvaluateAsync("PurchaseBlanketAgreement", pba.Id, pba.DocNum, pba.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            pba.Status    = "P";
            pba.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = pba.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        pba.Status    = "O";
        pba.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Blanket Agreement posted", docNum = pba.DocNum });
    }

    // ── POST /PurchaseBlanketAgreement/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>().FindAsync(id);
        if (pba == null) return NotFound();
        if (pba.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {pba.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("PurchaseBlanketAgreement", pba.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        pba.Status    = "O";
        pba.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Blanket Agreement approved and posted", docNum = pba.DocNum });
    }

    // ── POST /PurchaseBlanketAgreement/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>().FindAsync(id);
        if (pba == null) return NotFound();
        if (pba.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {pba.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("PurchaseBlanketAgreement", pba.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        pba.Status    = "D";
        pba.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Blanket Agreement rejected", docNum = pba.DocNum });
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

    // ── GET /PurchaseBlanketAgreement/{id}/Pdf ────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var pba = await _db.Set<PurchaseBlanketAgreement>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pba == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("PurchaseBlanketAgreement", pba, pba.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new PurchaseBlanketAgreementPdfDocument(pba, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{pba.DocNum}.pdf");
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
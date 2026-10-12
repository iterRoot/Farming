using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Services;
using QuestPDF.Fluent;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

[ApiController]
[Route("[controller]")]
public class SaleBlanketAgreementController : ControllerBase
{
    private readonly MyDbContext           _db;
    private readonly IMapper               _mapper;
    private readonly IDocumentNumberService _docNumber; // ✅ auto-numbering
    private readonly IApprovalGateService  _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public SaleBlanketAgreementController(
        MyDbContext            db,
        IMapper                mapper,
        IDocumentNumberService docNumber,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ════════════════════════════════════════════════════════════════
    // YOUR EXISTING ENDPOINTS (unchanged logic, added auto DocNo)
    // ════════════════════════════════════════════════════════════════

    // ── GET /SaleBlanketAgreement ────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Items).ThenInclude(l => l.ARInvoice)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleBlanketAgreementListResponse>>(list));
    }

    // ── GET /SaleBlanketAgreement/{id} ───────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Items).ThenInclude(l => l.ARInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (agreement == null) return NotFound();
        return Ok(_mapper.Map<SaleBlanketAgreementListResponse>(agreement));
    }

    // ── POST /SaleBlanketAgreement ───────────────────────────────
    // ✅ DocNum now auto-assigned from DocumentNumberRange
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleBlanketAgreementListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        // ✅ Auto-number from DocumentNumberRange
        string docNum;
        try { docNum = _docNumber.Next("SaleBlanketAgreement"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        var agreement = _mapper.Map<SaleBlanketAgreement>(dto);
        agreement.DocNum = docNum;   // ✅ override any docNum from request

        // Auto-fill ItemCode / ItemName
        foreach (var line in agreement.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<SaleBlanketAgreement>().Add(agreement);
        await _db.SaveChangesAsync();

        var created = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Items).ThenInclude(l => l.ARInvoice)
            .FirstAsync(x => x.Id == agreement.Id);

        return CreatedAtAction(nameof(GetById), new { id = agreement.Id },
            _mapper.Map<SaleBlanketAgreementListResponse>(created));
    }

    // ── POST /SaleBlanketAgreement/{id}/Post ─── promotes a Draft agreement
    // to Open. No GL/inventory side effect — posting is the status transition.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>().FindAsync(id);
        if (agreement == null) return NotFound();
        if (agreement.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {agreement.Status}).");

        var gate = await _approvalGate.EvaluateAsync("SaleBlanketAgreement", agreement.Id, agreement.DocNum, agreement.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            agreement.Status    = "P";
            agreement.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = agreement.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        agreement.Status    = "O";
        agreement.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Agreement posted", docNum = agreement.DocNum });
    }

    // ── POST /SaleBlanketAgreement/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>().FindAsync(id);
        if (agreement == null) return NotFound();
        if (agreement.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {agreement.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("SaleBlanketAgreement", agreement.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        agreement.Status    = "O";
        agreement.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Agreement approved and posted", docNum = agreement.DocNum });
    }

    // ── POST /SaleBlanketAgreement/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>().FindAsync(id);
        if (agreement == null) return NotFound();
        if (agreement.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {agreement.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("SaleBlanketAgreement", agreement.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        agreement.Status    = "D";
        agreement.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Agreement rejected", docNum = agreement.DocNum });
    }

    // ── PUT /SaleBlanketAgreement/{id} ───────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleBlanketAgreementUpdateRequest dto)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (agreement == null) return NotFound();

        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var existingDocNum = agreement.DocNum;  // ✅ preserve original DocNum
        _db.RemoveRange(agreement.Items);
        _mapper.Map(dto, agreement);
        agreement.DocNum = existingDocNum;       // ✅ never overwrite

        foreach (var line in agreement.Items)
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

    // ── GET /SaleBlanketAgreement/{id}/Pdf ───────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Items).ThenInclude(l => l.ARInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (agreement == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("SaleBlanketAgreement", agreement, agreement.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new SaleBlanketAgreementPdfDocument(agreement, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{agreement.DocNum}.pdf");
    }

    // ── DELETE /SaleBlanketAgreement/{id} ────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>().FindAsync(id);
        if (agreement == null) return NotFound();

        _db.Set<SaleBlanketAgreement>().Remove(agreement);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ════════════════════════════════════════════════════════════════
    // ✅ NEW ENDPOINTS FOR COPY FROM
    // ════════════════════════════════════════════════════════════════

    // ── GET /SaleBlanketAgreement/ForCustomer/{customerId} ───────
    // Returns Open agreements for a customer — shown in modal
    [HttpGet("ForCustomer/{customerId:int}")]
    public async Task<IActionResult> ForCustomer(int customerId)
    {
        var list = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Items).ThenInclude(l => l.ARInvoice)
            .Where(x => x.CustomerId == customerId && x.Status == "O")
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleBlanketAgreementListResponse>>(list));
    }

    // ── GET /SaleBlanketAgreement/{id}/CopyFrom ──────────────────
    // Returns pre-filled lines for Sale Order form
    [HttpGet("{id:int}/CopyFrom")]
    public async Task<IActionResult> CopyFrom(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Items).ThenInclude(l => l.ARInvoice)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (agreement == null)
            return NotFound($"Blanket Agreement {id} not found");

        if (agreement.Status == "C")
            return BadRequest($"Agreement '{agreement.DocNum}' is already Closed");

        var result = new CopyFromBlanketAgreementResponse
        {
            AgreementId     = agreement.Id,
            AgreementDocNum = agreement.DocNum,
            CustomerId      = agreement.CustomerId,
            CustomerCode    = agreement.Customer?.Code    ?? "",
            CustomerName    = agreement.Customer?.CardName ?? "",
            Remarks         = agreement.Remarks,
            Discount        = agreement.Discount,
            Tax             = agreement.Tax,
            Lines = agreement.Items.Select(l => new CopyFromBlanketAgreementLine
            {
                LineId   = l.Id,
                ItemId   = l.ItemId,
                ItemCode = l.ItemCode ?? l.Item?.ItemCode ?? "",
                ItemName = l.ItemName ?? l.Item?.ItemName ?? "",
                Quantity = l.Quantity,
                Price    = l.Price,
                Total    = l.Total,
            }).ToList(),
        };

        return Ok(result);
    }

    // ── POST /SaleBlanketAgreement/{id}/Close ────────────────────
    // Mark agreement as Closed after fully ordered
    [HttpPost("{id:int}/Close")]
    public async Task<IActionResult> Close(int id)
    {
        var agreement = await _db.Set<SaleBlanketAgreement>().FindAsync(id);
        if (agreement == null) return NotFound();
        if (agreement.Status == "C") return BadRequest("Already closed");

        agreement.Status = "C";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Agreement closed", docNum = agreement.DocNum });
    }
}
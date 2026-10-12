using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Services;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;
using DpInvoice = FarmingApi.Modules.SaleAR.ARDownPaymentInvoice.ARDownPaymentInvoice;

namespace FarmingApi.Modules.SaleAR.ArDownPaymentRequest;

[ApiController]
[Route("[controller]")]   // ✅ route = /ArDownPaymentRequest
public class ArDownPaymentRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public ArDownPaymentRequestController(MyDbContext db, IMapper mapper, IDocumentNumberService docNumber, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _docNumber = docNumber; _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /ArDownPaymentRequest ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var responses = _mapper.Map<List<ArDownPaymentRequestResponse>>(list);
        await FillInvoices(responses);
        return Ok(responses);
    }

    // ── GET /ArDownPaymentRequest/Open?customerId= ── requests that can still be invoiced
    [HttpGet("Open")]
    public async Task<IActionResult> GetOpen([FromQuery] int? customerId)
    {
        var invoiced = _db.Set<DpInvoice>().Where(i => i.BaseEntry != null && i.BaseType == "ArDownPaymentRequest").Select(i => i.BaseEntry!.Value);
        var list = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Where(x => x.Status == "O" && !invoiced.Contains(x.Id) && (customerId == null || x.CustomerId == customerId))
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<ArDownPaymentRequestResponse>>(list));
    }

    // ── GET /ArDownPaymentRequest/{id} ────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();
        var response = _mapper.Map<ArDownPaymentRequestResponse>(dp);
        await FillInvoices(new List<ArDownPaymentRequestResponse> { response });
        return Ok(response);
    }

    // ── POST /ArDownPaymentRequest ────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArDownPaymentRequestRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var dp = _mapper.Map<ArDownPaymentRequest>(dto);

        // Auto-assign the document number (server-side, race-safe).
        try { dp.DocNum = _docNumber.Next("ArDownPaymentRequest"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in dp.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<ArDownPaymentRequest>().Add(dp);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == dp.Id);

        return CreatedAtAction(nameof(GetById), new { id = dp.Id },
            _mapper.Map<ArDownPaymentRequestResponse>(created));
    }

    // ── POST /ArDownPaymentRequest/{id}/Post ─── promotes a Draft request
    // to Open. No GL side effect here — the down payment INVOICE (a separate
    // document) is what posts a Journal Entry; this is just the status door.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>().FindAsync(id);
        if (dp == null) return NotFound();
        if (dp.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {dp.Status}).");

        var gate = await _approvalGate.EvaluateAsync("ArDownPaymentRequest", dp.Id, dp.DocNum, dp.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            dp.Status    = "P";
            dp.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = dp.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        dp.Status    = "O";
        dp.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Down Payment Request posted", docNum = dp.DocNum });
    }

    // ── POST /ArDownPaymentRequest/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>().FindAsync(id);
        if (dp == null) return NotFound();
        if (dp.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {dp.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ArDownPaymentRequest", dp.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        dp.Status    = "O";
        dp.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Down Payment Request approved and posted", docNum = dp.DocNum });
    }

    // ── POST /ArDownPaymentRequest/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var dp = await _db.Set<ArDownPaymentRequest>().FindAsync(id);
        if (dp == null) return NotFound();
        if (dp.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {dp.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ArDownPaymentRequest", dp.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        dp.Status    = "D";
        dp.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Down Payment Request rejected", docNum = dp.DocNum });
    }

    // ── PUT /ArDownPaymentRequest/{id} ────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ArDownPaymentRequestUpdateRequest dto)
    {
        var dp = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

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

    // ── GET /ArDownPaymentRequest/{id}/Pdf ────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("ArDownPaymentRequest", dp, dp.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ArDownPaymentRequestPdfDocument(dp, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{dp.DocNum}.pdf");
    }

    // ── DELETE /ArDownPaymentRequest/{id} ─────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dp = await _db.Set<ArDownPaymentRequest>().FindAsync(id);
        if (dp == null) return NotFound();
        var invoice = await _db.Set<DpInvoice>().FirstOrDefaultAsync(i => i.BaseEntry == id && i.BaseType == "ArDownPaymentRequest");
        if (invoice != null)
            return BadRequest($"Down payment invoice {invoice.DocNum} was created from this request, so it can't be deleted.");
        _db.Set<ArDownPaymentRequest>().Remove(dp);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task FillInvoices(List<ArDownPaymentRequestResponse> responses)
    {
        var ids = responses.Select(r => r.Id).ToList();
        var links = await _db.Set<DpInvoice>()
            .Where(i => i.BaseEntry != null && i.BaseType == "ArDownPaymentRequest" && ids.Contains(i.BaseEntry!.Value))
            .Select(i => new { Request = i.BaseEntry!.Value, i.Id, i.DocNum, i.Status })
            .ToListAsync();
        foreach (var r in responses)
        {
            var l = links.FirstOrDefault(x => x.Request == r.Id);
            if (l == null) continue;
            r.InvoiceId = l.Id; r.InvoiceDocNum = l.DocNum; r.InvoiceStatus = l.Status;
        }
    }
}

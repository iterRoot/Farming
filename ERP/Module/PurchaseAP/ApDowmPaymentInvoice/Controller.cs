using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using QuestPDF.Fluent;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

[ApiController]
[Route("[controller]")]   // ✅ route = /APDownPaymentInvoice
public class APDownPaymentInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IAPDownPaymentInvoiceJournalService _journalService;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public APDownPaymentInvoiceController(MyDbContext db, IMapper mapper, IAPDownPaymentInvoiceJournalService journalService, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _journalService = journalService; _approvalGate = approvalGate; _printLayout = printLayout;
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

        // A Draft invoice is saved as-is, with no GL movement yet — that only
        // happens when it's posted (here, if not a draft, or later via
        // POST /APDownPaymentInvoice/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        inv.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<APDownPaymentInvoice>().Add(inv);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<APDownPaymentInvoice>()
                .Include(x => x.Vendor)
                .FirstAsync(x => x.Id == inv.Id);

            return CreatedAtAction(nameof(GetById), new { id = inv.Id },
                _mapper.Map<APDownPaymentInvoiceResponse>(draftCreated));
        }

        // Save the document + its Journal Entry in one transaction (DR Vendor Deposit / CR Accounts Payable).
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<APDownPaymentInvoice>().Add(inv);
            await _db.SaveChangesAsync();
            await PostDownPaymentInvoiceAsync(inv, vendor);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstAsync(x => x.Id == inv.Id);

        return CreatedAtAction(nameof(GetById), new { id = inv.Id },
            _mapper.Map<APDownPaymentInvoiceResponse>(created));
    }

    // ── POST /APDownPaymentInvoice/{id}/Post ─── promotes a Draft invoice to
    // Open, running the same Journal Entry posting Create runs for a
    // non-draft invoice.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        if (inv.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {inv.Status}).");

        var gate = await _approvalGate.EvaluateAsync("APDownPaymentInvoice", inv.Id, inv.DocNum, inv.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            inv.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = inv.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int? jeId = null; string? jeNo = null;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostDownPaymentInvoiceAsync(inv, inv.Vendor);
            inv.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AP Down Payment Invoice posted", docNum = inv.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /APDownPaymentInvoice/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();
        if (inv.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {inv.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APDownPaymentInvoice", inv.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int? jeId = null; string? jeNo = null;
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostDownPaymentInvoiceAsync(inv, inv.Vendor);
            inv.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AP Down Payment Invoice approved and posted", docNum = inv.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /APDownPaymentInvoice/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var inv = await _db.Set<APDownPaymentInvoice>().FindAsync(id);
        if (inv == null) return NotFound();
        if (inv.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {inv.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("APDownPaymentInvoice", inv.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        inv.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AP Down Payment Invoice rejected", docNum = inv.DocNum });
    }

    /// <summary>Creates the AP Down Payment Invoice's Journal Entry. Shared by
    /// Create (non-draft) and Post (draft → open) so both post identically.</summary>
    private async Task<(int? jeId, string? jeNo)> PostDownPaymentInvoiceAsync(APDownPaymentInvoice inv, BPEntity vendor)
    {
        var je = _journalService.CreateJournalEntry(inv, vendor);
        je.BaseDocEntry = inv.Id;
        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
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

    // ── GET /APDownPaymentInvoice/{id}/Pdf ────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var inv = await _db.Set<APDownPaymentInvoice>()
            .Include(x => x.Vendor)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (inv == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("APDownPaymentInvoice", inv);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>().OrderBy(x => x.Id).FirstOrDefaultAsync();
        var pdfBytes = new APDownPaymentInvoicePdfDocument(inv, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{inv.DocNum}.pdf");
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
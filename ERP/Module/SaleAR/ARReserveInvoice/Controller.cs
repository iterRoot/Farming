using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Services;
using QuestPDF.Fluent;

// ✅ Alias for BP entity
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.ARReserveInvoice;

[ApiController]
[Route("[controller]")]
public class ARReserveInvoiceController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IARReserveInvoiceJournalService _journalService;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public ARReserveInvoiceController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IARReserveInvoiceJournalService journalService,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout)
    {
        _db     = db;
        _mapper = mapper;
        _docNumber = docNumber;
        _journalService = journalService;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /ARReserveInvoice ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ARReserveInvoiceResponse>>(list));
    }

    // ── GET /ARReserveInvoice/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<ARReserveInvoiceResponse>(invoice));
    }

    // ── POST /ARReserveInvoice ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARReserveInvoiceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var invoice = _mapper.Map<ARReserveInvoice>(dto);

        // Auto-assign the document number (server-side, race-safe).
        try { invoice.DocNum = _docNumber.Next("ArReserveInvoice"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // Auto-fill ItemCode / ItemName from ItemsMaster
        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // A Draft invoice is saved as-is, with no GL movement yet — that only
        // happens when it's posted (here, if not a draft, or later via
        // POST /ARReserveInvoice/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        invoice.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<ARReserveInvoice>().Add(invoice);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<ARReserveInvoice>()
                .Include(x => x.Customer)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == invoice.Id);

            return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
                _mapper.Map<ARReserveInvoiceResponse>(draftCreated));
        }

        // Save the invoice + its Journal Entry in one transaction —
        // DR Accounts Receivable / CR Revenue / CR Output VAT.
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<ARReserveInvoice>().Add(invoice);
            await _db.SaveChangesAsync();           // invoice.Id populated

            (jeId, jeNo) = await PostReserveInvoiceAsync(invoice, customer);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == invoice.Id);

        var response = _mapper.Map<ARReserveInvoiceResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, response);
    }

    // ── POST /ARReserveInvoice/{id}/Post ─── promotes a Draft invoice to
    // Open, running the same Journal Entry posting Create runs for a
    // non-draft invoice.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        if (invoice.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {invoice.Status}).");

        var gate = await _approvalGate.EvaluateAsync("ARReserveInvoice", invoice.Id, invoice.DocNum, invoice.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            invoice.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = invoice.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostReserveInvoiceAsync(invoice, invoice.Customer);
            invoice.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AR Reserve Invoice posted", docNum = invoice.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /ARReserveInvoice/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        if (invoice.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {invoice.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARReserveInvoice", invoice.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostReserveInvoiceAsync(invoice, invoice.Customer);
            invoice.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AR Reserve Invoice approved and posted", docNum = invoice.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /ARReserveInvoice/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var invoice = await _db.Set<ARReserveInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {invoice.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARReserveInvoice", invoice.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        invoice.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AR Reserve Invoice rejected", docNum = invoice.DocNum });
    }

    /// <summary>Creates the AR Reserve Invoice's Journal Entry. Shared by
    /// Create (non-draft) and Post (draft → open) so both post identically.</summary>
    private async Task<(int? jeId, string? jeNo)> PostReserveInvoiceAsync(ARReserveInvoice invoice, BPEntity customer)
    {
        var je = _journalService.CreateJournalEntry(invoice, customer);
        je.BaseDocEntry = invoice.Id;
        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    // ── PUT /ARReserveInvoice/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARReserveInvoiceUpdateRequest dto)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();

        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(invoice.Items);
        _mapper.Map(dto, invoice);

        foreach (var line in invoice.Items)
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

    // ── GET /ARReserveInvoice/{id}/Pdf ────────────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("ARReserveInvoice", invoice, invoice.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ARReserveInvoicePdfDocument(invoice, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{invoice.DocNum}.pdf");
    }

    // ── DELETE /ARReserveInvoice/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ARReserveInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();

        _db.Set<ARReserveInvoice>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
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
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;
using DpRequest = FarmingApi.Modules.SaleAR.ArDownPaymentRequest.ArDownPaymentRequest;

namespace FarmingApi.Modules.SaleAR.ARDownPaymentInvoice;

[ApiController]
[Route("[controller]")]          // ✅ route = /ARDownPaymentInvoice
public class ARDownPaymentInvoiceController : ControllerBase  // ✅ renamed
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IARDonwPaymentInvoiceJournalService _journalService;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public ARDownPaymentInvoiceController(
        MyDbContext db,
        IMapper mapper,
        IDocumentNumberService docNumber,
        IARDonwPaymentInvoiceJournalService journalService,
        IApprovalGateService approvalGate,
        IPrintLayoutRenderService printLayout)
    {
        _db = db; _mapper = mapper; _docNumber = docNumber; _journalService = journalService;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        var responses = _mapper.Map<List<ARDownPaymentListResponse>>(list);
        await FillBaseDocNums(responses);
        return Ok(responses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dp == null) return NotFound();
        var response = _mapper.Map<ARDownPaymentListResponse>(dp);
        await FillBaseDocNums(new List<ARDownPaymentListResponse> { response });
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARDownPaymentListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var customer = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null) return BadRequest($"Customer {dto.CustomerId} not found");

        var linkError = await ValidateBaseRequestAsync(dto);
        if (linkError != null) return BadRequest(linkError);

        var dp = _mapper.Map<ARDownPaymentInvoice>(dto);
        dp.BaseType = dto.BaseEntry == null ? null : BaseRequestType;

        // Auto-assign the document number (server-side, race-safe).
        try { dp.DocNum = _docNumber.Next("ArDownPaymentInvoice"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in dp.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // A Draft invoice is saved as-is, with no GL movement yet — that only
        // happens when it's posted (here, if not a draft, or later via
        // POST /ARDownPaymentInvoice/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        dp.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<ARDownPaymentInvoice>().Add(dp);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<ARDownPaymentInvoice>()
                .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == dp.Id);

            var draftResponse = _mapper.Map<ARDownPaymentListResponse>(draftCreated);
            await FillBaseDocNums(new List<ARDownPaymentListResponse> { draftResponse });
            return CreatedAtAction(nameof(GetById), new { id = dp.Id }, draftResponse);
        }

        // Save the invoice + its Journal Entry in one transaction —
        // DR Accounts Receivable / CR Customer Deposit (down payment on account).
        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<ARDownPaymentInvoice>().Add(dp);
            await _db.SaveChangesAsync();           // dp.Id populated

            (jeId, jeNo) = await PostDownPaymentInvoiceAsync(dp, customer);
            await CloseBaseRequestAsync(dp);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == dp.Id);

        var response = _mapper.Map<ARDownPaymentListResponse>(created);
        response.JournalEntryId = jeId;
        response.JournalNo       = jeNo;
        await FillBaseDocNums(new List<ARDownPaymentListResponse> { response });

        return CreatedAtAction(nameof(GetById), new { id = dp.Id }, response);
    }

    // ── POST /ARDownPaymentInvoice/{id}/Post ─── promotes a Draft invoice to
    // Open, running the same Journal Entry posting Create runs for a
    // non-draft invoice.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();
        if (dp.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {dp.Status}).");

        var gate = await _approvalGate.EvaluateAsync("ARDownPaymentInvoice", dp.Id, dp.DocNum, dp.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            dp.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = dp.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostDownPaymentInvoiceAsync(dp, dp.Customer);
            dp.Status = "O";
            await CloseBaseRequestAsync(dp);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AR Down Payment Invoice posted", docNum = dp.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /ARDownPaymentInvoice/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();
        if (dp.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {dp.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARDownPaymentInvoice", dp.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        int?    jeId = null;
        string? jeNo = null;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            (jeId, jeNo) = await PostDownPaymentInvoiceAsync(dp, dp.Customer);
            dp.Status = "O";
            await CloseBaseRequestAsync(dp);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "AR Down Payment Invoice approved and posted", docNum = dp.DocNum, journalEntryId = jeId, journalNo = jeNo });
    }

    // ── POST /ARDownPaymentInvoice/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().FindAsync(id);
        if (dp == null) return NotFound();
        if (dp.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {dp.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARDownPaymentInvoice", dp.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        dp.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AR Down Payment Invoice rejected", docNum = dp.DocNum });
    }

    /// <summary>Creates the AR Down Payment Invoice's Journal Entry. Shared by
    /// Create (non-draft) and Post (draft → open) so both post identically.</summary>
    private async Task<(int? jeId, string? jeNo)> PostDownPaymentInvoiceAsync(ARDownPaymentInvoice dp, BPEntity customer)
    {
        var je = _journalService.CreateJournalEntry(dp, customer);
        je.BaseDocEntry = dp.Id;
        await _db.SaveChangesAsync();
        return (je.Id, je.JrnlNo);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARDownPaymentUpdateRequest dto)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
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

    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dp == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("ARDownPaymentInvoice", dp, dp.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ARDownPaymentInvoicePdfDocument(dp, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{dp.DocNum}.pdf");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var dp = await _db.Set<ARDownPaymentInvoice>().FindAsync(id);
        if (dp == null) return NotFound();
        await ReopenBaseRequestAsync(dp);   // its request goes back to Open if this invoice had closed it
        _db.Set<ARDownPaymentInvoice>().Remove(dp);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── Link to the A/R Down Payment Request this invoice settles ─────────
    private const string BaseRequestType = "ArDownPaymentRequest";

    private async Task<string?> ValidateBaseRequestAsync(ARDownPaymentListRequest dto)
    {
        if (dto.BaseEntry == null) return null;
        if (!string.IsNullOrEmpty(dto.BaseType) && dto.BaseType != BaseRequestType)
            return $"BaseType must be \"{BaseRequestType}\".";

        var request = await _db.Set<DpRequest>().FirstOrDefaultAsync(r => r.Id == dto.BaseEntry);
        if (request == null) return $"Down payment request {dto.BaseEntry} not found.";
        if (request.CustomerId != dto.CustomerId)
            return $"Request {request.DocNum} belongs to a different customer.";
        if (request.Status != "O")
            return $"Request {request.DocNum} is {StatusLabel(request.Status)}; only an open request can be invoiced.";

        var already = await _db.Set<ARDownPaymentInvoice>()
            .FirstOrDefaultAsync(i => i.BaseEntry == dto.BaseEntry && i.BaseType == BaseRequestType);
        if (already != null) return $"Request {request.DocNum} is already invoiced by {already.DocNum}.";

        if (dto.Total > request.Total + 0.01m)
            return $"The invoice ({dto.Total:N2}) can't exceed the request ({request.Total:N2}).";
        return null;
    }

    private static string StatusLabel(string s) => s switch { "D" => "a draft", "P" => "pending approval", "C" => "closed", _ => s };

    // Posting the invoice settles its request.
    private async Task CloseBaseRequestAsync(ARDownPaymentInvoice dp)
    {
        if (dp.BaseEntry == null) return;
        var request = await _db.Set<DpRequest>().FirstOrDefaultAsync(r => r.Id == dp.BaseEntry);
        if (request == null || request.Status == "C") return;
        request.Status = "C";
        request.UpdatedAt = DateTime.UtcNow;
    }

    // Removing a posted invoice frees its request to be invoiced again.
    private async Task ReopenBaseRequestAsync(ARDownPaymentInvoice dp)
    {
        if (dp.BaseEntry == null || dp.Status == "D" || dp.Status == "P") return;
        var request = await _db.Set<DpRequest>().FirstOrDefaultAsync(r => r.Id == dp.BaseEntry);
        if (request == null || request.Status != "C") return;
        request.Status = "O";
        request.UpdatedAt = DateTime.UtcNow;
    }

    private async Task FillBaseDocNums(List<ARDownPaymentListResponse> responses)
    {
        var ids = responses.Where(r => r.BaseEntry != null).Select(r => r.BaseEntry!.Value).Distinct().ToList();
        if (ids.Count == 0) return;
        var nums = await _db.Set<DpRequest>().Where(r => ids.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r.DocNum);
        foreach (var r in responses.Where(r => r.BaseEntry != null))
            r.BaseDocNum = nums.GetValueOrDefault(r.BaseEntry!.Value);
    }
}

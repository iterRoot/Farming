using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using FarmingApi.Services;

using QuestPDF.Fluent;
using CompanyEntity = FarmingApi.Modules.Company.Company;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using BPAddress = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BPAddress;
using AttachmentStorage = FarmingApi.Modules.Administration.Attachment.AttachmentStorage;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

[ApiController]
[Route("[controller]")]
public class ARInvoiceController : ControllerBase
{
    private readonly MyDbContext             _db;
    private readonly IMapper                 _mapper;
    private readonly IDocumentNumberService  _docNumber;
    private readonly IARInvoiceJournalService _journalService;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IWebHostEnvironment      _env;
    private readonly IApprovalGateService     _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;
    private readonly AttachmentStorage        _storage;

    public ARInvoiceController(
        MyDbContext              db,
        IMapper                  mapper,
        IDocumentNumberService   docNumber,
        IARInvoiceJournalService journalService,
        IInventoryPostingService inventoryPosting,
        IWebHostEnvironment      env,
        IApprovalGateService     approvalGate,
        IPrintLayoutRenderService printLayout,
        AttachmentStorage        storage)
    {
        _db             = db;
        _mapper         = mapper;
        _docNumber      = docNumber;
        _journalService = journalService;
        _inventoryPosting = inventoryPosting;
        _env            = env;
        _approvalGate   = approvalGate;
        _printLayout    = printLayout;
        _storage        = storage;
    }

    // ── GET /ARInvoice ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Attachments)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var responses = _mapper.Map<List<ARInvoiceListResponse>>(list);

        var jeMap = await _db.Set<JournalEntry>()
            .Where(j => j.BaseDocType == "KARI" && list.Select(i => i.Id).Contains(j.BaseDocEntry!.Value))
            .ToDictionaryAsync(j => j.BaseDocEntry!.Value, j => j.Id);

        foreach (var r in responses)
            if (jeMap.TryGetValue(r.Id, out var jeId))
                r.JournalEntryId = jeId;

        return Ok(responses);
    }

    // ── GET /ARInvoice/{id} ───────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();

        var response = _mapper.Map<ARInvoiceListResponse>(invoice);

        var je = await _db.Set<JournalEntry>()
            .FirstOrDefaultAsync(j => j.BaseDocType == "KARI" && j.BaseDocEntry == id);
        response.JournalEntryId = je?.Id;

        return Ok(response);
    }

    // ══════════════════════════════════════════════════════════════
    // POST /ARInvoice — TRANSACTIONAL (Invoice + JE or rollback both)
    // ══════════════════════════════════════════════════════════════
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ARInvoiceListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // ── Validate customer (with addresses + payment methods so we can
        //     default the Logistics/Accounting tabs from the master) ──────
        var customer = await _db.Set<BPEntity>()
            .Include(x => x.Addresses)
            .Include(x => x.PaymentMethods)
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        // ── Auto-number ───────────────────────────────────────────
        string docNum;
        try { docNum = _docNumber.Next("ArInvoice"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // ── Map + enrich lines ────────────────────────────────────
        var invoice    = _mapper.Map<ARInvoice>(dto);
        invoice.DocNum = docNum;

        // ── Default the Logistics & Accounting tabs from the customer
        //     master when the request left them blank (SAP B1 behavior) ────
        ApplyBusinessPartnerDefaults(invoice, customer);

        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            // Blank warehouse falls back to the default; an unknown one is rejected.
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // A Draft invoice is saved as-is, with no stock/GL movement yet —
        // those only happen when it's posted (here, if not a draft, or later
        // via POST /ARInvoice/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        invoice.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<ARInvoice>().Add(invoice);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<ARInvoice>()
                .Include(x => x.Customer)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .Include(x => x.Attachments)
                .FirstAsync(x => x.Id == invoice.Id);

            return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
                new { invoice = _mapper.Map<ARInvoiceListResponse>(draftCreated) });
        }

        // ══════════════════════════════════════════════════════════
        // BEGIN TRANSACTION — Invoice + Journal Entry
        // If ANYTHING fails, BOTH are rolled back.
        // ══════════════════════════════════════════════════════════
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // ── Step 1: Save the invoice ──────────────────────────
            _db.Set<ARInvoice>().Add(invoice);
            await _db.SaveChangesAsync();
            // invoice.Id is now populated

            var (je, _, _) = await PostARInvoiceAsync(invoice, customer);

            // ── Step 4: Commit — both invoice + JE are final ──────
            await transaction.CommitAsync();

            // ── Return created invoice + JE id ────────────────────
            var created = await _db.Set<ARInvoice>()
                .Include(x => x.Customer)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .Include(x => x.Attachments)
                .FirstAsync(x => x.Id == invoice.Id);

            return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, new
            {
                invoice        = _mapper.Map<ARInvoiceListResponse>(created),
                journalEntryId = je.Id,
                journalNo      = je.JrnlNo,
            });
        }
        catch (InvalidOperationException ex)
        {
            // ── ROLLBACK — neither invoice nor JE is saved ────────
            await transaction.RollbackAsync();

            return BadRequest(new
            {
                error   = "AR Invoice creation failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure ARControlAccount, RevenueAccount, TaxOutputAccount."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                error   = "Unexpected error — transaction rolled back",
                details = ex.Message,
            });
        }
    }

    // ── POST /ARInvoice/{id}/Post ─── promotes a Draft invoice to Open,
    // running the same inventory + Journal Entry posting Create runs for a
    // non-draft invoice.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        if (invoice.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {invoice.Status}).");

        var gate = await _approvalGate.EvaluateAsync("ARInvoice", invoice.Id, invoice.DocNum, invoice.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            invoice.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = invoice.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var (je, _, _) = await PostARInvoiceAsync(invoice, invoice.Customer);
            invoice.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "AR Invoice posted", docNum = invoice.DocNum, journalEntryId = je.Id, journalNo = je.JrnlNo });
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new
            {
                error   = "AR Invoice posting failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure ARControlAccount, RevenueAccount, TaxOutputAccount."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = "Unexpected error — transaction rolled back", details = ex.Message });
        }
    }

    // ── POST /ARInvoice/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        if (invoice.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {invoice.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARInvoice", invoice.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var (je, _, _) = await PostARInvoiceAsync(invoice, invoice.Customer);
            invoice.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "AR Invoice approved and posted", docNum = invoice.DocNum, journalEntryId = je.Id, journalNo = je.JrnlNo });
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(new
            {
                error   = "AR Invoice posting failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure ARControlAccount, RevenueAccount, TaxOutputAccount."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = "Unexpected error — transaction rolled back", details = ex.Message });
        }
    }

    // ── POST /ARInvoice/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {invoice.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ARInvoice", invoice.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        invoice.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "AR Invoice rejected", docNum = invoice.DocNum });
    }

    /// <summary>
    /// Creates the AR Invoice's Journal Entry and posts stock out for lines
    /// not already issued by a prior Delivery. Shared by Create (non-draft)
    /// and Post (draft → open) so both post identically.
    /// </summary>
    private async Task<(JournalEntry je, int? jeId, string? jeNo)> PostARInvoiceAsync(ARInvoice invoice, BPEntity customer)
    {
        var je = _journalService.CreateJournalEntry(invoice, customer);
        je.BaseDocEntry = invoice.Id;

        // Post stock out — only for lines NOT based on a Delivery. A delivery
        // already issued the goods, so posting here too would double-count
        // the stock. Standalone invoice lines (sold without a prior delivery)
        // are the ones that move stock.
        var stockDate = invoice.PostingDate ?? DateTime.UtcNow;
        foreach (var line in invoice.Items.Where(l => !IsBasedOnDelivery(l)))
        {
            await _inventoryPosting.PostOutAsync(
                line.ItemCode!, line.WhsCode!, line.Quantity,
                "AR Invoice", "ARInvoice", invoice.Id, invoice.DocNum, stockDate,
                customer.Code, customer.CardName, batches: line.Batches);
        }

        await _db.SaveChangesAsync();

        return (je, je.Id, je.JrnlNo);
    }

    // ── PUT /ARInvoice/{id} ───────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ARInvoiceUpdateRequest dto)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (invoice == null) return NotFound();

        if (invoice.Status == "C")
            return BadRequest("Cannot edit a Closed AR Invoice");

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var existingDocNum = invoice.DocNum;
        _db.RemoveRange(invoice.Items);
        _mapper.Map(dto, invoice);
        invoice.DocNum = existingDocNum;

        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// True when the line was copied from a Delivery. The delivery already
    /// issued the stock, so the invoice must not post an inventory movement
    /// for it — otherwise the same goods leave stock twice.
    /// </summary>
    private static bool IsBasedOnDelivery(ARInvoiceLine line) =>
        line.BaseEntry.HasValue
        && string.Equals(line.BaseType, "Delivery", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Fills the Logistics and Accounting tab fields from the customer master
    /// for any value the request left blank — mirrors how SAP B1 defaults a
    /// document's ship-to/bill-to, shipping type, payment terms, project and
    /// control account from the Business Partner.
    /// </summary>
    private static void ApplyBusinessPartnerDefaults(ARInvoice invoice, BPEntity customer)
    {
        // ── Logistics ──────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(invoice.ShipToAddress))
            invoice.ShipToAddress = FormatAddress(PickAddress(customer, adresType: 0));

        if (string.IsNullOrWhiteSpace(invoice.BillToAddress))
            invoice.BillToAddress = FormatAddress(PickAddress(customer, adresType: 1));

        if (string.IsNullOrWhiteSpace(invoice.ShippingType))
            invoice.ShippingType = customer.ShippingType;

        // ── Accounting ─────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(invoice.PaymentTerms))
            invoice.PaymentTerms = customer.PayTerms;

        if (string.IsNullOrWhiteSpace(invoice.PaymentMethod))
            invoice.PaymentMethod = customer.PaymentMethods
                .FirstOrDefault(p => p.Active)?.Code;

        if (string.IsNullOrWhiteSpace(invoice.BPProject))
            invoice.BPProject = customer.BPProject;

        if (string.IsNullOrWhiteSpace(invoice.ControlAccount))
            invoice.ControlAccount = customer.ARControlAccount;
    }

    /// <summary>Default address of the given type (0=Ship-To, 1=Bill-To),
    /// preferring the one flagged IsDefault.</summary>
    private static BPAddress? PickAddress(BPEntity customer, int adresType) =>
        customer.Addresses
            .Where(a => a.AdresType == adresType)
            .OrderByDescending(a => a.IsDefault)
            .FirstOrDefault();

    /// <summary>Flattens an address record into a single display line.</summary>
    private static string? FormatAddress(BPAddress? a)
    {
        if (a == null) return null;

        var parts = new[]
        {
            string.Join(" ", new[] { a.Street, a.StreetNo }
                .Where(s => !string.IsNullOrWhiteSpace(s))),
            a.BuildingFloorRoom,
            a.City,
            a.State,
            a.ZipCode,
            a.Country,
        }.Where(s => !string.IsNullOrWhiteSpace(s));

        var text = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    // ── GET /ARInvoice/{id}/Pdf ────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var invoice = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("ARInvoice", invoice, invoice.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new ARInvoicePdfDocument(invoice, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{invoice.DocNum}.pdf");
    }

    // ── POST /ARInvoice/{id}/Close ────────────────────────────────
    [HttpPost("{id:int}/Close")]
    public async Task<IActionResult> Close(int id)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == "C") return BadRequest("AR Invoice already closed");

        invoice.Status    = "C";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "AR Invoice closed", docNum = invoice.DocNum });
    }

    // ── DELETE /ARInvoice/{id} ────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == "C")
            return BadRequest("Cannot delete a Closed AR Invoice");

        // The attachment rows cascade; their stored files don't, so remove those too.
        var filePaths = await _db.Set<ARInvoiceAttachment>()
            .Where(a => a.ARInvoiceId == id).Select(a => a.FilePath).ToListAsync();
        foreach (var path in filePaths) _storage.Delete(path);

        _db.Set<ARInvoice>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ══════════════════════════════════════════════════════════════
    // ATTACHMENTS TAB
    // ══════════════════════════════════════════════════════════════

    // ── POST /ARInvoice/{id}/Attachments ──────────────────────────
    // Uploads one or more files and keeps their bytes in the database (KSFL) —
    // the host's disk doesn't survive a restart. FilePath holds the storage key.
    [HttpPost("{id:int}/Attachments")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB
    public async Task<IActionResult> UploadAttachments(int id, [FromForm] List<IFormFile> files)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == "C")
            return BadRequest("Cannot add attachments to a Closed AR Invoice");

        if (files == null || files.Count == 0)
            return BadRequest("No files were uploaded");

        var saved = new List<ARInvoiceAttachment>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;

            // Store under a unique name to avoid collisions; keep the original name in DB.
            var safeName    = Path.GetFileName(file.FileName);
            var storedPath  = $"ARInvoice/{id}/{Guid.NewGuid():N}{Path.GetExtension(safeName)}";

            // Queued on _db; the SaveChangesAsync below writes the file and its rows together.
            await using (var stream = file.OpenReadStream())
                await _storage.SaveAsync(storedPath, stream);

            var attachment = new ARInvoiceAttachment
            {
                ARInvoiceId  = id,
                FileName     = safeName,
                FilePath     = storedPath,
                FileSize     = file.Length,
                ContentType  = file.ContentType,
                UploadedDate = DateTime.UtcNow,
            };

            _db.Set<ARInvoiceAttachment>().Add(attachment);
            saved.Add(attachment);
        }

        if (saved.Count == 0)
            return BadRequest("All uploaded files were empty");

        await _db.SaveChangesAsync();

        return Ok(_mapper.Map<List<ARInvoiceAttachmentResponse>>(saved));
    }

    // ── GET /ARInvoice/{id}/Attachments/{attachmentId} ────────────
    // Downloads the stored file.
    [HttpGet("{id:int}/Attachments/{attachmentId:int}")]
    public async Task<IActionResult> DownloadAttachment(int id, int attachmentId)
    {
        var attachment = await _db.Set<ARInvoiceAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ARInvoiceId == id);
        if (attachment == null) return NotFound();

        var bytes = await _storage.ReadAsync(attachment.FilePath);
        if (bytes == null)
            return NotFound("The stored file is missing from storage");

        return File(bytes, attachment.ContentType ?? "application/octet-stream", attachment.FileName);
    }

    // ── DELETE /ARInvoice/{id}/Attachments/{attachmentId} ─────────
    [HttpDelete("{id:int}/Attachments/{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
    {
        var invoice = await _db.Set<ARInvoice>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == "C")
            return BadRequest("Cannot remove attachments from a Closed AR Invoice");

        var attachment = await _db.Set<ARInvoiceAttachment>()
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.ARInvoiceId == id);
        if (attachment == null) return NotFound();

        _storage.Delete(attachment.FilePath);
        _db.Set<ARInvoiceAttachment>().Remove(attachment);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
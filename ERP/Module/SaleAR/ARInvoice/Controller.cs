using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Services;

using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

[ApiController]
[Route("[controller]")]
public class ARInvoiceController : ControllerBase
{
    private readonly MyDbContext             _db;
    private readonly IMapper                 _mapper;
    private readonly IDocumentNumberService  _docNumber;
    private readonly IARInvoiceJournalService _journalService;

    public ARInvoiceController(
        MyDbContext              db,
        IMapper                  mapper,
        IDocumentNumberService   docNumber,
        IARInvoiceJournalService journalService)
    {
        _db             = db;
        _mapper         = mapper;
        _docNumber      = docNumber;
        _journalService = journalService;
    }

    // ── GET /ARInvoice ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ARInvoice>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
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

        // ── Validate customer ─────────────────────────────────────
        var customer = await _db.Set<BPEntity>()
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

        foreach (var line in invoice.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
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

            // ── Step 2: Auto-create Journal Entry ─────────────────
            // This adds the JE + lines to the DbContext but does NOT
            // call SaveChanges — we do it once for both.
            var je = _journalService.CreateJournalEntry(invoice, customer);

            // Update BaseDocEntry now that invoice.Id exists
            je.BaseDocEntry = invoice.Id;

            // ── Step 3: Save Journal Entry ────────────────────────
            await _db.SaveChangesAsync();

            // ── Step 4: Commit — both invoice + JE are final ──────
            await transaction.CommitAsync();

            // ── Return created invoice + JE id ────────────────────
            var created = await _db.Set<ARInvoice>()
                .Include(x => x.Customer)
                .Include(x => x.Items).ThenInclude(l => l.Item)
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
        }

        await _db.SaveChangesAsync();
        return NoContent();
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

        _db.Set<ARInvoice>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
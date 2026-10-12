using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;  // ✅ your namespace
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using FarmingApi.Services;

using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.Return;

[ApiController]
[Route("[controller]")]         // ✅ route = /Return
public class ReturnController : ControllerBase  // ✅ ReturnController
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IDocumentNumberService _docNumber;
    private readonly IApprovalGateService _approvalGate;

    public ReturnController(
        MyDbContext db,
        IMapper mapper,
        IInventoryPostingService inventoryPosting,
        IDocumentNumberService docNumber,
        IApprovalGateService approvalGate)
    {
        _db     = db;
        _mapper = mapper;
        _inventoryPosting = inventoryPosting;
        _docNumber = docNumber;
        _approvalGate = approvalGate;
    }

    // ── GET /Return ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ReturnListResponse>>(list));
    }

    // ── GET /Return/{id} ────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();
        return Ok(_mapper.Map<ReturnListResponse>(Return));
    }

    // ── POST /Return ────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var Return = _mapper.Map<Return>(dto);

        // Auto-assign the document number (server-side, race-safe) — same as
        // every other Sale A/R document.
        try { Return.DocNum = _docNumber.Next("Return"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in Return.Items)
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

        // A Draft return is saved as-is, with no stock movement yet — that
        // only happens when it's posted (here, if not a draft, or later via
        // POST /Return/{id}/Post).
        bool isDraft = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase);
        Return.Status = isDraft ? "D" : "O";

        if (isDraft)
        {
            _db.Set<Return>().Add(Return);
            await _db.SaveChangesAsync();

            var draftCreated = await _db.Set<Return>()
                .Include(x => x.Customer)
                .Include(x => x.Items).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == Return.Id);

            return CreatedAtAction(nameof(GetById), new { id = Return.Id },
                _mapper.Map<ReturnListResponse>(draftCreated));
        }

        // The return and its stock movement must commit together, or neither.
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<Return>().Add(Return);
            await _db.SaveChangesAsync();       // Return.Id populated

            await PostReturnAsync(Return, customer);

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == Return.Id);

        return CreatedAtAction(nameof(GetById), new { id = Return.Id },
            _mapper.Map<ReturnListResponse>(created));
    }

    // ── POST /Return/{id}/Post ─── promotes a Draft return to Open, running
    // the same inventory-in posting Create runs for a non-draft return.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();
        if (Return.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {Return.Status}).");

        var gate = await _approvalGate.EvaluateAsync("Return", Return.Id, Return.DocNum, Return.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            Return.Status = "P";
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = Return.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await PostReturnAsync(Return, Return.Customer);
            Return.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "Return posted", docNum = Return.DocNum });
    }

    // ── POST /Return/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();
        if (Return.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {Return.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("Return", Return.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            await PostReturnAsync(Return, Return.Customer);
            Return.Status = "O";
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        return Ok(new { message = "Return approved and posted", docNum = Return.DocNum });
    }

    // ── POST /Return/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var Return = await _db.Set<Return>().FindAsync(id);
        if (Return == null) return NotFound();
        if (Return.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {Return.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("Return", Return.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        Return.Status = "D";
        await _db.SaveChangesAsync();

        return Ok(new { message = "Return rejected", docNum = Return.DocNum });
    }

    /// <summary>Brings returned goods back into stock at current average cost.
    /// Shared by Create (non-draft) and Post (draft → open) so both post
    /// identically.</summary>
    private async Task PostReturnAsync(Return Return, BPEntity customer)
    {
        var stockDate = Return.PostingDate ?? DateTime.UtcNow;
        foreach (var line in Return.Items)
        {
            await _inventoryPosting.PostReturnInAsync(
                line.ItemCode!, line.WhsCode!, line.Quantity,
                "Return", "Return", Return.Id, Return.DocNum, stockDate,
                customer.Code, customer.CardName, batches: line.Batches);
        }

        await _db.SaveChangesAsync();
    }

    // ── PUT /Return/{id} ────────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReturnUpdateRequest dto)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(Return.Items);
        _mapper.Map(dto, Return);

        foreach (var line in Return.Items)
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

    // ── DELETE /Return/{id} ─────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var Return = await _db.Set<Return>().FindAsync(id);
        if (Return == null) return NotFound();

        _db.Set<Return>().Remove(Return);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
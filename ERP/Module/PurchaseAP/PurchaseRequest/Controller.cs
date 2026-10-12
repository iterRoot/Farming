using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Modules.Administration.DocumentNumberRange;
using FarmingApi.Services;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.PurchaseRequest;

// ═══════════════════════════════════════════════════════════════
// PURCHASE REQUEST CONTROLLER
// GET    /PurchaseRequest
// GET    /PurchaseRequest/{id}
// POST   /PurchaseRequest
// PUT    /PurchaseRequest/{id}
// DELETE /PurchaseRequest/{id}
// POST   /PurchaseRequest/{id}/Post
// POST   /PurchaseRequest/{id}/Approve
// POST   /PurchaseRequest/{id}/Reject
// POST   /PurchaseRequest/{id}/Close
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]   // route = /PurchaseRequest
public class PurchaseRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IDocumentNumberRangeRepository _docNums;
    private readonly IApprovalGateService _approvalGate;

    public PurchaseRequestController(MyDbContext db, IMapper mapper,
                                      IDocumentNumberRangeRepository docNums,
                                      IApprovalGateService approvalGate)
    {
        _db = db; _mapper = mapper; _docNums = docNums; _approvalGate = approvalGate;
    }

    // ── GET /PurchaseRequest ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<PurchaseRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<PurchaseRequestResponse>>(list));
    }

    // ── GET /PurchaseRequest/{id} ─────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pr = await _db.Set<PurchaseRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pr == null) return NotFound();
        return Ok(_mapper.Map<PurchaseRequestResponse>(pr));
    }

    // ── POST /PurchaseRequest ─────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseRequestRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Vendor is optional — a requisition can be raised before anyone has
        // decided who to buy from. Validate it only when one was actually sent.
        BPEntity? vendor = null;
        if (dto.VendorId.HasValue)
        {
            vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId.Value);
            if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");
        }

        var pr = _mapper.Map<PurchaseRequest>(dto);

        // The document number is assigned here, never taken from the client.
        try { pr.DocNum = _docNums.GenerateNextNumber("PurchaseRequest").DocNo; }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // A Draft request is saved as-is; Open is the only status that means
        // "ready to act on" — same D/O/C convention as every other document.
        pr.Status = string.Equals(dto.Status, "D", StringComparison.OrdinalIgnoreCase) ? "D" : "O";

        foreach (var line in pr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<PurchaseRequest>().Add(pr);
        await _db.SaveChangesAsync();

        var created = await _db.Set<PurchaseRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == pr.Id);

        return CreatedAtAction(nameof(GetById), new { id = pr.Id },
            _mapper.Map<PurchaseRequestResponse>(created));
    }

    // ── PUT /PurchaseRequest/{id} ─────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PurchaseRequestUpdateRequest dto)
    {
        var pr = await _db.Set<PurchaseRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (pr == null) return NotFound();
        if (pr.Status == "C")
            return BadRequest("Cannot edit a Closed Purchase Request");

        if (dto.VendorId.HasValue)
        {
            var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId.Value);
            if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");
        }

        var existingDocNum = pr.DocNum;
        var existingStatus = pr.Status;
        _db.RemoveRange(pr.Items);
        _mapper.Map(dto, pr);
        pr.DocNum = existingDocNum;   // never overwrite
        pr.Status = existingStatus;   // Status changes only via Post/Close, not a plain edit

        foreach (var line in pr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /PurchaseRequest/{id}/Post ─── promotes a Draft request to
    // Open. No GL/inventory side effect on a requisition — posting is the
    // status transition that makes it visible/actionable to purchasing.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var pr = await _db.Set<PurchaseRequest>().FindAsync(id);
        if (pr == null) return NotFound();
        if (pr.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {pr.Status}).");

        var gate = await _approvalGate.EvaluateAsync("PurchaseRequest", pr.Id, pr.DocNum, pr.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            pr.Status    = "P";
            pr.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = pr.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        pr.Status    = "O";
        pr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Request posted", docNum = pr.DocNum });
    }

    // ── POST /PurchaseRequest/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var pr = await _db.Set<PurchaseRequest>().FindAsync(id);
        if (pr == null) return NotFound();
        if (pr.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {pr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("PurchaseRequest", pr.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        pr.Status    = "O";
        pr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Request approved and posted", docNum = pr.DocNum });
    }

    // ── POST /PurchaseRequest/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var pr = await _db.Set<PurchaseRequest>().FindAsync(id);
        if (pr == null) return NotFound();
        if (pr.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {pr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("PurchaseRequest", pr.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        pr.Status    = "D";
        pr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Request rejected", docNum = pr.DocNum });
    }

    // ── POST /PurchaseRequest/{id}/Close ──────────────────────────────────
    [HttpPost("{id:int}/Close")]
    public async Task<IActionResult> Close(int id)
    {
        var pr = await _db.Set<PurchaseRequest>().FindAsync(id);
        if (pr == null) return NotFound();
        if (pr.Status == "C") return BadRequest("Purchase Request already closed");

        pr.Status    = "C";
        pr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Purchase Request closed", docNum = pr.DocNum });
    }

    // ── DELETE /PurchaseRequest/{id} ──────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pr = await _db.Set<PurchaseRequest>().FindAsync(id);
        if (pr == null) return NotFound();
        if (pr.Status == "C")
            return BadRequest("Cannot delete a Closed Purchase Request");

        _db.Set<PurchaseRequest>().Remove(pr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Administration.DocumentNumberRange;
using FarmingApi.Modules.Inventory.ItemsMaster;

// ✅ Alias for BP entity
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ReturnRequest;

[ApiController]
[Route("[controller]")]
public class ReturnRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IApprovalGateService _approvalGate;
    private readonly IDocumentNumberRangeRepository _docNums;

    public ReturnRequestController(MyDbContext db, IMapper mapper, IApprovalGateService approvalGate, IDocumentNumberRangeRepository docNums)
    {
        _db     = db;
        _mapper = mapper;
        _approvalGate = approvalGate;
        _docNums = docNums;
    }

    // ── GET /ReturnRequest ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ReturnRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ReturnRequestListResponse>>(list));
    }

    // ── GET /ReturnRequest/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _db.Set<ReturnRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (invoice == null) return NotFound();
        return Ok(_mapper.Map<ReturnRequestListResponse>(invoice));
    }

    // ── POST /ReturnRequest ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnRequestListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BPEntity>()   // ✅ alias
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var invoice = _mapper.Map<ReturnRequest>(dto);

        // The screen reserves a number via /DocumentNumberRange/Next and sends it;
        // any other caller sends none, and DocNum is required, so assign one.
        if (string.IsNullOrWhiteSpace(invoice.DocNum))
        {
            try { invoice.DocNum = _docNums.GenerateNextNumber("ReturnRequest").DocNo; }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

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

        _db.Set<ReturnRequest>().Add(invoice);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ReturnRequest>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == invoice.Id);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
            _mapper.Map<ReturnRequestListResponse>(created));
    }

    // ── POST /ReturnRequest/{id}/Post ─── promotes a Draft request to Open.
    // No GL/inventory side effect — the Return document itself is what
    // brings goods back into stock; this is just the status transition.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var invoice = await _db.Set<ReturnRequest>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {invoice.Status}).");

        var gate = await _approvalGate.EvaluateAsync("ReturnRequest", invoice.Id, invoice.DocNum, invoice.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            invoice.Status    = "P";
            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = invoice.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        invoice.Status    = "O";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Return Request posted", docNum = invoice.DocNum });
    }

    // ── POST /ReturnRequest/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var invoice = await _db.Set<ReturnRequest>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {invoice.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ReturnRequest", invoice.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        invoice.Status    = "O";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Return Request approved and posted", docNum = invoice.DocNum });
    }

    // ── POST /ReturnRequest/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var invoice = await _db.Set<ReturnRequest>().FindAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {invoice.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("ReturnRequest", invoice.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        invoice.Status    = "D";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Return Request rejected", docNum = invoice.DocNum });
    }

    // ── PUT /ReturnRequest/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReturnRequestUpdateRequest dto)
    {
        var invoice = await _db.Set<ReturnRequest>()
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

    // ── DELETE /ReturnRequest/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _db.Set<ReturnRequest>().FindAsync(id);
        if (invoice == null) return NotFound();

        _db.Set<ReturnRequest>().Remove(invoice);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
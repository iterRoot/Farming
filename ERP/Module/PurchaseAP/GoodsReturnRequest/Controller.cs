using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.Administration.DocumentNumberRange;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReturnRequest;

[ApiController]
[Route("[controller]")]
public class GoodsReturnRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IApprovalGateService _approvalGate;
    private readonly IDocumentNumberRangeRepository _docNums;

    public GoodsReturnRequestController(MyDbContext db, IMapper mapper, IApprovalGateService approvalGate, IDocumentNumberRangeRepository docNums)
    { _db = db; _mapper = mapper; _approvalGate = approvalGate; _docNums = docNums; }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
        return Ok(_mapper.Map<List<GoodsReturnRequestResponse>>(list));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var grr = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (grr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReturnRequestResponse>(grr));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReturnRequestRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var grr = _mapper.Map<GoodsReturnRequest>(dto);

        // The screen reserves a number via /DocumentNumberRange/Next and sends it;
        // any other caller sends none, and DocNum is required, so assign one.
        if (string.IsNullOrWhiteSpace(grr.DocNum))
        {
            try { grr.DocNum = _docNums.GenerateNextNumber("GoodsReturnRequest").DocNo; }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
        foreach (var line in grr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<GoodsReturnRequest>().Add(grr);
        await _db.SaveChangesAsync();

        var created = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Vendor)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == grr.Id);
        return CreatedAtAction(nameof(GetById), new { id = grr.Id },
            _mapper.Map<GoodsReturnRequestResponse>(created));
    }

    // ── POST /GoodsReturnRequest/{id}/Post ─── promotes a Draft request to
    // Open. No GL/inventory side effect — the Goods Return document itself
    // is what moves stock; this is just the status transition.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var grr = await _db.Set<GoodsReturnRequest>().FindAsync(id);
        if (grr == null) return NotFound();
        if (grr.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {grr.Status}).");

        var gate = await _approvalGate.EvaluateAsync("GoodsReturnRequest", grr.Id, grr.DocNum, grr.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            grr.Status    = "P";
            grr.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = grr.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        grr.Status    = "O";
        grr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Return Request posted", docNum = grr.DocNum });
    }

    // ── POST /GoodsReturnRequest/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var grr = await _db.Set<GoodsReturnRequest>().FindAsync(id);
        if (grr == null) return NotFound();
        if (grr.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {grr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("GoodsReturnRequest", grr.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        grr.Status    = "O";
        grr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Return Request approved and posted", docNum = grr.DocNum });
    }

    // ── POST /GoodsReturnRequest/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var grr = await _db.Set<GoodsReturnRequest>().FindAsync(id);
        if (grr == null) return NotFound();
        if (grr.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {grr.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("GoodsReturnRequest", grr.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        grr.Status    = "D";
        grr.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Return Request rejected", docNum = grr.DocNum });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReturnRequestUpdateRequest dto)
    {
        var grr = await _db.Set<GoodsReturnRequest>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (grr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(grr.Items);
        _mapper.Map(dto, grr);
        foreach (var line in grr.Items)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var grr = await _db.Set<GoodsReturnRequest>().FindAsync(id);
        if (grr == null) return NotFound();
        _db.Set<GoodsReturnRequest>().Remove(grr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
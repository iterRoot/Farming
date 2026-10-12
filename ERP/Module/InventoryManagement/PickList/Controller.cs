using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using SaleOrderEntity = FarmingApi.Modules.SaleAR.SaleOrder.SaleOrder;

namespace FarmingApi.Modules.InventoryManagement.PickList;

// ═══════════════════════════════════════════════════════════════════
// PICK LIST  (route: /PickList, also reached as /picklist)
// GET    /picklist                → all pick lists
// GET    /picklist/candidates     → open sales-order lines to pick
// GET    /picklist/{id}           → one, with lines
// POST   /picklist                → create from selected candidate lines
// PUT    /picklist/{id}           → revise header + picked quantities
// POST   /picklist/{id}/cancel    → set status Cancelled
// DELETE /picklist/{id}
//
// NOTE: creating or cancelling a pick list does not yet move stock or
// touch the source sales orders — it only records the picking sheet.
// ═══════════════════════════════════════════════════════════════════
public class PickListController : MyController
{
    private static readonly string[] Statuses = { "Released", "Closed", "Cancelled" };

    private readonly IMapper             _mapper;
    private readonly IPickListRepository _repository;
    private readonly MyDbContext         _db;

    public PickListController(IPickListRepository repository, IMapper mapper, MyDbContext db)
    {
        _mapper     = mapper;
        _repository = repository;
        _db         = db;
    }

    private string NextNumber()
    {
        var year   = DateTime.UtcNow.Year;
        var prefix = $"PICK-{year}-";
        var used   = _repository.GetAll()
            .Where(p => p.PickNumber.StartsWith(prefix))
            .Select(p => p.PickNumber)
            .ToList();
        var highest = 0;
        foreach (var n in used)
            if (int.TryParse(n[prefix.Length..], out var v) && v > highest) highest = v;
        return $"{prefix}{highest + 1:D5}";
    }

    // ── GET /picklist ─────────────────────────────────────────
    [HttpGet]
    public IActionResult Gets()
    {
        var query = _repository.GetAll().Include(p => p.Lines).OrderByDescending(p => p.Id);
        var rows  = _mapper.ProjectTo<PickListResponse>(query).ToList();
        return Ok(rows);
    }

    // ── GET /picklist/candidates ──────────────────────────────
    // Open sales-order lines. There is no partial-delivery tracking yet, so
    // OpenQty is the full ordered quantity of every line on an open order.
    [HttpGet("candidates")]
    public IActionResult Candidates([FromQuery] string? whsCode, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var orders = _db.Set<SaleOrderEntity>()
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Where(o => o.Status == "O");

        if (fromDate.HasValue) orders = orders.Where(o => o.DeliveryDate >= fromDate.Value);
        if (toDate.HasValue)   orders = orders.Where(o => o.DeliveryDate <= toDate.Value);

        var result = orders.ToList().SelectMany(o => o.Items
            .Where(l => l.Quantity > 0)
            .Select(l => new PickCandidate
            {
                Id              = l.Id,
                TransType       = "Sales Order",
                DocNo           = o.DocNum,
                BpCode          = o.Customer?.Code,
                DeliveryDueDate = o.DeliveryDate,
                ItemNo          = l.ItemCode,
                Description     = l.ItemName,
                UomCode         = null,
                UomName         = null,
                WhsCode         = null,
                OrderQty        = l.Quantity,
                OpenQty         = l.Quantity,
            }))
            .ToList();

        // whsCode filter is accepted for API compatibility; sales-order lines
        // carry no warehouse yet, so a specific warehouse matches nothing.
        if (!string.IsNullOrWhiteSpace(whsCode))
            result = result.Where(r => r.WhsCode == whsCode).ToList();

        return Ok(result);
    }

    // ── GET /picklist/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var entity = _repository.GetAll().Include(p => p.Lines).FirstOrDefault(p => p.Id == id);
        if (entity == null) return NotFound($"Pick list {id} not found");
        return Ok(_mapper.Map<PickListResponse>(entity));
    }

    // ── POST /picklist ────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] PickListCreateRequest request)
    {
        if (request.PickDate == null) return BadRequest("Pick date is required");
        if (string.IsNullOrWhiteSpace(request.User)) return BadRequest("User is required");
        if (request.Lines.Count == 0) return BadRequest("Add at least one line");

        var status = string.IsNullOrWhiteSpace(request.Status) ? "Released" : request.Status;
        if (!Statuses.Contains(status)) return BadRequest($"Status must be one of: {string.Join(", ", Statuses)}");

        var entity = new PickList
        {
            PickNumber = NextNumber(),
            PickDate   = request.PickDate,
            User       = request.User,
            Picker     = request.Picker,
            Status     = status,
            Remarks    = request.Remarks,
            CreatedAt  = DateTime.UtcNow,
            InActive   = false,
        };

        var lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<PickListLine>(lr);
            line.LineNum   = lineNum++;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive  = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Pick list created", id = entity.Id, pickNumber = entity.PickNumber });
    }

    // ── PUT /picklist/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PickListUpdateRequest request)
    {
        var entity = _repository.GetAll().Include(p => p.Lines).FirstOrDefault(p => p.Id == id);
        if (entity == null) return NotFound($"Pick list {id} not found");
        if (entity.Status == "Cancelled") return BadRequest("A cancelled pick list cannot be edited");

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Statuses.Contains(request.Status))
                return BadRequest($"Status must be one of: {string.Join(", ", Statuses)}");
            entity.Status = request.Status;
        }
        entity.User    = request.User   ?? entity.User;
        entity.Picker  = request.Picker ?? entity.Picker;
        entity.Remarks = request.Remarks;
        entity.UpdatedAt = DateTime.UtcNow;

        foreach (var lu in request.Lines)
        {
            var line = entity.Lines.FirstOrDefault(l => l.Id == lu.Id);
            if (line != null) line.PickedQty = lu.PickedQty;
        }

        _repository.Update(entity);
        _repository.Commit();
        return NoContent();
    }

    // ── POST /picklist/{id}/cancel ────────────────────────────
    [HttpPost("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(p => p.Id == id);
        if (entity == null) return NotFound($"Pick list {id} not found");
        if (entity.Status == "Cancelled") return BadRequest("Pick list is already cancelled");

        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Pick list cancelled", id });
    }

    // ── DELETE /picklist/{id} ─────────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(p => p.Id == id);
        if (entity == null) return NotFound($"Pick list {id} not found");

        entity.DeletedAt = DateTime.UtcNow;
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }
}

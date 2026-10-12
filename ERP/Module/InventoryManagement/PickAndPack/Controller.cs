using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using SO           = FarmingApi.Modules.SaleAR.SaleOrder.SaleOrder;
using ReserveInv   = FarmingApi.Modules.SaleAR.ARReserveInvoice.ARReserveInvoice;
using ProdOrder    = FarmingApi.Modules.InventoryManagement.ProductionOrder.ProductionOrder;
using TransferReq  = FarmingApi.Modules.Inventory.StockTransferRequest.StockTransferRequest;
using WarehouseEntity = FarmingApi.Modules.Sale.Warehouse.Warehouse;

namespace FarmingApi.Modules.InventoryManagement.PickAndPack;

// ══════════════════════════════════════════════════════════════════
// PICK & PACK  (route: /pickpack)
// GET    /pickpack/criteria            → all saved criteria
// GET    /pickpack/criteria/{id}       → one criteria (same as /pickpack/{id})
// GET    /pickpack/{id}                → one criteria + its pick lines
// GET    /pickpack/{id}/lines          → the pick lines
// POST   /pickpack/criteria            → save criteria (no lines yet)
// POST   /pickpack/generate            → save + generate a pick list
// PUT    /pickpack/criteria/{id}       → edit the criteria (lines are untouched until /run)
// POST   /pickpack/{id}/run            → regenerate lines (refused once anything is picked)
// POST   /pickpack/{id}/release        → release lines for picking
// PUT    /pickpack/{id}/lines          → record picked quantities
// DELETE /pickpack/criteria/{id}
//
// Every criterion is applied to what it selects: the four source documents
// (Sale Orders, Reserve Invoices, released Production Orders' components,
// open Stock Transfer Requests), the warehouse list, the range filters,
// grouping and sorting. A source line already on another active pick list
// is left out, so nothing is picked twice. Picking records quantities only —
// it doesn't move stock or close the source document.
// ══════════════════════════════════════════════════════════════════
[ApiController]
[Route("pickpack")]
public class PickPackController : ControllerBase
{
    private readonly IPickPackRepository _repo;
    private readonly MyDbContext _db;
    private readonly IMapper _mapper;

    public PickPackController(IPickPackRepository repo, MyDbContext db, IMapper mapper)
    {
        _repo = repo; _db = db; _mapper = mapper;
    }

    [HttpGet("criteria")]
    public IActionResult List()
    {
        var list = _repo.GetAll().Include(x => x.Lines)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ToList();
        return Ok(_mapper.Map<List<PickPackCriteriaResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id) => GetCriteria(id);

    [HttpGet("criteria/{id:int}")]
    public IActionResult GetCriteria(int id)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        var res = _mapper.Map<PickPackCriteriaResponse>(e);
        res.Lines = LineDtos(e);
        return Ok(res);
    }

    [HttpGet("{id:int}/lines")]
    public IActionResult GetLines(int id)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(LineDtos(e));
    }

    [HttpPost("criteria")]
    public IActionResult Save([FromBody] PickPackCriteriaRequest dto)
    {
        var error = ValidateCriteria(dto);
        if (error != null) return BadRequest(new { message = error });

        var e = _mapper.Map<PickPackCriteria>(dto);
        e.Status = "Open";
        e.CreatedAt = DateTime.UtcNow;
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { message = "Selection criteria saved.", id = e.Id });
    }

    [HttpPost("generate")]
    public IActionResult Generate([FromBody] PickPackCriteriaRequest dto)
    {
        var error = ValidateCriteria(dto);
        if (error != null) return BadRequest(new { message = error });

        var e = _mapper.Map<PickPackCriteria>(dto);
        e.CreatedAt = DateTime.UtcNow;
        _repo.Add(e);
        _repo.Commit();                       // the id is needed to keep it off its own exclusion list

        BuildLines(e);
        e.Status = e.ReleaseManually ? "Open" : "Released";
        _repo.Commit();
        return Ok(new { message = $"Generated {e.Lines.Count} pick line(s).", id = e.Id, lines = e.Lines.Count });
    }

    [HttpPut("criteria/{id:int}")]
    public IActionResult UpdateCriteria(int id, [FromBody] PickPackCriteriaRequest dto)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        if (e.Status is "Closed" or "Cancelled") return BadRequest(new { message = $"A {e.Status} pick list can't be edited." });
        var error = ValidateCriteria(dto);
        if (error != null) return BadRequest(new { message = error });

        var status = e.Status;
        _mapper.Map(dto, e);
        e.Status = status;
        // The only status a user sets is closing or cancelling; everything else follows from the lines.
        if (dto.Status is "Closed" or "Cancelled") e.Status = dto.Status;
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Commit();
        return Ok(new { message = "Selection criteria updated.", id = e.Id });
    }

    [HttpPost("{id:int}/run")]
    public IActionResult Run(int id)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        if (e.Status is "Closed" or "Cancelled") return BadRequest(new { message = $"A {e.Status} pick list can't be regenerated." });
        if (e.Lines.Any(l => l.PickedQty > 0))
            return BadRequest(new { message = "Something has already been picked on this list, so it can't be regenerated. Create a new pick list instead." });

        _db.RemoveRange(e.Lines);
        e.Lines.Clear();
        BuildLines(e);
        e.Status = e.ReleaseManually ? "Open" : "Released";
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Commit();
        return Ok(new { message = $"Regenerated {e.Lines.Count} pick line(s).", id = e.Id, lines = e.Lines.Count });
    }

    [HttpPost("{id:int}/release")]
    public IActionResult Release(int id, [FromBody] PickPackReleaseRequest? req)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        if (e.Status is "Closed" or "Cancelled") return BadRequest(new { message = $"A {e.Status} pick list can't be released." });

        var ids = req?.LineIds;
        var targets = (ids == null || ids.Count == 0 ? e.Lines.Where(l => l.Status == "Open") : e.Lines.Where(l => ids.Contains(l.Id))).ToList();
        if (ids is { Count: > 0 } && ids.Any(i => e.Lines.All(l => l.Id != i)))
            return BadRequest(new { message = "One of those lines isn't on this pick list." });
        var released = 0;
        foreach (var l in targets.Where(l => l.Status == "Open")) { l.Status = "Released"; released++; }
        if (released == 0) return BadRequest(new { message = "There are no unreleased lines to release." });

        RefreshStatus(e);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Commit();
        return Ok(new { message = $"Released {released} line(s).", released });
    }

    [HttpPut("{id:int}/lines")]
    public IActionResult SetPicked(int id, [FromBody] List<PickPackLineUpdate> updates)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        if (e.Status is "Closed" or "Cancelled") return BadRequest(new { message = $"A {e.Status} pick list can't be changed." });
        if (updates == null || updates.Count == 0) return BadRequest(new { message = "Nothing to save." });
        if (updates.GroupBy(u => u.Id).Any(g => g.Count() > 1)) return BadRequest(new { message = "A line appears more than once." });

        var byId = e.Lines.ToDictionary(l => l.Id);
        foreach (var u in updates)
        {
            if (!byId.TryGetValue(u.Id, out var l)) return BadRequest(new { message = $"Line {u.Id} isn't on this pick list." });
            if (u.PickedQty < 0) return BadRequest(new { message = $"{l.ItemCode}: picked quantity can't be negative." });
            if (u.PickedQty > l.RequiredQty) return BadRequest(new { message = $"{l.ItemCode}: can't pick more than the {l.RequiredQty:0.####} required." });
            if (u.PickedQty > 0 && l.Status == "Open") return BadRequest(new { message = $"{l.ItemCode}: release the line before picking it." });
        }
        foreach (var u in updates)
        {
            var l = byId[u.Id];
            l.PickedQty = u.PickedQty;
            l.Status = l.Status == "Open" ? "Open" : (u.PickedQty >= l.RequiredQty ? "Picked" : "Released");
            l.UpdatedAt = DateTime.UtcNow;
        }
        RefreshStatus(e);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Commit();
        return Ok(new { status = e.Status, picked = e.Lines.Count(l => l.Status == "Picked"), total = e.Lines.Count });
    }

    [HttpDelete("criteria/{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    // ── helpers ──────────────────────────────────────────────────
    private static void RefreshStatus(PickPackCriteria e)
    {
        if (e.Status is "Closed" or "Cancelled") return;
        if (e.Lines.Count > 0 && e.Lines.All(l => l.Status == "Picked")) e.Status = "Picked";
        else if (e.Lines.Any(l => l.Status is "Released" or "Picked")) e.Status = "Released";
        else e.Status = "Open";
    }

    private List<PickPackLineDto> LineDtos(PickPackCriteria e)
    {
        var dtos = _mapper.Map<List<PickPackLineDto>>(e.Lines.OrderBy(l => l.LineNum));
        foreach (var d in dtos)
            d.GroupKey = e.GroupBy switch
            {
                "Customer"  => d.CustomerName,
                "Item"      => d.ItemCode,
                "Warehouse" => d.WhsCode,
                _           => null,
            };
        return dtos;
    }

    private string? ValidateCriteria(PickPackCriteriaRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CriteriaName)) return "Criteria name is required";
        var m = dto.Manage;
        if (!m.SalesOrders && !m.ReserveInvoices && !m.ProductionOrders && !m.StockTransferRequests)
            return "Choose at least one source document to pick from.";
        if (!string.IsNullOrEmpty(dto.GroupBy) && !PickPackOptions.GroupBy.Contains(dto.GroupBy))
            return $"Group by must be one of: {string.Join(", ", PickPackOptions.GroupBy)}.";
        if (!string.IsNullOrEmpty(dto.SortBy) && !PickPackOptions.SortBy.Contains(dto.SortBy))
            return $"Sort by \"{dto.SortBy}\" isn't supported. Use one of: {string.Join(", ", PickPackOptions.SortBy)}.";

        if (dto.Warehouses.Count > 0)
        {
            var known = _db.Set<WarehouseEntity>().Select(w => w.Code.ToUpper()).ToHashSet();
            var unknown = dto.Warehouses.FirstOrDefault(w => !known.Contains((w ?? "").ToUpper()));
            if (unknown != null) return $"Warehouse '{unknown}' does not exist.";
        }

        foreach (var f in dto.Filters)
        {
            var hasFrom = !string.IsNullOrWhiteSpace(f.From);
            var hasTo   = !string.IsNullOrWhiteSpace(f.To);
            if (!hasFrom && !hasTo) continue;
            if (f.Field == "Fulfillment %" && IsFullRange(f)) continue;       // the untouched default: 0 to 100
            if (PickPackOptions.DateFields.Contains(f.Field ?? ""))
            {
                if (hasFrom && !DateTime.TryParse(f.From, out _)) return $"{f.Field}: '{f.From}' isn't a date.";
                if (hasTo && !DateTime.TryParse(f.To, out _)) return $"{f.Field}: '{f.To}' isn't a date.";
                if (hasFrom && hasTo && DateTime.Parse(f.From!) > DateTime.Parse(f.To!)) return $"{f.Field}: 'from' is after 'to'.";
            }
            else if (!PickPackOptions.TextFields.Contains(f.Field ?? ""))
                return $"The filter '{f.Field}' isn't supported. Supported: {string.Join(", ", PickPackOptions.TextFields.Concat(PickPackOptions.DateFields))}.";
        }
        return null;
    }

    private static bool IsFullRange(PickPackFilterDto f) =>
        decimal.TryParse(f.From, out var from) && decimal.TryParse(f.To, out var to) && from <= 0 && to >= 100;

    // One candidate pick line plus what filtering/sorting needs but isn't stored.
    private class Candidate
    {
        public PickPackLine Line = null!;
        public string CustomerCode = "";
        public DateTime? PostingDate;
    }

    private void BuildLines(PickPackCriteria e)
    {
        var candidates = new List<Candidate>();

        // Source lines already on another active pick list are taken.
        var taken = _db.Set<PickPackLine>()
            .Where(l => l.PickPackCriteriaId != e.Id && l.BaseLineId != null && l.PickPackCriteria.Status != "Cancelled")
            .Select(l => new { l.BaseDocType, l.BaseDocEntry, BaseLineId = l.BaseLineId!.Value })
            .ToList()
            .Select(t => (t.BaseDocType, t.BaseDocEntry, t.BaseLineId))
            .ToHashSet();
        bool Free(string type, int doc, int line) => !taken.Contains((type, doc, line));

        if (e.ManageSalesOrders)
        {
            foreach (var o in _db.Set<SO>().Include(o => o.Items).Include(o => o.Customer).Where(o => o.Status == "O").ToList())
                foreach (var l in o.Items.Where(l => Free("SaleOrder", o.Id, l.Id)))
                    candidates.Add(new Candidate
                    {
                        CustomerCode = o.Customer?.Code ?? "", PostingDate = o.PostingDate,
                        Line = new PickPackLine
                        {
                            BaseDocType = "SaleOrder", BaseDocEntry = o.Id, BaseLineId = l.Id, BaseDocNum = o.DocNum,
                            CustomerName = o.Customer?.CardName, DueDate = o.DeliveryDate,
                            ItemCode = l.ItemCode ?? "", ItemName = l.ItemName, WhsCode = l.WhsCode, RequiredQty = l.Quantity,
                        },
                    });
        }

        if (e.ManageReserveInvoices)
        {
            foreach (var o in _db.Set<ReserveInv>().Include(o => o.Items).Include(o => o.Customer).Where(o => o.Status == "O").ToList())
                foreach (var l in o.Items.Where(l => Free("ARReserveInvoice", o.Id, l.Id)))
                    candidates.Add(new Candidate
                    {
                        CustomerCode = o.Customer?.Code ?? "", PostingDate = o.PostingDate,
                        Line = new PickPackLine
                        {
                            BaseDocType = "ARReserveInvoice", BaseDocEntry = o.Id, BaseLineId = l.Id, BaseDocNum = o.DocNum,
                            CustomerName = o.Customer?.CardName, DueDate = o.DueDate,
                            ItemCode = l.ItemCode ?? "", ItemName = l.ItemName, WhsCode = l.WhsCode, RequiredQty = l.Quantity,
                        },
                    });
        }

        if (e.ManageProductionOrders)
        {
            // A released order needs its components picked from stock.
            foreach (var o in _db.Set<ProdOrder>().Include(o => o.Lines).Where(o => o.Status == "Released").ToList())
                foreach (var l in o.Lines.Where(l => Free("ProductionOrder", o.Id, l.Id)))
                    candidates.Add(new Candidate
                    {
                        CustomerCode = "", PostingDate = o.CreatedAt,
                        Line = new PickPackLine
                        {
                            BaseDocType = "ProductionOrder", BaseDocEntry = o.Id, BaseLineId = l.Id, BaseDocNum = o.DocNum,
                            CustomerName = $"Production: {o.ItemName}", DueDate = o.DueDate,
                            ItemCode = l.ItemCode, ItemName = l.ItemName, WhsCode = l.Warehouse, RequiredQty = l.PlannedQty,
                        },
                    });
        }

        if (e.ManageStockTransferRequests)
        {
            foreach (var r in _db.Set<TransferReq>().Include(r => r.Lines).Where(r => r.Status == "O" || r.Status == "A").ToList())
                foreach (var l in r.Lines.Where(l => l.LineStatus == "O" && Free("StockTransferRequest", r.Id, l.Id)))
                    candidates.Add(new Candidate
                    {
                        CustomerCode = r.CardCode ?? "", PostingDate = r.PostingDate,
                        Line = new PickPackLine
                        {
                            BaseDocType = "StockTransferRequest", BaseDocEntry = r.Id, BaseLineId = l.Id, BaseDocNum = r.DocNo,
                            CustomerName = r.CardName ?? $"Transfer to {r.ToWarehouse}", DueDate = r.DueDate,
                            ItemCode = l.ItemNo, ItemName = l.ItemDescription,
                            WhsCode = string.IsNullOrWhiteSpace(l.FromWarehouse) ? r.FromWarehouse : l.FromWarehouse,
                            RequiredQty = l.Quantity,
                        },
                    });
        }

        // Warehouses: when any are chosen, only lines picked from those.
        if (e.Warehouses.Count > 0)
        {
            var wanted = e.Warehouses.Select(w => w.ToUpper()).ToHashSet();
            candidates = candidates.Where(c => c.Line.WhsCode != null && wanted.Contains(c.Line.WhsCode.ToUpper())).ToList();
        }

        foreach (var f in e.Filters) candidates = candidates.Where(c => Matches(c, f)).ToList();

        var now = DateTime.UtcNow;
        foreach (var c in candidates)
        {
            c.Line.Status = e.ReleaseManually ? "Open" : "Released";
            c.Line.CreatedAt = now;
        }

        Func<Candidate, object?> sortKey = e.SortBy switch
        {
            "Customer"        => c => c.Line.CustomerName,
            "Posting Date"    => c => c.PostingDate ?? DateTime.MaxValue,
            "Sales Order No." => c => c.Line.BaseDocNum,
            _                 => c => c.Line.DueDate ?? DateTime.MaxValue,   // Delivery/Due Date
        };
        Func<Candidate, object?>? groupKey = e.GroupBy switch
        {
            "Customer"  => c => c.Line.CustomerName,
            "Item"      => c => c.Line.ItemCode,
            "Warehouse" => c => c.Line.WhsCode,
            _           => null,
        };
        var ordered = groupKey == null
            ? candidates.OrderBy(sortKey)
            : candidates.OrderBy(groupKey).ThenBy(sortKey);

        var n = 1;
        foreach (var c in ordered) { c.Line.LineNum = n++; e.Lines.Add(c.Line); }
    }

    private static bool Matches(Candidate c, PickPackFilter f)
    {
        var hasFrom = !string.IsNullOrWhiteSpace(f.From);
        var hasTo   = !string.IsNullOrWhiteSpace(f.To);
        if (!hasFrom && !hasTo) return true;
        if (f.Field == "Fulfillment %") return true;   // only the untouched 0–100 default gets here (validated on save)

        if (PickPackOptions.DateFields.Contains(f.Field ?? ""))
        {
            var date = f.Field == "Posting Date" ? c.PostingDate : c.Line.DueDate;   // "Required Date" = the date it's needed by
            if (date == null) return false;
            if (hasFrom && date.Value.Date < DateTime.Parse(f.From!).Date) return false;
            if (hasTo && date.Value.Date > DateTime.Parse(f.To!).Date) return false;
            return true;
        }

        // Text ranges compare case-insensitively; an upper bound also admits values that start with it.
        var values = f.Field switch
        {
            "Sales Order No." => new[] { c.Line.BaseDocNum ?? "" },
            "Customer"        => new[] { c.CustomerCode, c.Line.CustomerName ?? "" },
            "Item"            => new[] { c.Line.ItemCode },
            _                 => Array.Empty<string>(),
        };
        return values.Any(v =>
            (!hasFrom || string.Compare(v, f.From!.Trim(), StringComparison.OrdinalIgnoreCase) >= 0 || v.StartsWith(f.From!.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (!hasTo || string.Compare(v, f.To!.Trim(), StringComparison.OrdinalIgnoreCase) <= 0 || v.StartsWith(f.To!.Trim(), StringComparison.OrdinalIgnoreCase)));
    }
}

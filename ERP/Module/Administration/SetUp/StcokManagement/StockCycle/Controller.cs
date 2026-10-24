using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using JournalEntryNumberService = FarmingApi.Modules.Financials.JournalEntry.JournalEntryNumberService;
using ItemEntity       = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using InventoryBalance = FarmingApi.Modules.Sale.Inventory.Inventory;
using WarehouseEntity  = FarmingApi.Modules.Sale.Warehouse.Warehouse;
using GLDetermination  = FarmingApi.Modules.Financials.GLAccountDetermination.GLAccountDetermination;
using CoaEntity        = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;

namespace FarmingApi.Modules.Inventory.StockCycles;

// ═══════════════════════════════════════════════════════════════════
// STOCK CYCLE / INVENTORY COUNTING  (route: /StockCycle)
// Open → InProgress → Completed (or Cancelled)
//   POST /{id}/Start      → snapshots the book quantity of every item in the
//                           cycle's warehouse (or all warehouses) as count lines
//   POST /{id}/Snapshot   → re-take the snapshot (only while nothing is counted)
//   GET  /{id}/Lines      → lines with counted quantity, variance, live quantity
//   POST /{id}/Lines      → add an item found that wasn't in the snapshot
//   PUT  /{id}/Counts     → record counted quantities
//   POST /{id}/Complete   → posts each counted difference to stock and the G/L
// Completing sets each counted item's book quantity to what was counted, as of
// completion: adjustment = counted − book quantity right now. Stock shouldn't
// move while a count is running; lines whose book quantity changed since the
// snapshot are flagged in the lines list. Uncounted lines are left alone.
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class StockCycleController : ControllerBase
{
    private readonly IStockCycleRepository _repository;
    private readonly IMapper               _mapper;
    private readonly MyDbContext           _db;
    private readonly IInventoryPostingService _posting;

    public StockCycleController(IStockCycleRepository repository, IMapper mapper, MyDbContext db, IInventoryPostingService posting)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
        _posting    = posting;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines)
            .OrderByDescending(x => x.StartDate)
            .ToList();
        return Ok(_mapper.Map<List<StockCycleResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<StockCycleResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] StockCycleRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code))
            return BadRequest($"Stock cycle code '{code}' already exists");
        var whsError = CheckWarehouse(dto.WarehouseCode);
        if (whsError != null) return BadRequest(whsError);

        var entity       = _mapper.Map<StockCycle>(dto);
        entity.Code      = code;
        entity.Status    = "Open";          // a cycle's status only changes through its actions
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<StockCycleResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StockCycleRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status is "Completed" or "Cancelled")
            return BadRequest($"Cannot edit a {entity.Status} stock cycle");

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Stock cycle code '{code}' already exists");
        if (entity.Lines.Count > 0 && (dto.WarehouseCode ?? "") != (entity.WarehouseCode ?? ""))
            return BadRequest("The warehouse can't change once the count lines are taken.");
        var whsError = CheckWarehouse(dto.WarehouseCode);
        if (whsError != null) return BadRequest(whsError);

        var status = entity.Status;
        var existingCompleted = entity.CompletedDate;
        _mapper.Map(dto, entity);
        entity.Code          = code;
        entity.Status        = status;   // never settable from the request body
        entity.CompletedDate = existingCompleted;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repository.Commit();
        return Ok(_mapper.Map<StockCycleResponse>(entity));
    }

    // ── POST /StockCycle/{id}/Start ───────────────────────────
    [HttpPost("{id:int}/Start")]
    public IActionResult Start(int id)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Open") return BadRequest("Only Open cycles can be started");

        TakeSnapshot(entity);
        if (entity.Lines.Count == 0)
            return BadRequest("There is no stock to count in " + (entity.WarehouseCode == null ? "any warehouse." : $"warehouse {entity.WarehouseCode}."));

        entity.Status    = "InProgress";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { message = "Stock cycle started", code = entity.Code, lines = entity.Lines.Count });
    }

    // ── POST /StockCycle/{id}/Snapshot ────────────────────────
    [HttpPost("{id:int}/Snapshot")]
    public IActionResult Snapshot(int id)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "InProgress") return BadRequest("Only a cycle in progress can be re-snapshotted.");
        if (entity.Lines.Any(l => l.CountedQty != null))
            return BadRequest("Counts have already been entered, so the snapshot can't be retaken.");
        TakeSnapshot(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { lines = entity.Lines.Count });
    }

    // ── GET /StockCycle/{id}/Lines ────────────────────────────
    [HttpGet("{id:int}/Lines")]
    public IActionResult GetLines(int id)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var live = entity.Status == "InProgress"
            ? _db.Set<InventoryBalance>().ToList().ToDictionary(b => (b.ItemsCode, b.WhsCode), b => b.Balance)
            : null;

        return Ok(entity.Lines.OrderBy(l => l.ItemCode).ThenBy(l => l.WhsCode).Select(l => new StockCycleLineResponse
        {
            Id = l.Id, ItemCode = l.ItemCode, ItemName = l.ItemName, Uom = l.Uom, WhsCode = l.WhsCode,
            SystemQty = l.SystemQty, CountedQty = l.CountedQty,
            Variance = l.CountedQty == null ? null : l.CountedQty - l.SystemQty,
            CurrentQty = live == null ? null : live.GetValueOrDefault((l.ItemCode, l.WhsCode)),
            PostedQty = l.PostedQty, PostedUnitCost = l.PostedUnitCost,
        }));
    }

    // ── POST /StockCycle/{id}/Lines ── an item found that wasn't in the snapshot
    [HttpPost("{id:int}/Lines")]
    public IActionResult AddLine(int id, [FromBody] StockCycleAddLineRequest req)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "InProgress") return BadRequest("Lines can only be added while the cycle is in progress.");

        var item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == req.ItemCode);
        if (item == null) return BadRequest($"Item {req.ItemCode} not found.");
        var whsError = CheckWarehouse(req.WhsCode);
        if (whsError != null || string.IsNullOrWhiteSpace(req.WhsCode)) return BadRequest(whsError ?? "Warehouse is required.");
        if (entity.WarehouseCode != null && !string.Equals(entity.WarehouseCode, req.WhsCode, StringComparison.OrdinalIgnoreCase))
            return BadRequest($"This cycle counts warehouse {entity.WarehouseCode} only.");
        if (entity.Lines.Any(l => l.ItemCode == item.ItemCode && string.Equals(l.WhsCode, req.WhsCode, StringComparison.OrdinalIgnoreCase)))
            return BadRequest($"{item.ItemCode} in {req.WhsCode} is already on this count.");

        var book = _db.Set<InventoryBalance>().FirstOrDefault(b => b.ItemsCode == item.ItemCode && b.WhsCode == req.WhsCode)?.Balance ?? 0m;
        var whs = _db.Set<WarehouseEntity>().First(w => w.Code.ToUpper() == req.WhsCode.ToUpper()).Code;
        entity.Lines.Add(new StockCycleLine
        {
            ItemCode = item.ItemCode, ItemName = item.ItemName, Uom = string.IsNullOrWhiteSpace(item.UomCode) ? item.UomName : item.UomCode,
            WhsCode = whs, SystemQty = book,
        });
        _repository.Commit();
        return Ok(new { lines = entity.Lines.Count });
    }

    // ── PUT /StockCycle/{id}/Counts ───────────────────────────
    [HttpPut("{id:int}/Counts")]
    public IActionResult SetCounts(int id, [FromBody] List<StockCycleCountUpdate> counts)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "InProgress") return BadRequest("Counts can only be entered while the cycle is in progress.");
        if (counts == null || counts.Count == 0) return BadRequest("Nothing to save.");
        if (counts.GroupBy(c => c.LineId).Any(g => g.Count() > 1)) return BadRequest("A line appears more than once.");

        var byId = entity.Lines.ToDictionary(l => l.Id);
        foreach (var c in counts)
        {
            if (!byId.TryGetValue(c.LineId, out var line)) return BadRequest($"Line {c.LineId} isn't part of this cycle.");
            if (c.CountedQty < 0) return BadRequest($"The count for {line.ItemCode} can't be negative.");
        }
        foreach (var c in counts) byId[c.LineId].CountedQty = c.CountedQty;

        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { counted = entity.Lines.Count(l => l.CountedQty != null), total = entity.Lines.Count });
    }

    // ── POST /StockCycle/{id}/Complete ────────────────────────
    [HttpPost("{id:int}/Complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "InProgress")
            return BadRequest(entity.Status == "Completed" ? "Already completed" : $"Only a cycle in progress can be completed (this one is {entity.Status}).");

        var counted = entity.Lines.Where(l => l.CountedQty != null).ToList();
        if (counted.Count == 0) return BadRequest("Enter at least one counted quantity before completing.");

        var date = DateTime.UtcNow.Date;
        var lockError = PostingPeriodCheck.LockError(_db, date);
        if (lockError != null) return BadRequest(lockError);

        // Work out every adjustment first, against live book quantities.
        var plan = new List<(StockCycleLine Line, ItemEntity Item, decimal Diff, decimal Cost)>();
        foreach (var l in counted)
        {
            var item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == l.ItemCode);
            if (item == null) return BadRequest($"Item {l.ItemCode} no longer exists.");
            var book = _db.Set<InventoryBalance>().FirstOrDefault(b => b.ItemsCode == l.ItemCode && b.WhsCode == l.WhsCode)?.Balance ?? 0m;
            var diff = l.CountedQty!.Value - book;
            if (diff != 0 && FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchTracker.KindOf(item) != null)
                return BadRequest($"{item.ItemCode} is managed by batch/serial numbers, so a count difference ({diff:+0.####;-0.####}) can't be posted here. Correct it with a Goods Receipt or Goods Issue so the numbers are recorded.");
            // A shortage leaves stock at what the item's valuation method says it costs
            // (average, standard, or the FIFO layers it would consume); a find enters at its cost.
            var unit = diff < 0 ? _posting.PreviewIssueCost(item.ItemCode, l.WhsCode, -diff) / -diff : item.ItemCost;
            plan.Add((l, item, diff, unit));
        }

        var gain = Math.Round(plan.Where(p => p.Diff > 0).Sum(p => p.Diff * p.Cost), 2);
        var loss = Math.Round(plan.Where(p => p.Diff < 0).Sum(p => -p.Diff * p.Cost), 2);
        var gl = _db.Set<GLDetermination>().FirstOrDefault();
        if (gain > 0 || loss > 0)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(gl?.InventoryAccount)) missing.Add("Inventory Account");
            if (gain > 0 && string.IsNullOrWhiteSpace(gl?.InventoryCountingGain)) missing.Add("Inventory Counting Gain");
            if (loss > 0 && string.IsNullOrWhiteSpace(gl?.InventoryCountingLoss)) missing.Add("Inventory Counting Loss");
            if (missing.Count > 0) return BadRequest($"Set these in G/L Account Determination first: {string.Join(", ", missing)}.");

            var codes = new List<string> { gl!.InventoryAccount! };
            if (gain > 0) codes.Add(gl.InventoryCountingGain!);
            if (loss > 0) codes.Add(gl.InventoryCountingLoss!);
            foreach (var code in codes.Distinct())
            {
                var acct = _db.Set<CoaEntity>().FirstOrDefault(a => a.AcctCode == code);
                if (acct == null) return BadRequest($"G/L account {code} doesn't exist in the Chart of Accounts.");
                if (acct.IsGroup || !acct.IsActive || !acct.AllowPosting || acct.Frozen)
                    return BadRequest($"G/L account {code} can't be posted to (it's a title, inactive, frozen, or blocks posting).");
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var (line, item, diff, cost) in plan)
            {
                line.PostedQty = diff;
                line.PostedUnitCost = cost;
                if (diff > 0)
                    await _posting.PostInAsync(item.ItemCode, line.WhsCode, diff, cost, "Inventory Counting", "StockCycle", entity.Id, entity.Code, date);
                else if (diff < 0)
                    await _posting.PostOutAsync(item.ItemCode, line.WhsCode, -diff, "Inventory Counting", "StockCycle", entity.Id, entity.Code, date);
            }

            JournalEntryDoc? je = null;
            if (gain > 0 || loss > 0)
            {
                je = BuildJournalEntry(entity, gl!, plan.Select(p => (p.Item.ItemCode, Value: Math.Round(p.Diff * p.Cost, 2))).ToList(), date);
                _db.Set<JournalEntryDoc>().Add(je);
                await _db.SaveChangesAsync();
                entity.JournalEntryId = je.Id;
            }

            entity.Status        = "Completed";
            entity.CompletedDate = DateTime.UtcNow;
            entity.UpdatedAt     = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new
            {
                message = "Stock cycle completed", code = entity.Code,
                adjusted = plan.Count(p => p.Diff != 0), unchanged = plan.Count(p => p.Diff == 0),
                uncounted = entity.Lines.Count - counted.Count,
                gainValue = gain, lossValue = loss,
                journalEntryId = je?.Id, journalNo = je?.JrnlNo,
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ── POST /StockCycle/{id}/Cancel ──────────────────────────
    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Completed") return BadRequest("Cannot cancel a Completed cycle");
        if (entity.Status == "Cancelled") return BadRequest("Already cancelled");

        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Stock cycle cancelled", code = entity.Code });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Completed")
            return BadRequest("Cannot delete a Completed stock cycle");
        _repository.Remove(entity);   // its count lines go with it (cascade)
        _repository.Commit();
        return NoContent();
    }

    // ── helpers ───────────────────────────────────────────────
    private string? CheckWarehouse(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        return _db.Set<WarehouseEntity>().Any(w => w.Code.ToUpper() == code.Trim().ToUpper())
            ? null : $"Warehouse '{code}' does not exist.";
    }

    // Replaces the cycle's lines with the book quantity of every item held in scope.
    private void TakeSnapshot(StockCycle cycle)
    {
        _db.Set<StockCycleLine>().RemoveRange(cycle.Lines.ToList());
        cycle.Lines.Clear();

        var balances = _db.Set<InventoryBalance>()
            .Where(b => cycle.WarehouseCode == null || b.WhsCode == cycle.WarehouseCode)
            .ToList();
        var codes = balances.Select(b => b.ItemsCode).Distinct().ToList();
        var items = _db.Set<ItemEntity>().Where(i => codes.Contains(i.ItemCode)).ToDictionary(i => i.ItemCode);
        foreach (var b in balances.OrderBy(b => b.ItemsCode).ThenBy(b => b.WhsCode))
        {
            if (!items.TryGetValue(b.ItemsCode, out var item)) continue;
            cycle.Lines.Add(new StockCycleLine
            {
                ItemCode = item.ItemCode, ItemName = item.ItemName,
                Uom = string.IsNullOrWhiteSpace(item.UomCode) ? item.UomName : item.UomCode,
                WhsCode = b.WhsCode, SystemQty = b.Balance,
            });
        }
    }

    private JournalEntryDoc BuildJournalEntry(StockCycle cycle, GLDetermination gl, List<(string ItemCode, decimal Value)> values, DateTime date)
    {
        var je = new JournalEntryDoc
        {
            JrnlNo = JournalEntryNumberService.Next(_db, date), TransDate = date, DueDate = date, DocDate = date,
            RefNo = cycle.Code, Memo = $"Inventory counting {cycle.Code}", TransType = "KSTC", BaseDocType = "KSTC",
            BaseDocEntry = cycle.Id, ExchangeRate = 1, Status = "O", IsAutoGenerated = true, VersionNum = 1,
            CreatedAt = DateTime.UtcNow, InActive = false,
        };
        var names = _db.Set<CoaEntity>().Where(a => a.AcctCode == gl.InventoryAccount || a.AcctCode == gl.InventoryCountingGain || a.AcctCode == gl.InventoryCountingLoss)
            .ToDictionary(a => a.AcctCode, a => a.AcctName);
        var n = 1;
        void Add(string acct, decimal dr, decimal cr, string memo) => je.Lines.Add(new JournalEntryLine
        {
            LineNum = n++, AccountCode = acct, AccountName = names.GetValueOrDefault(acct, acct), Debit = dr, Credit = cr,
            DebitLC = dr, CreditLC = cr, LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
        });

        foreach (var (itemCode, value) in values.Where(v => v.Value != 0))
        {
            var memo = $"{cycle.Code} — {itemCode}";
            if (value > 0) { Add(gl.InventoryAccount!, value, 0, memo); Add(gl.InventoryCountingGain!, 0, value, memo); }
            else           { Add(gl.InventoryCountingLoss!, -value, 0, memo); Add(gl.InventoryAccount!, 0, -value, memo); }
        }
        je.TotalDebit = je.Lines.Sum(l => l.Debit); je.TotalCredit = je.Lines.Sum(l => l.Credit);
        je.TotalLC = je.TotalDebit; je.TotalFC = je.TotalDebit;
        return je;
    }
}

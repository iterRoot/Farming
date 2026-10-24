using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using InventoryBalanceRow = FarmingApi.Modules.Sale.Inventory.Inventory;

namespace FarmingApi.Modules.Inventory.ItemsMaster;

[ApiController]
[Route("[controller]")]
public class ItemsMasterController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IItemsMasterRepository _repository;
    private readonly MyDbContext _db;
    private readonly IInventoryPostingService _posting;

    public ItemsMasterController(IItemsMasterRepository repository, IMapper mapper, MyDbContext db, IInventoryPostingService posting)
    {
        _repository = repository;
        _mapper = mapper;
        _db = db;
        _posting = posting;
    }

    // ═══════════════════════════════════════════════════════════
    // Valuation method
    // ═══════════════════════════════════════════════════════════
    private const string BadMethod = "Valuation method must be Moving Average, Standard or FIFO.";

    // Switching to FIFO seeds one opening layer per warehouse holding stock, at the
    // item's current cost; switching away re-prices the item at the layers' weighted
    // average and drops the layers. Stock value is unchanged either way.
    private string? ApplyMethodSwitch(ItemsMaster item, string oldMethod, string newMethod)
    {
        if (oldMethod == newMethod) return null;

        if (newMethod == Valuation.Fifo)
        {
            var old = _db.Set<InventoryCostLayer>().Where(l => l.ItemCode == item.ItemCode).ToList();
            _db.Set<InventoryCostLayer>().RemoveRange(old);
            foreach (var b in _db.Set<InventoryBalanceRow>().Where(b => b.ItemsCode == item.ItemCode && b.Balance > 0).ToList())
            {
                _db.Set<InventoryCostLayer>().Add(new InventoryCostLayer
                {
                    ItemCode = item.ItemCode, WhsCode = b.WhsCode, ReceivedDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),   // pre-existing stock is always the oldest
                    SourceDocType = "Opening", SourceDocEntry = 0, SourceDocNum = "Switched to FIFO",
                    InitialQty = b.Balance, RemainingQty = b.Balance, UnitCost = item.ItemCost,
                });
            }
        }
        else if (oldMethod == Valuation.Fifo)
        {
            var layers = _db.Set<InventoryCostLayer>().Where(l => l.ItemCode == item.ItemCode).ToList();
            var open = layers.Where(l => l.RemainingQty > 0).ToList();
            var qty = open.Sum(l => l.RemainingQty);
            if (qty > 0) item.ItemCost = open.Sum(l => l.RemainingQty * l.UnitCost) / qty;
            _db.Set<InventoryCostLayer>().RemoveRange(layers);
        }
        return null;
    }

    // "None", "Batch" or "Serial" (the item form also sends the long names).
    private static string? CanonManageBy(string? v)
    {
        var t = (v ?? "").Trim();
        if (t.Length == 0 || t.Equals("None", StringComparison.OrdinalIgnoreCase)) return "None";
        if (t.StartsWith("Batch", StringComparison.OrdinalIgnoreCase)) return "Batch";
        if (t.StartsWith("Serial", StringComparison.OrdinalIgnoreCase)) return "Serial";
        return null;
    }
    private const string BadManageBy = "Manage Item by must be None, Batch Numbers or Serial Numbers.";

    private decimal OnHandQty(string itemCode) =>
        _db.Set<InventoryBalanceRow>().Where(b => b.ItemsCode == itemCode).Sum(b => (decimal?)b.Balance) ?? 0m;

    // GET /ItemsMaster/{id}/Valuation — what the item is worth and how that is arrived at.
    [HttpGet("{id:int}/Valuation")]
    public IActionResult ValuationSummary(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound();
        var method = InventoryManagement.InventoryJournal.Valuation.Of(item.ValuationMethod);
        var balances = _db.Set<InventoryBalanceRow>().Where(b => b.ItemsCode == item.ItemCode).OrderBy(b => b.WhsCode).ToList();
        var layers = method == InventoryManagement.InventoryJournal.Valuation.Fifo ? _posting.OpenLayers(item.ItemCode) : new List<InventoryCostLayer>();

        return Ok(new
        {
            itemCode = item.ItemCode,
            method,
            itemCost = item.ItemCost,
            onHand = balances.Sum(b => b.Balance),
            value = method == InventoryManagement.InventoryJournal.Valuation.Fifo
                ? layers.Sum(l => l.RemainingQty * l.UnitCost)
                : balances.Sum(b => b.Balance) * item.ItemCost,
            warehouses = balances.Select(b => new
            {
                whsCode = b.WhsCode,
                qty = b.Balance,
                value = method == InventoryManagement.InventoryJournal.Valuation.Fifo
                    ? layers.Where(l => l.WhsCode == b.WhsCode).Sum(l => l.RemainingQty * l.UnitCost)
                    : b.Balance * item.ItemCost,
            }),
            layers = layers.Select(l => new
            {
                l.WhsCode, l.ReceivedDate, l.SourceDocType, l.SourceDocNum,
                l.InitialQty, l.RemainingQty, l.UnitCost, value = l.RemainingQty * l.UnitCost,
            }),
        });
    }

    // ═══════════════════════════════════════════════════════════
    // Transaction usage
    // ═══════════════════════════════════════════════════════════
    // Counts every row in the database that points at this item, either by
    // ItemId (FK to KITM) or by ItemCode (line tables with no FK, e.g.
    // GoodsIssueLine / GoodsReceiptLine). Driven off the catalog, so new
    // document modules are covered automatically.
    private const string UsageSql = @"
SELECT COALESCE(SUM(cnt), 0)::int AS ""Value""
FROM (
    SELECT (xpath('/row/c/text()', query_to_xml(
        format('SELECT COUNT(*) AS c FROM public.%I WHERE %I = %L', t.table_name, t.column_name, {0}),
        false, true, '')))[1]::text::bigint AS cnt
    FROM (
        SELECT tc.table_name, kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
             ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu
             ON tc.constraint_name = ccu.constraint_name
        WHERE tc.constraint_type = 'FOREIGN KEY' AND ccu.table_name = 'KITM'
    ) t

    UNION ALL

    SELECT (xpath('/row/c/text()', query_to_xml(
        format('SELECT COUNT(*) AS c FROM public.%I WHERE %I = %L', c.table_name, c.column_name, {1}),
        false, true, '')))[1]::text::bigint AS cnt
    FROM (
        SELECT table_name, column_name
        FROM information_schema.columns
        WHERE column_name = 'ItemCode'
          AND table_schema = 'public'
          AND table_name <> 'KITM'
    ) c
) s;";

    private int CountTransactions(int itemId, string itemCode) =>
        _db.Database.SqlQueryRaw<int>(UsageSql, itemId, itemCode ?? "").AsEnumerable().First();

    // ── GET /ItemsMaster/{id}/Usage ────────────────────────────
    // Tells the UI whether the Item Code may still be edited.
    [HttpGet("{id:int}/Usage")]
    public IActionResult Usage(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");

        var count = CountTransactions(item.Id, item.ItemCode);
        return Ok(new
        {
            itemCode          = item.ItemCode,
            transactionCount  = count,
            hasTransactions   = count > 0,
            canEditItemCode   = count == 0,
        });
    }

    // GET ALL
    [HttpGet]
    public IActionResult Gets()
    {
        var data = _repository.GetAll();
        var result = _mapper.ProjectTo<ItemsMasterResponse>(data).ToList();
        return Ok(result);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound();

        return Ok(_mapper.Map<ItemsMasterResponse>(item));
    }

    // ── POST /ItemsMaster/Barcodes/Generate ── gives every item that has no barcode a
    //    generated one (existing barcodes are never changed). Safe to run again.
    [HttpPost("Barcodes/Generate")]
    public IActionResult GenerateMissingBarcodes()
    {
        var count = ItemBarcodes.FillMissing(_db);
        if (count > 0) _db.SaveChanges();
        return Ok(new { message = count == 0 ? "Every item already has a barcode." : $"Generated barcodes for {count} item(s).", generated = count });
    }

    // CREATE
    [HttpPost]
    public IActionResult Create([FromBody] ItemsMasterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemCode) ||
            string.IsNullOrWhiteSpace(request.ItemName))
        {
            return BadRequest("ItemCode and ItemName are required");
        }

        var exists = _repository.GetAll()
            .Any(x => x.ItemCode == request.ItemCode);

        if (exists)
            return BadRequest("Item Code already exists");

        var method = InventoryManagement.InventoryJournal.Valuation.Canonical(string.IsNullOrWhiteSpace(request.ValuationMethod) ? "MovingAverage" : request.ValuationMethod);
        if (method == null) return BadRequest(BadMethod);

        var manageBy = CanonManageBy(request.ManageBy);
        if (manageBy == null) return BadRequest(BadManageBy);

        var barcodeError = ItemBarcodes.CheckUnique(_db, request.Barcode, null);
        if (barcodeError != null) return BadRequest(barcodeError);

        var entity = _mapper.Map<ItemsMaster>(request);
        entity.Barcode = (request.Barcode ?? "").Trim();
        entity.ValuationMethod = method;
        entity.ManageBy = manageBy;
        entity.CreatedAt = DateTime.UtcNow;

        _repository.Add(entity);
        _repository.Commit();

        // No barcode typed or scanned in → generate one (it's built from the new Id).
        if (ItemBarcodes.FillMissing(_db, new[] { entity }) > 0) _repository.Commit();

        return Ok(new
        {
            message = "Created successfully",
            id = entity.Id,
            barcode = entity.Barcode,
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ItemsMasterUpdateRequest request)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound();

        var currentCode = item.ItemCode;
        var newCode     = (request.ItemCode ?? "").Trim();
        var codeChanged = newCode.Length > 0 &&
                          !string.Equals(newCode, currentCode, StringComparison.Ordinal);

        if (codeChanged)
        {
            // Once an item is used on a document its code is frozen —
            // renaming it would orphan every line that references it.
            var used = CountTransactions(item.Id, currentCode);
            if (used > 0)
                return BadRequest(
                    $"Item Code cannot be changed: '{currentCode}' is used on {used} transaction line(s).");

            if (_repository.GetSingle(x => x.ItemCode == newCode && x.Id != id) != null)
                return BadRequest($"Item Code '{newCode}' already exists.");
        }

        var oldMethod = InventoryManagement.InventoryJournal.Valuation.Of(item.ValuationMethod);
        var newMethod = InventoryManagement.InventoryJournal.Valuation.Canonical(string.IsNullOrWhiteSpace(request.ValuationMethod) ? item.ValuationMethod : request.ValuationMethod);
        if (newMethod == null) return BadRequest(BadMethod);

        // With stock on hand, the cost of a standard or FIFO item moves only through a
        // Stock Revaluation — editing it here would change stock value without a journal entry.
        var oldCost = item.ItemCost;
        var stockQty = OnHandQty(currentCode);
        if (newMethod != InventoryManagement.InventoryJournal.Valuation.MovingAverage && stockQty != 0 && request.ItemCost != oldCost)
            return BadRequest($"{currentCode} has {stockQty:0.####} in stock, so its cost can't be edited here. Use Stock Revaluation to change the cost of a {newMethod} item.");

        var oldManage = CanonManageBy(item.ManageBy) ?? "None";
        var newManage = CanonManageBy(request.ManageBy);
        if (newManage == null) return BadRequest(BadManageBy);
        // Numbers can only be tracked from an empty shelf: existing stock has none to show.
        if (newManage != oldManage && stockQty != 0)
            return BadRequest($"{currentCode} has {stockQty:0.####} in stock, so it can't switch to or from batch/serial numbers. Empty its stock first.");

        var oldBarcode = item.Barcode;
        var newBarcode = (request.Barcode ?? "").Trim();
        if (newBarcode.Length > 0 && newBarcode != oldBarcode)
        {
            var barcodeError = ItemBarcodes.CheckUnique(_db, newBarcode, item.Id);
            if (barcodeError != null) return BadRequest(barcodeError);
        }

        _mapper.Map(request, item);
        item.Barcode = newBarcode.Length > 0 ? newBarcode : oldBarcode;   // a blank field never wipes the barcode
        if (string.IsNullOrWhiteSpace(item.Barcode)) ItemBarcodes.FillMissing(_db, new[] { item });
        item.ManageBy = newManage;
        item.ItemCode  = codeChanged ? newCode : currentCode;   // never blanked by the map
        item.ValuationMethod = newMethod;
        item.UpdatedAt = DateTime.UtcNow;
        if (newMethod != InventoryManagement.InventoryJournal.Valuation.MovingAverage && stockQty != 0) item.ItemCost = oldCost;
        ApplyMethodSwitch(item, oldMethod, newMethod);

        _repository.Update(item);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════
    // BULK IMPORT — from an Excel/CSV file parsed on the client.
    // Body: { mode: "skip"|"update"|"reject", items: [ ...core fields ] }
    // ═══════════════════════════════════════════════════════════
    [HttpPost("Import")]
    public IActionResult Import([FromBody] ItemImportRequest request)
    {
        var mode = (request?.Mode ?? "skip").Trim().ToLowerInvariant();
        if (mode != "skip" && mode != "update" && mode != "reject")
            return BadRequest("Mode must be one of: skip, update, reject");

        var items = request?.Items ?? new List<ItemImportRow>();
        var result = new ItemImportResult { Mode = mode, Total = items.Count, Committed = false };

        if (items.Count == 0)
            return BadRequest("No rows to import");

        // Snapshot of existing codes → entity, for duplicate detection / update.
        var existing = _repository.GetAll().ToDictionary(x => x.ItemCode, x => x, StringComparer.OrdinalIgnoreCase);
        var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // barcode → item code that owns it, so a file can't give two items the same barcode
        var barcodeOwner = _repository.GetAll().Where(x => x.Barcode != null && x.Barcode != "")
            .Select(x => new { x.Barcode, x.ItemCode }).ToList()
            .GroupBy(x => x.Barcode).ToDictionary(g => g.Key, g => g.First().ItemCode);

        // Normalize + shallow-validate every row first (row number is 1-based).
        var prepared = new List<(int Row, ItemImportRow Data, string Code, string? Error)>();
        for (int i = 0; i < items.Count; i++)
        {
            var row  = items[i];
            var code = (row.ItemCode ?? "").Trim();
            var name = (row.ItemName ?? "").Trim();
            var barcode = (row.Barcode ?? "").Trim();

            string? error = null;
            if (code.Length == 0)                          error = "ItemCode is required";
            else if (name.Length == 0)                     error = "ItemName is required";
            else if (!seenInFile.Add(code))                error = $"Duplicate ItemCode '{code}' within the file";
            else if (barcode.Length > 0 && barcodeOwner.TryGetValue(barcode, out var owner)
                     && !string.Equals(owner, code, StringComparison.OrdinalIgnoreCase))
                error = $"Barcode '{barcode}' is already used by item {owner}";
            if (error == null && barcode.Length > 0) barcodeOwner[barcode] = code;

            prepared.Add((i + 1, row, code, error));
        }

        // Reject mode: if ANY row clashes with an existing code (or is invalid),
        // cancel the whole import and report — write nothing.
        if (mode == "reject")
        {
            var clashes = prepared
                .Where(p => p.Error == null && existing.ContainsKey(p.Code))
                .Select(p => p.Code).ToList();
            var invalids = prepared.Where(p => p.Error != null).ToList();

            if (clashes.Count > 0 || invalids.Count > 0)
            {
                foreach (var p in prepared)
                {
                    var msg = p.Error
                              ?? (existing.ContainsKey(p.Code) ? "ItemCode already exists — import cancelled" : "Not imported (file rejected)");
                    result.Rows.Add(new ItemImportRowResult { Row = p.Row, ItemCode = p.Code, Status = "failed", Message = msg });
                }
                result.Failed = result.Rows.Count;
                return Ok(result);  // Committed = false
            }
        }

        // Apply each row.
        foreach (var p in prepared)
        {
            if (p.Error != null)
            {
                result.Failed++;
                result.Rows.Add(new ItemImportRowResult { Row = p.Row, ItemCode = p.Code, Status = "failed", Message = p.Error });
                continue;
            }

            if (existing.TryGetValue(p.Code, out var current))
            {
                if (mode == "skip")
                {
                    result.Skipped++;
                    result.Rows.Add(new ItemImportRowResult { Row = p.Row, ItemCode = p.Code, Status = "skipped", Message = "ItemCode already exists" });
                    continue;
                }
                // update
                ApplyCore(current, p.Data);
                current.UpdatedAt = DateTime.UtcNow;
                _repository.Update(current);
                result.Updated++;
                result.Rows.Add(new ItemImportRowResult { Row = p.Row, ItemCode = p.Code, Status = "updated" });
            }
            else
            {
                var entity = new ItemsMaster { ItemCode = p.Code, CreatedAt = DateTime.UtcNow };
                ApplyCore(entity, p.Data);
                _repository.Add(entity);
                existing[p.Code] = entity;   // guard against later duplicate inserts
                result.Created++;
                result.Rows.Add(new ItemImportRowResult { Row = p.Row, ItemCode = p.Code, Status = "created" });
            }
        }

        _repository.Commit();
        if (ItemBarcodes.FillMissing(_db) > 0) _repository.Commit();
        result.Committed = true;
        return Ok(result);
    }

    // Copies only the supplied core fields onto an entity. Nulls are ignored so
    // an update never blanks a field the spreadsheet left empty.
    private static void ApplyCore(ItemsMaster e, ItemImportRow r)
    {
        if (!string.IsNullOrWhiteSpace(r.ItemName))  e.ItemName  = r.ItemName.Trim();
        if (!string.IsNullOrWhiteSpace(r.ItemGroup)) e.ItemGroup = r.ItemGroup.Trim();
        if (!string.IsNullOrWhiteSpace(r.ItemType))  e.ItemType  = r.ItemType.Trim();
        if (!string.IsNullOrWhiteSpace(r.Barcode))   e.Barcode   = r.Barcode.Trim();
        if (!string.IsNullOrWhiteSpace(r.UomGroup))  e.UomGroup  = r.UomGroup.Trim();
        if (!string.IsNullOrWhiteSpace(r.UomName))   e.UomName   = r.UomName.Trim();
        if (r.SalesItem.HasValue)    e.SalesItem    = r.SalesItem.Value;
        if (r.PurchaseItem.HasValue) e.PurchaseItem = r.PurchaseItem.Value;
        if (r.StockItem.HasValue)    e.StockItem    = r.StockItem.Value;
        if (r.UnitPrice.HasValue)    e.UnitPrice    = r.UnitPrice.Value;
    }

    // DELETE
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound();

        _repository.Remove(item);
        _repository.Commit();

        return NoContent();
    }
}
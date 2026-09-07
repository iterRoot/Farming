using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;

namespace FarmingApi.Modules.Inventory.ItemsMaster;

[ApiController]
[Route("[controller]")]
public class ItemsMasterController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IItemsMasterRepository _repository;
    private readonly MyDbContext _db;

    public ItemsMasterController(IItemsMasterRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper = mapper;
        _db = db;
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

        var entity = _mapper.Map<ItemsMaster>(request);
        entity.CreatedAt = DateTime.UtcNow;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message = "Created successfully",
            id = entity.Id
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

        _mapper.Map(request, item);
        item.ItemCode  = codeChanged ? newCode : currentCode;   // never blanked by the map
        item.UpdatedAt = DateTime.UtcNow;

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

        // Normalize + shallow-validate every row first (row number is 1-based).
        var prepared = new List<(int Row, ItemImportRow Data, string Code, string? Error)>();
        for (int i = 0; i < items.Count; i++)
        {
            var row  = items[i];
            var code = (row.ItemCode ?? "").Trim();
            var name = (row.ItemName ?? "").Trim();

            string? error = null;
            if (code.Length == 0)                          error = "ItemCode is required";
            else if (name.Length == 0)                     error = "ItemName is required";
            else if (!seenInFile.Add(code))                error = $"Duplicate ItemCode '{code}' within the file";

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
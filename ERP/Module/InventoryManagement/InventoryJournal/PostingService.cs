using Microsoft.EntityFrameworkCore;
using FarmingApi.Modules.Inventory.ItemsMaster;
using WarehouseEntity  = FarmingApi.Modules.Sale.Warehouse.Warehouse;
using InventoryBalance = FarmingApi.Modules.Sale.Inventory.Inventory;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

// ═══════════════════════════════════════════════════════════════
// Shared stock-posting logic called from every document that moves
// inventory (Goods Receipt PO, Delivery, ...). For each movement it:
//   1. writes one InventoryJournal row (the audit trail),
//   2. updates the running Inventory balance row for that item+warehouse,
//   3. values the movement by the item's valuation method:
//        MovingAverage — inbound blends into the item's average; outbound at the average.
//        Standard      — the item's cost is a fixed standard: receipts enter at standard and the
//                        difference is recorded as a price variance; issues leave at standard.
//        FIFO          — each receipt is a cost layer (KCLY); issues consume the oldest layers
//                        first and are valued at what those layers cost.
// Callers add these calls inside their own existing DB transaction and
// SaveChangesAsync — this service only tracks entities, it never saves.
// ═══════════════════════════════════════════════════════════════
public interface IInventoryPostingService
{
    // Throws if no warehouse is configured — see the implementation for why.
    Task<string> GetDefaultWarehouseCodeAsync();

    /// <summary>
    /// Returns <paramref name="requested"/> when it names a real warehouse,
    /// or the default warehouse when it is null/blank. Throws on an unknown code.
    /// </summary>
    Task<string> ResolveWarehouseCodeAsync(string? requested);

    Task PostInAsync(string itemCode, string whsCode, decimal qty, decimal unitCost,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null, IReadOnlyList<BatchLine>? batches = null);

    /// <summary>
    /// Brings returned goods back into stock at the item's current moving-average
    /// cost, so a return does not move the average. Never post a return at the
    /// selling price — that would inflate the item's cost.
    /// </summary>
    Task PostReturnInAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null, IReadOnlyList<BatchLine>? batches = null);

    /// <summary>Posts stock out and returns the total cost the movement was valued at.</summary>
    Task<decimal> PostOutAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null, IReadOnlyList<BatchLine>? batches = null, bool allowExpired = false);

    /// <summary>
    /// Checks a line's batch/serial numbers without posting anything — call it for every line of a
    /// document (sharing one tally) before saving, so a bad number is refused before the document exists.
    /// Returns an error message, or null when the line is fine.
    /// </summary>
    Task<string?> CheckBatchesAsync(BatchTally tally, string itemCode, string whsCode, decimal qty,
        IReadOnlyList<BatchLine>? batches, BatchFlow flow, DateTime postingDate);

    /// <summary>
    /// What an issue of this quantity would be valued at right now, without posting it —
    /// used by documents that build their cost-of-goods entry before the stock moves.
    /// </summary>
    decimal PreviewIssueCost(string itemCode, string whsCode, decimal qty);

    /// <summary>Re-prices every open cost layer of a FIFO item (used by Stock Revaluation).</summary>
    void RevalueLayers(string itemCode, decimal newCost);

    /// <summary>Open layers, oldest first, including ones added earlier in this unit of work.</summary>
    List<InventoryCostLayer> OpenLayers(string itemCode, string? whsCode = null);
}

public static class Valuation
{
    public const string MovingAverage = "MovingAverage";
    public const string Standard      = "Standard";
    public const string Fifo          = "FIFO";
    public static readonly string[] All = { MovingAverage, Standard, Fifo };

    public static string Of(string? method) =>
        (method ?? "").Replace(" ", "").ToUpperInvariant() switch
        {
            "FIFO"     => Fifo,
            "STANDARD" => Standard,
            _          => MovingAverage,
        };

    /// <summary>The canonical name for a method typed by a user, or null when it isn't one of ours.</summary>
    public static string? Canonical(string? method)
    {
        var key = (method ?? "").Replace(" ", "").ToUpperInvariant();
        return key switch
        {
            "MOVINGAVERAGE" => MovingAverage,
            "STANDARD" or "STANDARDCOST" => Standard,
            "FIFO" => Fifo,
            _ => null,
        };
    }
}

public class InventoryPostingService : IInventoryPostingService
{
    private readonly MyDbContext _db;
    private readonly BatchTracker _batches;
    public InventoryPostingService(MyDbContext db) { _db = db; _batches = new BatchTracker(db); }

    public async Task<string?> CheckBatchesAsync(BatchTally tally, string itemCode, string whsCode, decimal qty,
        IReadOnlyList<BatchLine>? batches, BatchFlow flow, DateTime postingDate)
    {
        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode);
        if (item == null) return $"Item {itemCode} not found.";
        return await _batches.ValidateAsync(tally, item, whsCode, qty, batches, flow, postingDate);
    }

    // A stock movement must land in a warehouse. If none is configured we throw
    // rather than return null — silently skipping the posting would let the
    // document and its journal entry save while the stock ledger recorded
    // nothing, leaving the G/L and the ledger permanently out of sync.
    public async Task<string> GetDefaultWarehouseCodeAsync()
    {
        var whs = await _db.Set<WarehouseEntity>().FirstOrDefaultAsync(w => w.IsDefault)
                  ?? await _db.Set<WarehouseEntity>().OrderBy(w => w.Id).FirstOrDefaultAsync();

        if (whs == null)
            throw new InvalidOperationException(
                "No warehouse is configured. Create a warehouse (Inventory → Warehouse) "
                + "before posting documents that move stock.");

        return whs.Code;
    }

    public async Task<string> ResolveWarehouseCodeAsync(string? requested)
    {
        if (string.IsNullOrWhiteSpace(requested))
            return await GetDefaultWarehouseCodeAsync();

        var code = requested.Trim();
        var exists = await _db.Set<WarehouseEntity>()
            .AnyAsync(w => w.Code.ToUpper() == code.ToUpper());

        if (!exists)
            throw new InvalidOperationException($"Warehouse \"{requested}\" does not exist.");

        return code;
    }

    public Task PostInAsync(string itemCode, string whsCode, decimal qty, decimal unitCost,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null, IReadOnlyList<BatchLine>? batches = null)
        => PostInCore(itemCode, whsCode, qty, unitCost, transType, baseDocType, baseDocEntry, baseDocNum, postingDate,
            cardCode, cardName, batches, BatchFlow.In);

    private async Task PostInCore(string itemCode, string whsCode, decimal qty, decimal unitCost,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode, string? cardName, IReadOnlyList<BatchLine>? batches, BatchFlow flow)
    {
        if (qty <= 0) return;

        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode)
                   ?? throw new InvalidOperationException($"Item {itemCode} not found.");

        var batchError = await _batches.ValidateAsync(null, item, whsCode, qty, batches, flow, postingDate);
        if (batchError != null) throw new InvalidOperationException(batchError);

        var balance = await GetOrCreateBalanceAsync(itemCode, whsCode);
        var method  = Valuation.Of(item.ValuationMethod);

        var currentQty = balance.Balance;
        var newQty     = currentQty + qty;
        decimal ledgerUnit, valueBalance, variance = 0m;

        if (method == Valuation.Fifo)
        {
            _db.Set<InventoryCostLayer>().Add(new InventoryCostLayer
            {
                ItemCode = itemCode, WhsCode = whsCode, ReceivedDate = DateTime.SpecifyKind(postingDate.Date, DateTimeKind.Utc),
                SourceDocType = baseDocType, SourceDocEntry = baseDocEntry, SourceDocNum = baseDocNum,
                InitialQty = qty, RemainingQty = qty, UnitCost = unitCost, CreatedAt = DateTime.UtcNow,
            });
            item.ItemCost = NextIssueCost(itemCode, item.ItemCost);
            ledgerUnit    = unitCost;
            valueBalance  = OpenLayers(itemCode, whsCode).Sum(l => l.RemainingQty * l.UnitCost);
        }
        else if (method == Valuation.Standard)
        {
            var std = item.ItemCost;
            if (std <= 0)
                throw new InvalidOperationException(
                    $"Item {itemCode} uses standard costing but has no standard cost. Set its Item Cost first, then receive it.");
            variance     = Math.Round((unitCost - std) * qty, 4);
            ledgerUnit   = std;                       // stock is carried at standard
            valueBalance = newQty * std;
        }
        else
        {
            // Moving-average cost: blend the incoming lot's cost into the item's average.
            var currentCost = item.ItemCost;
            var newAvgCost  = newQty == 0 ? unitCost : ((currentQty * currentCost) + (qty * unitCost)) / newQty;
            item.ItemCost   = newAvgCost;
            ledgerUnit      = unitCost;
            valueBalance    = newQty * newAvgCost;
        }
        item.UpdatedAt = DateTime.UtcNow;

        balance.InQty    += qty;
        balance.Balance   = newQty;
        balance.UpdatedAt = DateTime.UtcNow;

        _db.Set<InventoryJournal>().Add(new InventoryJournal
        {
            ItemCode     = itemCode,
            WhsCode      = whsCode,
            PostingDate  = postingDate,
            TransType    = transType,
            BaseDocType  = baseDocType,
            BaseDocEntry = baseDocEntry,
            BaseDocNum   = baseDocNum,
            CardCode     = cardCode,
            CardName     = cardName,
            InQty        = qty,
            OutQty       = 0,
            UnitCost     = ledgerUnit,
            TransValue   = qty * ledgerUnit,
            QtyBalance   = newQty,
            ValueBalance = valueBalance,
            PriceVariance = variance,
            CreatedAt    = DateTime.UtcNow,
            InActive     = false,
        });

        await _batches.ApplyAsync(item, whsCode, batches, flow, postingDate, transType, baseDocType, baseDocEntry, baseDocNum, cardCode);
    }

    public async Task PostReturnInAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null, IReadOnlyList<BatchLine>? batches = null)
    {
        if (qty <= 0) return;

        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode)
                   ?? throw new InvalidOperationException($"Item {itemCode} not found.");

        // Returning at the current average leaves the average unchanged.
        await PostInCore(itemCode, whsCode, qty, item.ItemCost,
            transType, baseDocType, baseDocEntry, baseDocNum, postingDate, cardCode, cardName, batches, BatchFlow.ReturnIn);
    }

    public async Task<decimal> PostOutAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null, IReadOnlyList<BatchLine>? batches = null, bool allowExpired = false)
    {
        if (qty <= 0) return 0m;

        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode)
                   ?? throw new InvalidOperationException($"Item {itemCode} not found.");

        var outFlow = allowExpired ? BatchFlow.WriteOff : BatchFlow.Out;
        var batchError = await _batches.ValidateAsync(null, item, whsCode, qty, batches, outFlow, postingDate);
        if (batchError != null) throw new InvalidOperationException(batchError);

        var balance = await GetOrCreateBalanceAsync(itemCode, whsCode);
        var newQty  = balance.Balance - qty;

        decimal total, valueBalance;
        if (Valuation.Of(item.ValuationMethod) == Valuation.Fifo)
        {
            total = Consume(itemCode, whsCode, qty, item.ItemCost, apply: true);
            item.ItemCost = NextIssueCost(itemCode, item.ItemCost);
            valueBalance  = OpenLayers(itemCode, whsCode).Sum(l => l.RemainingQty * l.UnitCost);
        }
        else
        {
            // Moving average and standard: issues leave at the item's current cost,
            // which the movement itself doesn't change.
            total        = qty * item.ItemCost;
            valueBalance = newQty * item.ItemCost;
        }
        item.UpdatedAt = DateTime.UtcNow;

        balance.OutQty   += qty;
        balance.Balance   = newQty;
        balance.UpdatedAt = DateTime.UtcNow;

        _db.Set<InventoryJournal>().Add(new InventoryJournal
        {
            ItemCode     = itemCode,
            WhsCode      = whsCode,
            PostingDate  = postingDate,
            TransType    = transType,
            BaseDocType  = baseDocType,
            BaseDocEntry = baseDocEntry,
            BaseDocNum   = baseDocNum,
            CardCode     = cardCode,
            CardName     = cardName,
            InQty        = 0,
            OutQty       = qty,
            UnitCost     = total / qty,
            TransValue   = -total,
            QtyBalance   = newQty,
            ValueBalance = valueBalance,
            CreatedAt    = DateTime.UtcNow,
            InActive     = false,
        });
        await _batches.ApplyAsync(item, whsCode, batches, outFlow, postingDate, transType, baseDocType, baseDocEntry, baseDocNum, cardCode);
        return total;
    }

    public decimal PreviewIssueCost(string itemCode, string whsCode, decimal qty)
    {
        if (qty <= 0) return 0m;
        var item = _db.Set<ItemsMaster>().FirstOrDefault(i => i.ItemCode == itemCode);
        if (item == null) return 0m;
        return Valuation.Of(item.ValuationMethod) == Valuation.Fifo
            ? Consume(itemCode, whsCode, qty, item.ItemCost, apply: false)
            : qty * item.ItemCost;
    }

    public void RevalueLayers(string itemCode, decimal newCost)
    {
        foreach (var layer in OpenLayers(itemCode)) layer.UnitCost = newCost;
    }

    public List<InventoryCostLayer> OpenLayers(string itemCode, string? whsCode = null)
    {
        var set = _db.Set<InventoryCostLayer>();
        var fromDb = set.Where(l => l.ItemCode == itemCode && (whsCode == null || l.WhsCode == whsCode)).ToList();
        var added  = set.Local.Where(l => l.Id == 0 && l.ItemCode == itemCode && (whsCode == null || l.WhsCode == whsCode));
        return fromDb.Union(added)
            .Where(l => l.RemainingQty > 0)
            .OrderBy(l => l.ReceivedDate.ToUniversalTime()).ThenBy(l => l.Id == 0 ? int.MaxValue : l.Id)   // unsaved layers come after saved ones
            .ToList();
    }

    // Walks the warehouse's open layers oldest-first. When the layers run out (stock
    // that predates layering, or a negative-stock issue) the shortfall is valued at the
    // last layer's cost, or the item's cost if there was none.
    private decimal Consume(string itemCode, string whsCode, decimal qty, decimal fallbackCost, bool apply)
    {
        var left = qty; var total = 0m; var lastCost = fallbackCost;
        foreach (var layer in OpenLayers(itemCode, whsCode))
        {
            if (left <= 0) break;
            var take = Math.Min(layer.RemainingQty, left);
            total   += take * layer.UnitCost;
            lastCost = layer.UnitCost;
            left    -= take;
            if (apply) layer.RemainingQty -= take;
        }
        if (left > 0) total += left * lastCost;
        return total;
    }

    // The cost the next unit issued would carry: the oldest open layer across warehouses.
    private decimal NextIssueCost(string itemCode, decimal current)
    {
        var oldest = OpenLayers(itemCode).FirstOrDefault();
        return oldest?.UnitCost ?? current;
    }

    private async Task<InventoryBalance> GetOrCreateBalanceAsync(string itemCode, string whsCode)
    {
        var balance = await _db.Set<InventoryBalance>()
            .FirstOrDefaultAsync(x => x.ItemsCode == itemCode && x.WhsCode == whsCode);

        if (balance != null) return balance;

        balance = new InventoryBalance
        {
            ItemsCode = itemCode,
            WhsCode   = whsCode,
            CreatedAt = DateTime.UtcNow,
            InActive  = false,
        };
        _db.Set<InventoryBalance>().Add(balance);
        return balance;
    }
}

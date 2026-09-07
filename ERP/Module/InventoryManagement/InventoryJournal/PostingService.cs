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
//   3. on inbound movements, recalculates the item's moving-average cost.
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
        string? cardCode = null, string? cardName = null);

    /// <summary>
    /// Brings returned goods back into stock at the item's current moving-average
    /// cost, so a return does not move the average. Never post a return at the
    /// selling price — that would inflate the item's cost.
    /// </summary>
    Task PostReturnInAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null);

    Task PostOutAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null);
}

public class InventoryPostingService : IInventoryPostingService
{
    private readonly MyDbContext _db;
    public InventoryPostingService(MyDbContext db) => _db = db;

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

    public async Task PostInAsync(string itemCode, string whsCode, decimal qty, decimal unitCost,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null)
    {
        if (qty <= 0) return;

        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode)
                   ?? throw new InvalidOperationException($"Item {itemCode} not found.");

        var balance = await GetOrCreateBalanceAsync(itemCode, whsCode);

        var currentQty  = balance.Balance;
        var currentCost = item.ItemCost;
        var newQty      = currentQty + qty;

        // Moving-average cost: blend the incoming lot's cost into the item's average.
        var newAvgCost = newQty == 0 ? unitCost : ((currentQty * currentCost) + (qty * unitCost)) / newQty;

        item.ItemCost  = newAvgCost;
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
            UnitCost     = unitCost,
            TransValue   = qty * unitCost,
            QtyBalance   = newQty,
            ValueBalance = newQty * newAvgCost,
            CreatedAt    = DateTime.UtcNow,
            InActive     = false,
        });
    }

    public async Task PostReturnInAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null)
    {
        if (qty <= 0) return;

        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode)
                   ?? throw new InvalidOperationException($"Item {itemCode} not found.");

        // Returning at the current average leaves the average unchanged.
        await PostInAsync(itemCode, whsCode, qty, item.ItemCost,
            transType, baseDocType, baseDocEntry, baseDocNum, postingDate, cardCode, cardName);
    }

    public async Task PostOutAsync(string itemCode, string whsCode, decimal qty,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, DateTime postingDate,
        string? cardCode = null, string? cardName = null)
    {
        if (qty <= 0) return;

        var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(i => i.ItemCode == itemCode)
                   ?? throw new InvalidOperationException($"Item {itemCode} not found.");

        var balance = await GetOrCreateBalanceAsync(itemCode, whsCode);

        // Cost basis for an outbound movement is the item's current moving-average
        // cost — it isn't changed by the movement itself.
        var currentCost = item.ItemCost;
        var newQty      = balance.Balance - qty;

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
            UnitCost     = currentCost,
            TransValue   = -(qty * currentCost),
            QtyBalance   = newQty,
            ValueBalance = newQty * currentCost,
            CreatedAt    = DateTime.UtcNow,
            InActive     = false,
        });
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

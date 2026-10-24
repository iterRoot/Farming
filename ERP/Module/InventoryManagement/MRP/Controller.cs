using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ItemEntity      = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using InventoryBal    = FarmingApi.Modules.Sale.Inventory.Inventory;
using SaleOrderEntity = FarmingApi.Modules.SaleAR.SaleOrder.SaleOrder;
using Bom             = FarmingApi.Modules.InventoryManagement.BillOfMaterial.BillOfMaterial;

namespace FarmingApi.Modules.InventoryManagement.MRP;

// ═══════════════════════════════════════════════════════════════════
// MRP  (route: /MRP)
// GET /MRP — a planning report, not a document: for every item with
// open Sale Order demand, explodes any Bill of Material it's the
// parent of (recursively, so a component that is itself manufactured
// pulls demand for its own components too), nets that against on-hand
// stock above the item's MinStock safety level, and suggests either a
// Production Order (item has an active BOM) or a Purchase Request
// (it doesn't) for the shortfall. No lead times, capacity, or
// scheduling — a shortage list to act on, not a full scheduler.
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class MRPController : ControllerBase
{
    private readonly MyDbContext _db;
    public MRPController(MyDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Run()
    {
        var items = await _db.Set<ItemEntity>().ToListAsync();
        var itemByCode = items.ToDictionary(i => i.ItemCode, i => i);

        var boms = await _db.Set<Bom>().Include(b => b.Lines).Where(b => b.IsActive).ToListAsync();
        var bomByParent = boms
            .GroupBy(b => b.ParentItemCode)
            .ToDictionary(g => g.Key, g => g.First()); // first active BOM if more than one — simplification

        var onHand = await _db.Set<InventoryBal>()
            .GroupBy(b => b.ItemsCode)
            .Select(g => new { ItemCode = g.Key, Balance = g.Sum(x => x.Balance) })
            .ToDictionaryAsync(x => x.ItemCode, x => x.Balance);

        var openOrders = await _db.Set<SaleOrderEntity>()
            .Include(o => o.Items)
            .Where(o => o.Status == "O")
            .ToListAsync();

        // ── Direct sales demand per item ─────────────────────────────
        var salesDemand = new Dictionary<string, decimal>();
        var demandFrom  = new Dictionary<string, List<string>>();
        foreach (var order in openOrders)
        {
            foreach (var line in order.Items)
            {
                if (string.IsNullOrWhiteSpace(line.ItemCode) || line.Quantity <= 0) continue;
                salesDemand[line.ItemCode] = salesDemand.GetValueOrDefault(line.ItemCode) + line.Quantity;
                if (!demandFrom.TryGetValue(line.ItemCode, out var list)) demandFrom[line.ItemCode] = list = new();
                if (!list.Contains(order.DocNum)) list.Add(order.DocNum);
            }
        }

        // ── Explode BOMs level by level, not one dequeue at a time ───
        // A naive single-pass BFS would explode a shared sub-component's own
        // BOM using only the FIRST parent's contribution to it, missing a
        // second sibling parent's demand for the same item that arrives
        // later. Instead, every item's total incoming demand for this level
        // is fully summed into nextLevel before any of them are exploded —
        // so two different manufactured parents needing the same component
        // both count. (Not handled: an item that is both sold directly AND
        // used as a sub-assembly component, where the direct-sale portion
        // would need folding in before that item's own children are
        // exploded too — a genuine full topological sort, out of scope for
        // a planning report rather than a scheduler.)
        // Explosion must propagate on NET shortfall, not gross demand — an
        // item that already has enough stock (above its own MinStock) to
        // cover what's being asked of it needs no more built or bought, so
        // it pulls no demand onto its own components either.
        decimal NetOf(string code, decimal gross)
        {
            var stock = onHand.GetValueOrDefault(code);
            var min   = itemByCode.TryGetValue(code, out var it) ? it.MinStock : 0m;
            var atp   = Math.Max(0, stock - min);
            return Math.Max(0, gross - atp);
        }

        var componentDemand = new Dictionary<string, decimal>();
        var levelDemand = salesDemand.ToDictionary(kv => kv.Key, kv => NetOf(kv.Key, kv.Value));
        var exploded = new HashSet<string>();
        var depthGuard = 0;

        while (levelDemand.Count > 0 && depthGuard++ < 20)
        {
            var nextLevelGross = new Dictionary<string, decimal>();
            foreach (var (itemCode, netQty) in levelDemand)
            {
                if (netQty <= 0) continue;
                if (!bomByParent.TryGetValue(itemCode, out var bom)) continue;
                if (!exploded.Add(itemCode)) continue; // cycle guard — a BOM shouldn't loop, but never trust that blindly

                var factor = bom.Quantity > 0 ? netQty / bom.Quantity : 0;
                foreach (var line in bom.Lines)
                {
                    var needed = Math.Round(line.Quantity * factor, 4);
                    componentDemand[line.ItemCode] = componentDemand.GetValueOrDefault(line.ItemCode) + needed;
                    nextLevelGross[line.ItemCode] = nextLevelGross.GetValueOrDefault(line.ItemCode) + needed;

                    if (!demandFrom.TryGetValue(line.ItemCode, out var list)) demandFrom[line.ItemCode] = list = new();
                    var tag = $"BOM {bom.Code} for {itemCode}";
                    if (!list.Contains(tag)) list.Add(tag);
                }
            }
            // Net the next level's freshly-summed gross contribution before exploding it further.
            levelDemand = nextLevelGross.ToDictionary(kv => kv.Key, kv => NetOf(kv.Key, kv.Value));
        }

        // ── Net each touched item ─────────────────────────────────────
        var touched = salesDemand.Keys.Union(componentDemand.Keys).ToList();
        var result = new List<MrpLine>();

        foreach (var code in touched)
        {
            if (!itemByCode.TryGetValue(code, out var item)) continue; // unknown item code, skip

            var sales     = salesDemand.GetValueOrDefault(code);
            var component = componentDemand.GetValueOrDefault(code);
            var gross     = sales + component;
            if (gross <= 0) continue;

            var stock  = onHand.GetValueOrDefault(code);
            var atp    = Math.Max(0, stock - item.MinStock);
            var net    = Math.Max(0, gross - atp);
            var manufactured = bomByParent.TryGetValue(code, out var ownBom);

            result.Add(new MrpLine
            {
                ItemId             = item.Id,
                ItemCode           = item.ItemCode,
                ItemName           = item.ItemName,
                IsManufactured     = manufactured,
                BillOfMaterialId   = manufactured ? ownBom!.Id : null,
                Warehouse          = manufactured ? ownBom!.Warehouse : null,
                SalesDemand        = sales,
                ComponentDemand    = component,
                GrossRequirement   = gross,
                OnHand             = stock,
                MinStock           = item.MinStock,
                AvailableToPromise = atp,
                NetRequirement     = net,
                SuggestedAction    = net <= 0 ? "None" : manufactured ? "Production Order" : "Purchase Request",
                DemandFrom         = demandFrom.GetValueOrDefault(code, new()),
            });
        }

        return Ok(new MrpResponse
        {
            GeneratedAt   = DateTime.UtcNow,
            ShortageCount = result.Count(r => r.NetRequirement > 0),
            Lines         = result.OrderByDescending(r => r.NetRequirement).ThenBy(r => r.ItemCode).ToList(),
        });
    }
}

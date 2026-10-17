using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ItemEntity      = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using InventoryBal    = FarmingApi.Modules.Sale.Inventory.Inventory;
using SaleOrderEntity = FarmingApi.Modules.SaleAR.SaleOrder.SaleOrder;
using Bom             = FarmingApi.Modules.InventoryManagement.BillOfMaterial.BillOfMaterial;

namespace FarmingApi.Modules.InventoryManagement.OrderRecommendation;

// ═══════════════════════════════════════════════════════════════════
// ORDER RECOMMENDATION  (route: /OrderRecommendation)
// GET /OrderRecommendation — the actionable side of MRP: for every item
// whose on-hand can't cover open sales-order demand plus its minimum
// stock, recommend a quantity to make (item has an active BOM) or buy
// (it doesn't). A planning report, not a document — it suggests the
// orders a planner then raises.
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class OrderRecommendationController : ControllerBase
{
    private readonly MyDbContext _db;
    public OrderRecommendationController(MyDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Run()
    {
        var items = await _db.Set<ItemEntity>().ToListAsync();

        var activeBoms = await _db.Set<Bom>().Where(b => b.IsActive).ToListAsync();
        var bomByParent = activeBoms
            .GroupBy(b => b.ParentItemCode)
            .ToDictionary(g => g.Key, g => g.First());

        var onHand = await _db.Set<InventoryBal>()
            .GroupBy(b => b.ItemsCode)
            .Select(g => new { ItemCode = g.Key, Balance = g.Sum(x => x.Balance) })
            .ToDictionaryAsync(x => x.ItemCode, x => x.Balance);

        var openOrders = await _db.Set<SaleOrderEntity>()
            .Include(o => o.Items).Where(o => o.Status == "O").ToListAsync();

        var salesDemand = new Dictionary<string, decimal>();
        foreach (var order in openOrders)
            foreach (var line in order.Items)
            {
                if (string.IsNullOrWhiteSpace(line.ItemCode) || line.Quantity <= 0) continue;
                salesDemand[line.ItemCode] = salesDemand.GetValueOrDefault(line.ItemCode) + line.Quantity;
            }

        var lines = new List<OrderRecommendationLine>();
        foreach (var item in items)
        {
            var demand = salesDemand.GetValueOrDefault(item.ItemCode);
            var stock  = onHand.GetValueOrDefault(item.ItemCode);
            // Need enough to cover open demand and still hold the minimum stock level.
            var required = demand + item.MinStock;
            var recommend = Math.Max(0, required - stock);
            if (recommend <= 0) continue;

            var manufactured = bomByParent.TryGetValue(item.ItemCode, out var bom);
            lines.Add(new OrderRecommendationLine
            {
                ItemId           = item.Id,
                ItemCode         = item.ItemCode,
                ItemName         = item.ItemName,
                RecommendedType  = manufactured ? "Production" : "Purchase",
                SalesDemand      = demand,
                OnHand           = stock,
                MinStock         = item.MinStock,
                RecommendedQty   = Math.Round(recommend, 4),
                BillOfMaterialId = manufactured ? bom!.Id : null,
                Warehouse        = manufactured ? bom!.Warehouse : null,
            });
        }

        lines = lines.OrderByDescending(l => l.RecommendedQty).ThenBy(l => l.ItemCode).ToList();
        return Ok(new OrderRecommendationResponse
        {
            GeneratedAt = DateTime.UtcNow,
            Count       = lines.Count,
            MakeCount   = lines.Count(l => l.RecommendedType == "Production"),
            BuyCount    = lines.Count(l => l.RecommendedType == "Purchase"),
            Lines       = lines,
        });
    }
}

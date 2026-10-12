using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;

namespace FarmingApi.Modules.InventoryManagement.PriceLists;

public class ResolvedPrice
{
    public decimal  Price  { get; set; }
    public string   Source { get; set; } = null!;   // "Explicit" | "Derived" | "Item sale price"
    public string?  Via    { get; set; }            // base list name, when derived
}

// ═══════════════════════════════════════════════════════════════
// How an item's price on a list is found:
//   1. a price entered for that item on that list, else
//   2. the base list's price × this list's factor, rounded by this
//      list's rounding rule (recursively, so lists can chain), else
//   3. for a list that is its own base with nothing entered: the
//      item's sale price × factor, rounded.
// ═══════════════════════════════════════════════════════════════
public static class PriceResolver
{
    private const int MaxDepth = 10;

    public static ResolvedPrice? Resolve(MyDbContext db, int listId, string itemCode)
    {
        var lists = db.Set<PriceList>().ToDictionary(l => l.Id);
        var explicitPrices = db.Set<ItemPrice>().Where(p => p.ItemCode == itemCode)
            .ToDictionary(p => p.PriceListId, p => p.UnitPrice);
        var item = db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == itemCode);
        return Resolve(lists, explicitPrices, item?.SalePrice, listId, 0);
    }

    // Prices for every item on one list — one query per table, not per item.
    public static Dictionary<string, ResolvedPrice> ResolveAll(MyDbContext db, int listId, IEnumerable<ItemEntity> items)
    {
        var lists = db.Set<PriceList>().ToDictionary(l => l.Id);
        var byList = db.Set<ItemPrice>().ToList().GroupBy(p => p.ItemCode)
            .ToDictionary(g => g.Key, g => g.ToDictionary(p => p.PriceListId, p => p.UnitPrice));
        var result = new Dictionary<string, ResolvedPrice>();
        foreach (var item in items)
        {
            var explicitPrices = byList.GetValueOrDefault(item.ItemCode) ?? new Dictionary<int, decimal>();
            var r = Resolve(lists, explicitPrices, item.SalePrice, listId, 0);
            if (r != null) result[item.ItemCode] = r;
        }
        return result;
    }

    private static ResolvedPrice? Resolve(Dictionary<int, PriceList> lists, Dictionary<int, decimal> explicitPrices,
        decimal? itemSalePrice, int listId, int depth)
    {
        if (depth > MaxDepth || !lists.TryGetValue(listId, out var list)) return null;

        if (explicitPrices.TryGetValue(listId, out var own))
            return new ResolvedPrice { Price = own, Source = "Explicit" };

        if (list.BasePriceListId is int baseId && baseId != listId)
        {
            var basePrice = Resolve(lists, explicitPrices, itemSalePrice, baseId, depth + 1);
            if (basePrice == null) return null;
            var baseName = lists.GetValueOrDefault(baseId)?.Name;
            return new ResolvedPrice { Price = Round(basePrice.Price * list.Factor, list), Source = "Derived", Via = baseName };
        }

        if (itemSalePrice is decimal sale)
            return new ResolvedPrice { Price = Round(sale * list.Factor, list), Source = "Item sale price" };
        return null;
    }

    // The list's rounding method, applied in the list's rounding rule's direction.
    public static decimal Round(decimal value, PriceList list) => list.RoundingMethod switch
    {
        "Round to Full Amount"         => ToMultiple(value, 1m, list.RoundingRule),
        "Round to Full Decimal Amount" => ToMultiple(value, 0.1m, list.RoundingRule),
        "Round to Full Tens"           => ToMultiple(value, 10m, list.RoundingRule),
        "Round Off Interval"           => list.EndingInterval is > 0 ? ToMultiple(value, list.EndingInterval.Value, list.RoundingRule) : value,
        "Round Off Ending"             => list.EndingInterval is >= 0 ? ToEnding(value, list.EndingInterval.Value, list.RoundingRule) : value,
        _                              => Math.Round(value, 4),
    };

    private static decimal ToMultiple(decimal value, decimal step, string rule)
    {
        var q = value / step;
        var n = rule switch
        {
            "Round Up"   => Math.Ceiling(q),
            "Round Down" => Math.Floor(q),
            _            => Math.Round(q, MidpointRounding.AwayFromZero),
        };
        return Math.Round(n * step, 4);
    }

    // Prices that end in a fixed value, e.g. ending 0.99 → …, 4.99, 5.99, 6.99 …
    // An ending of 1 or more works on tens instead (ending 9.99 → 9.99, 19.99, 29.99).
    private static decimal ToEnding(decimal value, decimal ending, string rule)
    {
        var modulus = ending >= 1m ? 10m : 1m;
        var k = Math.Floor((value - ending) / modulus);
        var below = k * modulus + ending;           // the ending at or below the value
        var above = below + modulus;                // the next one up
        if (below == value) return Math.Round(value, 4);
        var pick = rule switch
        {
            "Round Up"   => above,
            "Round Down" => below,
            _            => (value - below) < (above - value) ? below : above,
        };
        return Math.Round(pick < 0 ? 0 : pick, 4);
    }
}

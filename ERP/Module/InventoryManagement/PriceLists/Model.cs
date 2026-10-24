namespace FarmingApi.Modules.InventoryManagement.PriceLists;

public static class PriceListOptions
{
    public static readonly string[] RoundingMethods =
    {
        "No Rounding", "Round to Full Amount", "Round to Full Decimal Amount",
        "Round to Full Tens", "Round Off Ending", "Round Off Interval",
    };

    // Ending/Interval only means something for these two methods.
    public static readonly string[] MethodsUsingEndingInterval = { "Round Off Ending", "Round Off Interval" };

    public static readonly string[] RoundingRules = { "Round to Closest", "Round Up", "Round Down" };

    public static readonly string[] AuthorizationGroups =
    {
        "Price List - Group No. 1", "Price List - Group No. 2",
        "Price List - Group No. 3", "Price List - Group No. 4",
    };
}

public class PriceListResponse
{
    public int      Id                 { get; set; }
    public string   Name               { get; set; } = null!;
    public bool     MultipleBase       { get; set; }
    public int?     BasePriceListId    { get; set; }
    public string?  BasePriceListName  { get; set; }
    public decimal  Factor             { get; set; }
    public string   RoundingMethod     { get; set; } = null!;
    public string   RoundingRule       { get; set; } = null!;
    public decimal? EndingInterval     { get; set; }
    public string?  AuthorizationGroup { get; set; }
    public bool     GrossPrice         { get; set; }
    public string   PrimaryCurrency    { get; set; } = null!;
    public bool     IsActive           { get; set; }
    public DateTime  CreatedAt         { get; set; }
    public DateTime? UpdatedAt         { get; set; }
}

public class PriceListRequest
{
    public string   Name               { get; set; } = null!;
    public bool     MultipleBase       { get; set; }
    public int?     BasePriceListId    { get; set; }
    public decimal  Factor             { get; set; } = 1m;
    public string   RoundingMethod     { get; set; } = "No Rounding";
    public string   RoundingRule       { get; set; } = "Round to Closest";
    public decimal? EndingInterval     { get; set; }
    public string?  AuthorizationGroup { get; set; }
    public bool     GrossPrice         { get; set; }
    public string   PrimaryCurrency    { get; set; } = "USD";
    public bool     IsActive           { get; set; } = true;
}

// One grid row on the Price Lists screen: Id null = a new list.
public class PriceListBatchRow : PriceListRequest
{
    public int? Id { get; set; }
}

public class PriceListOptionsResponse
{
    public string[] RoundingMethods            { get; set; } = PriceListOptions.RoundingMethods;
    public string[] MethodsUsingEndingInterval { get; set; } = PriceListOptions.MethodsUsingEndingInterval;
    public string[] RoundingRules              { get; set; } = PriceListOptions.RoundingRules;
    public string[] AuthorizationGroups        { get; set; } = PriceListOptions.AuthorizationGroups;
}

// One item's line on a price list's price page.
public class ItemPriceRow
{
    public string   ItemCode       { get; set; } = null!;
    public string   ItemName       { get; set; } = null!;
    public string?  Uom            { get; set; }
    public decimal  SalePrice      { get; set; }   // the item master's own price
    public decimal? ExplicitPrice  { get; set; }   // typed in on this list, if any
    public decimal? EffectivePrice { get; set; }   // what a document would use
    public string?  Source         { get; set; }   // Explicit | Derived | Item sale price
    public string?  Via            { get; set; }   // base list, when derived
}

public class ItemPriceUpdate
{
    public string   ItemCode  { get; set; } = null!;
    public decimal? UnitPrice { get; set; }        // null clears the typed-in price
}

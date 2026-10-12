using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Administration.SetUp.Generals.SavedSearch;

// ═══════════════════════════════════════════════════════════════════
// SEARCHABLE-ENTITY REGISTRY
//
// This is the whole safety model for Formatted Search. A saved search
// never carries executable code, SQL, or a dynamic LINQ expression
// string — it stores a target entity KEY and a field NAME, both of
// which must match an entry in this hardcoded dictionary or the
// request is rejected outright. Matching entries resolve to a plain,
// compiled C# delegate that reads one specific property off one
// specific entity type. There is no path from a saved search's stored
// text to anything the database interprets as code — the worst a
// malicious Value could do is fail to parse as the field's declared
// type and match nothing.
// ═══════════════════════════════════════════════════════════════════
public enum FieldType { Text, Number, Boolean }

public class SearchFieldDef
{
    public string   Name     { get; init; } = null!;
    public string   Label    { get; init; } = null!;
    public FieldType Type    { get; init; }
    public Func<object, object?> Accessor { get; init; } = null!;
}

public class SearchTargetDef
{
    public string   Key     { get; init; } = null!;
    public string   Label   { get; init; } = null!;
    public Func<MyDbContext, List<object>> Load     { get; init; } = null!;
    public Func<object, (int Id, string Display)> Summarize { get; init; } = null!;
    public List<SearchFieldDef> Fields { get; init; } = new();
}

public static class SearchableEntityRegistry
{
    public static readonly Dictionary<string, SearchTargetDef> Targets = new()
    {
        ["ItemsMaster"] = new SearchTargetDef
        {
            Key   = "ItemsMaster",
            Label = "Items",
            Load  = db => db.Set<ItemEntity>().ToList().Cast<object>().ToList(),
            Summarize = o => { var i = (ItemEntity)o; return (i.Id, $"{i.ItemCode} — {i.ItemName}"); },
            Fields = new()
            {
                Field("ItemCode",   "Item Code",   FieldType.Text,    o => ((ItemEntity)o).ItemCode),
                Field("ItemName",   "Item Name",   FieldType.Text,    o => ((ItemEntity)o).ItemName),
                Field("ItemGroup",  "Item Group",  FieldType.Text,    o => ((ItemEntity)o).ItemGroup),
                Field("ItemType",   "Item Type",   FieldType.Text,    o => ((ItemEntity)o).ItemType),
                Field("Barcode",    "Barcode",     FieldType.Text,    o => ((ItemEntity)o).Barcode),
                Field("ManageBy",   "Manage By",   FieldType.Text,    o => ((ItemEntity)o).ManageBy),
                Field("MinStock",   "Min Stock",   FieldType.Number,  o => ((ItemEntity)o).MinStock),
                Field("SalePrice",  "Sale Price",  FieldType.Number,  o => ((ItemEntity)o).SalePrice),
                Field("SalesItem",  "Is Sales Item",    FieldType.Boolean, o => ((ItemEntity)o).SalesItem),
                Field("PurchaseItem","Is Purchase Item",FieldType.Boolean, o => ((ItemEntity)o).PurchaseItem),
            },
        },
        ["BusinessPartnersMaster"] = new SearchTargetDef
        {
            Key   = "BusinessPartnersMaster",
            Label = "Business Partners",
            Load  = db => db.Set<BPEntity>().ToList().Cast<object>().ToList(),
            Summarize = o => { var b = (BPEntity)o; return (b.Id, $"{b.Code} — {b.CardName}"); },
            Fields = new()
            {
                Field("Code",       "Code",        FieldType.Text, o => ((BPEntity)o).Code),
                Field("CardName",   "Name",        FieldType.Text, o => ((BPEntity)o).CardName),
                Field("GroupCode",  "Group",       FieldType.Text, o => ((BPEntity)o).GroupCode),
                Field("Currency",   "Currency",    FieldType.Text, o => ((BPEntity)o).Currency),
                Field("Email",      "Email",       FieldType.Text, o => ((BPEntity)o).Email),
                Field("Tel1",       "Phone",       FieldType.Text, o => ((BPEntity)o).Tel1),
                Field("Industry",   "Industry",    FieldType.Text, o => ((BPEntity)o).Industry),
            },
        },
    };

    public static readonly Dictionary<FieldType, string[]> OperatorsByType = new()
    {
        [FieldType.Text]    = new[] { "Equals", "NotEquals", "Contains", "StartsWith" },
        [FieldType.Number]  = new[] { "Equals", "NotEquals", "GreaterThan", "LessThan", "GreaterOrEqual", "LessOrEqual" },
        [FieldType.Boolean] = new[] { "Equals" },
    };

    private static SearchFieldDef Field(string name, string label, FieldType type, Func<object, object?> accessor)
        => new() { Name = name, Label = label, Type = type, Accessor = accessor };
}

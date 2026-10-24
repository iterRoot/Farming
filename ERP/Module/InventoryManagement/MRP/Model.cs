namespace FarmingApi.Modules.InventoryManagement.MRP;

public class MrpLine
{
    public int      ItemId             { get; set; }
    public string   ItemCode           { get; set; } = null!;
    public string   ItemName           { get; set; } = null!;
    public bool     IsManufactured     { get; set; }   // has an active BOM as its parent item
    public int?     BillOfMaterialId   { get; set; }   // set only when IsManufactured
    public decimal  SalesDemand        { get; set; }   // open Sale Order quantity, this item directly
    public decimal  ComponentDemand    { get; set; }   // demand pulled in by exploding a parent's BOM
    public decimal  GrossRequirement   { get; set; }   // SalesDemand + ComponentDemand
    public decimal  OnHand             { get; set; }
    public decimal  MinStock           { get; set; }
    public decimal  AvailableToPromise { get; set; }   // max(0, OnHand - MinStock)
    public decimal  NetRequirement     { get; set; }   // max(0, GrossRequirement - AvailableToPromise)
    public string   SuggestedAction    { get; set; } = "None";  // "Production Order" | "Purchase Request" | "None"
    public string?  Warehouse          { get; set; }   // BOM's warehouse, when manufactured
    public List<string> DemandFrom     { get; set; } = new();   // Sale Order doc nums / parent item codes, for "why"
}

public class MrpResponse
{
    public DateTime GeneratedAt { get; set; }
    public int      ShortageCount { get; set; }
    public List<MrpLine> Lines { get; set; } = new();
}

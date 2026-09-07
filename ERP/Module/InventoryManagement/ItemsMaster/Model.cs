namespace FarmingApi.Modules.Inventory.ItemsMaster;

// ══════════════════════════════════════════════════════════════════
// Shared field set — mirrors the SAP B1 "Item Master Data" tabs.
// ══════════════════════════════════════════════════════════════════
public abstract class ItemsMasterFields
{
    // 🔹 General
    public string  ItemName     { get; set; } = string.Empty;
    public string  ItemGroup    { get; set; } = string.Empty;
    public string  ItemType     { get; set; } = "Inventory";
    public string  Barcode      { get; set; } = string.Empty;
    public string  Manufacturer { get; set; } = string.Empty;
    public string  UomName      { get; set; } = string.Empty;
    public string  UomCode      { get; set; } = string.Empty;
    public string  ManageBy     { get; set; } = "None";

    public bool    SalesItem    { get; set; } = true;
    public bool    PurchaseItem { get; set; } = true;
    public bool    StockItem    { get; set; } = true;

    public string? ForeignName        { get; set; }
    public string? UomGroup           { get; set; }
    public decimal UnitPrice          { get; set; }
    public string? PriceCurrency      { get; set; }
    public string? PricingUnit        { get; set; }
    public bool    DoNotApplyDiscountGroups { get; set; }
    public bool    DoNotApplyDiscount { get; set; }
    public string? AdditionalIdentifier { get; set; }
    public string? ShippingType       { get; set; }
    public DateTime? ActiveFrom       { get; set; }
    public DateTime? ActiveTo         { get; set; }
    public string? Remarks            { get; set; }
    public string? CountryOfOrigin    { get; set; }
    public string? StandardItemIdentification { get; set; }
    public string? CommodityClassification    { get; set; }

    // 🔹 Inventory / Stock Data
    public string  Warehouse        { get; set; } = string.Empty;
    public decimal MinStock         { get; set; }
    public decimal MaxStock         { get; set; }
    public string  ValuationMethod  { get; set; } = "MovingAverage";
    public string? SetGlAccountsBy  { get; set; }
    public decimal StockWeight      { get; set; }
    public string? StockCountingUomCode { get; set; }
    public string? StockCountingUomName { get; set; }
    public decimal ItemsPerCountingUnit { get; set; } = 1;
    public decimal ItemCost              { get; set; }
    public bool    ManageStockByWarehouse { get; set; }
    public decimal RequiredStock         { get; set; }

    // 🔹 Sales
    public decimal SalePrice       { get; set; }
    public string  PriceList       { get; set; } = string.Empty;
    public string  SaleTaxGroup    { get; set; } = string.Empty;
    public decimal SaleDiscount    { get; set; }
    public string  SaleUom         { get; set; } = string.Empty;
    public string  SaleDescription { get; set; } = string.Empty;
    public string? SalesVatCode       { get; set; }
    public string? SalesUomCode       { get; set; }
    public string? SalesUomName       { get; set; }
    public decimal ItemsPerSalesUnit  { get; set; } = 1;
    public string? SalesPackageType   { get; set; }
    public decimal SalesQtyPerPackage { get; set; } = 1;
    public decimal SalesLength        { get; set; }
    public decimal SalesWidth         { get; set; }
    public decimal SalesHeight        { get; set; }
    public decimal SalesVolume        { get; set; }
    public decimal SalesWeight        { get; set; }
    public string? CreateQrCodeFrom   { get; set; }

    // 🔹 Purchasing
    public decimal PurchasePrice    { get; set; }
    public string  PreferredVendor  { get; set; } = string.Empty;
    public string  VendorItemNo     { get; set; } = string.Empty;
    public string  PurchaseUom      { get; set; } = string.Empty;
    public int     LeadTime         { get; set; }
    public string  PurchaseTaxGroup { get; set; } = string.Empty;
    public string? MfrCatalogueNo        { get; set; }
    public string? PurchasingUomCode     { get; set; }
    public string? PurchasingUomName     { get; set; }
    public decimal ItemsPerPurchasingUnit { get; set; } = 1;
    public string? PurchasePackageType   { get; set; }
    public decimal PurchaseQtyPerPackage { get; set; } = 1;
    public decimal PurchaseLength        { get; set; }
    public decimal PurchaseWidth         { get; set; }
    public decimal PurchaseHeight        { get; set; }
    public decimal PurchaseVolume        { get; set; }
    public decimal PurchaseWeight        { get; set; }
    public string? DutyGroup             { get; set; }
    public string? PurchaseVatCode       { get; set; }

    // 🔹 Planning
    public string? PlanningMethod    { get; set; }
    public string? ProcurementMethod { get; set; }
    public string? OrderInterval     { get; set; }
    public decimal OrderMultiple     { get; set; }
    public decimal MinimumOrderQty   { get; set; }
    public int     ToleranceDays     { get; set; }

    // 🔹 Production
    public bool    PhantomItem       { get; set; }
    public string? IssueMethod       { get; set; }
    public string? BomType           { get; set; }
    public decimal ProductionStdCost { get; set; }
    public bool    IncludeInProductionStdCostRollup { get; set; } = true;
}

public class ItemsMasterResponse : ItemsMasterFields
{
    public int    Id       { get; set; }
    public string ItemCode { get; set; } = null!;
    public ItemStatus Status { get; set; }

    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// 🔥 CREATE
public class ItemsMasterRequest : ItemsMasterFields
{
    public string ItemCode { get; set; } = string.Empty;
    /// <summary>0 = Active, 1 = Inactive</summary>
    public int Status { get; set; } = 0;
}

// 🔥 UPDATE — ItemCode is editable only while the item has no transactions.
public class ItemsMasterUpdateRequest : ItemsMasterFields
{
    /// <summary>Leave null/blank to keep the existing code.</summary>
    public string? ItemCode { get; set; }
    public ItemStatus Status { get; set; }
}

// ══════════════════════════════════════════════════════════════════
// 🔥 EXCEL IMPORT — bulk create/update Item Master from a spreadsheet.
//    Only the "core" columns are imported; everything else keeps the
//    entity defaults on insert and is left untouched on update.
// ══════════════════════════════════════════════════════════════════
public class ItemImportRow
{
    public string?  ItemCode     { get; set; }
    public string?  ItemName     { get; set; }
    public string?  ItemGroup    { get; set; }
    public string?  ItemType     { get; set; }
    public string?  Barcode      { get; set; }
    public string?  UomGroup     { get; set; }
    public string?  UomName      { get; set; }
    public bool?    SalesItem    { get; set; }
    public bool?    PurchaseItem { get; set; }
    public bool?    StockItem    { get; set; }
    public decimal? UnitPrice    { get; set; }
}

public class ItemImportRequest
{
    /// <summary>skip | update | reject — how to treat rows whose ItemCode already exists.</summary>
    public string Mode { get; set; } = "skip";
    public List<ItemImportRow> Items { get; set; } = new();
}

public class ItemImportRowResult
{
    public int     Row      { get; set; }   // 1-based position in the uploaded file
    public string? ItemCode { get; set; }
    public string  Status   { get; set; } = "";  // created | updated | skipped | failed
    public string? Message  { get; set; }
}

public class ItemImportResult
{
    public bool   Committed { get; set; }        // false when a reject-mode run was cancelled
    public string Mode      { get; set; } = "";
    public int    Total     { get; set; }
    public int    Created   { get; set; }
    public int    Updated   { get; set; }
    public int    Skipped   { get; set; }
    public int    Failed    { get; set; }
    public List<ItemImportRowResult> Rows { get; set; } = new();
}

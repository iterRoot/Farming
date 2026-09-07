using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.ItemsMaster;

public class ItemsMaster : AuditableEntity
{

    // 🔹 General
    public string ItemCode { get; set; } = null!;
    public string ItemName { get; set; } = null!;
    public string ItemGroup { get; set; } = string.Empty;
    public string ItemType { get; set; } = "Inventory";
    public string Barcode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;

    public string UomName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;

    public string ManageBy { get; set; } = "None";

    public bool SalesItem { get; set; } = true;
    public bool PurchaseItem { get; set; } = true;

    public ItemStatus Status { get; set; } = ItemStatus.Active;

    // 🔹 Inventory
    public string Warehouse { get; set; } = string.Empty;
    public decimal MinStock { get; set; }
    public decimal MaxStock { get; set; }
    public string ValuationMethod { get; set; } = "MovingAverage";

    // 🔹 Sales
    public decimal SalePrice { get; set; }
    public string PriceList { get; set; } = string.Empty;
    public string SaleTaxGroup { get; set; } = string.Empty;
    public decimal SaleDiscount { get; set; }
    public string SaleUom { get; set; } = string.Empty;
    public string SaleDescription { get; set; } = string.Empty;

    // 🔹 Purchasing
    public decimal PurchasePrice { get; set; }
    public string PreferredVendor { get; set; } = string.Empty;
    public string VendorItemNo { get; set; } = string.Empty;
    public string PurchaseUom { get; set; } = string.Empty;
    public int LeadTime { get; set; }
    public string PurchaseTaxGroup { get; set; } = string.Empty;

    // ══════════════════════════════════════════════════════════════
    // SAP B1 "Item Master Data" — additional scalar fields
    // ══════════════════════════════════════════════════════════════

    // 🔹 General
    public string? ForeignName        { get; set; }
    public string? UomGroup           { get; set; }
    public decimal UnitPrice          { get; set; }
    public string? PriceCurrency      { get; set; }
    public string? PricingUnit        { get; set; }
    public bool    StockItem          { get; set; } = true;
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

    // 🔹 Purchasing Data
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

    // 🔹 Sales Data
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

    // 🔹 Stock Data
    public string? SetGlAccountsBy       { get; set; } = "Item Group";
    public decimal StockWeight           { get; set; }
    public string? StockCountingUomCode  { get; set; }
    public string? StockCountingUomName  { get; set; }
    public decimal ItemsPerCountingUnit  { get; set; } = 1;
    public decimal ItemCost               { get; set; }
    public bool    ManageStockByWarehouse { get; set; }
    public decimal RequiredStock          { get; set; }

    // 🔹 Planning Data
    public string? PlanningMethod    { get; set; } = "None";
    public string? ProcurementMethod { get; set; } = "Buy";
    public string? OrderInterval     { get; set; }
    public decimal OrderMultiple     { get; set; }
    public decimal MinimumOrderQty   { get; set; }
    public int     ToleranceDays     { get; set; }

    // 🔹 Production Data
    public bool    PhantomItem        { get; set; }
    public string? IssueMethod        { get; set; } = "Backflush";
    public string? BomType            { get; set; }
    public decimal ProductionStdCost  { get; set; }
    public bool    IncludeInProductionStdCostRollup { get; set; } = true;
}
public class ItemsMasterConfig : IEntityTypeConfiguration<ItemsMaster>
{
    public void Configure(EntityTypeBuilder<ItemsMaster> builder)
    {
        builder.ToTable("KITM");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ItemCode).IsUnique();

        builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(100).IsRequired();

        builder.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PurchasePrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MinStock).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaxStock).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SaleDiscount).HasColumnType("decimal(18,2)");

        // ── SAP scalar additions ──────────────────────────────────
        foreach (var money in new[] { "UnitPrice", "ItemCost", "ProductionStdCost" })
            builder.Property<decimal>(money).HasColumnType("decimal(18,2)");

        foreach (var qty in new[]
        {
            "ItemsPerPurchasingUnit", "PurchaseQtyPerPackage",
            "PurchaseLength", "PurchaseWidth", "PurchaseHeight", "PurchaseVolume", "PurchaseWeight",
            "ItemsPerSalesUnit", "SalesQtyPerPackage",
            "SalesLength", "SalesWidth", "SalesHeight", "SalesVolume", "SalesWeight",
            "StockWeight", "ItemsPerCountingUnit", "RequiredStock",
            "OrderMultiple", "MinimumOrderQty",
        })
            builder.Property<decimal>(qty).HasColumnType("decimal(18,3)");

        builder.Property(x => x.ForeignName).HasMaxLength(150);
        builder.Property(x => x.UomGroup).HasMaxLength(50);
        builder.Property(x => x.PriceCurrency).HasMaxLength(10);
        builder.Property(x => x.PricingUnit).HasMaxLength(50);
        builder.Property(x => x.AdditionalIdentifier).HasMaxLength(100);
        builder.Property(x => x.ShippingType).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(1000);
        builder.Property(x => x.CountryOfOrigin).HasMaxLength(100);
        builder.Property(x => x.StandardItemIdentification).HasMaxLength(100);
        builder.Property(x => x.CommodityClassification).HasMaxLength(100);
        builder.Property(x => x.MfrCatalogueNo).HasMaxLength(100);
        builder.Property(x => x.DutyGroup).HasMaxLength(50);
        builder.Property(x => x.PurchaseVatCode).HasMaxLength(50);
        builder.Property(x => x.SalesVatCode).HasMaxLength(50);
        builder.Property(x => x.CreateQrCodeFrom).HasMaxLength(100);
        builder.Property(x => x.SetGlAccountsBy).HasMaxLength(50);
        builder.Property(x => x.PlanningMethod).HasMaxLength(50);
        builder.Property(x => x.ProcurementMethod).HasMaxLength(50);
        builder.Property(x => x.OrderInterval).HasMaxLength(50);
        builder.Property(x => x.IssueMethod).HasMaxLength(50);
        builder.Property(x => x.BomType).HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion(
                v => v == ItemStatus.Active ? 0 : 1,
                v => v == 0 ? ItemStatus.Active : ItemStatus.InActive
            );

        builder.HasCheckConstraint(
            "CK_ItemsMaster_ItemCode_NotEmpty",
            "\"ItemCode\" <> ''"
        );
    }
}
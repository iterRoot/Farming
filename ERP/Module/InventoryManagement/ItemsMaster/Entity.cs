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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FixedAssets;

// SAP B1 "Asset Master Data" — an item whose Item Type is Fixed Assets, with a
// dedicated Fixed Assets (depreciation) block on top of the general item fields.
public class AssetMaster : AuditableEntity
{
    // ── Header ──────────────────────────────────────────────
    public string   ItemNo        { get; set; } = null!;   // asset code
    public string   Description   { get; set; } = null!;   // item name
    public string?  ForeignName   { get; set; }
    public string   ItemType      { get; set; } = "Fixed Assets";
    public string?  ItemGroup     { get; set; }
    public string?  UomGroup      { get; set; }
    public string?  Barcode       { get; set; }
    public string?  PriceList     { get; set; }
    public decimal  UnitPrice     { get; set; }

    // ── General tab ─────────────────────────────────────────
    public bool     WithholdingTaxLiable   { get; set; }
    public bool     DoNotApplyDiscountGroups { get; set; }
    public bool     DoNotApplyDiscount     { get; set; }
    public string?  Manufacturer  { get; set; }
    public string?  AdditionalIdentifier { get; set; }
    public string?  ShippingType  { get; set; }
    public string   ManageItemBy  { get; set; } = "None";  // None | Serial Numbers | Batches
    public bool     Active        { get; set; } = true;
    public string?  LinkedToResource { get; set; }
    public string?  StandardItemIdentification { get; set; }
    public string?  CommodityClassification { get; set; }

    // ── Fixed Assets tab ────────────────────────────────────
    public string?  AssetClass    { get; set; }
    public string?  AssetGroup    { get; set; }
    public DateOnly? CapitalizationDate { get; set; }
    public int      UsefulLifeMonths      { get; set; }
    public int      RemainingLifeMonths   { get; set; }
    public string   DepreciationMethod    { get; set; } = "Straight Line";
    public decimal  AcquisitionCost       { get; set; }
    public decimal  SalvageValue          { get; set; }
    public decimal  AccumulatedDepreciation { get; set; }
    public string   AssetStatus   { get; set; } = "New";   // New | Active | Inactive | Deactivated
    public string?  DepreciationAccount   { get; set; }
    public string?  AssetAccount          { get; set; }
}

public class AssetMasterConfig : IEntityTypeConfiguration<AssetMaster>
{
    public void Configure(EntityTypeBuilder<AssetMaster> b)
    {
        b.ToTable("KAST");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Description).HasMaxLength(200).IsRequired();
        b.Property(x => x.ManageItemBy).HasMaxLength(20).IsRequired();
        b.Property(x => x.DepreciationMethod).HasMaxLength(30).IsRequired();
        b.Property(x => x.AssetStatus).HasMaxLength(20).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.AcquisitionCost).HasPrecision(18, 2);
        b.Property(x => x.SalvageValue).HasPrecision(18, 2);
        b.Property(x => x.AccumulatedDepreciation).HasPrecision(18, 2);
        b.HasIndex(x => x.ItemNo).IsUnique();
    }
}

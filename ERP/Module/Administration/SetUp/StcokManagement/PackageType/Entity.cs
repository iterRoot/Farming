using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.PackageTypes;

// ═══════════════════════════════════════════════════════════════
// PACKAGE TYPE (KPKT)
// SAP B1 equivalent: Package Types setup
// Defines containers used for shipping goods:
// Box, Pallet, Bag, Drum, Crate, etc.
// ═══════════════════════════════════════════════════════════════
public class PackageType : AuditableEntity
{
    public string   Code       { get; set; } = null!;  // e.g. "PKG-BOX"
    public string   Name       { get; set; } = null!;  // e.g. "Standard Cardboard Box"
    public string?  Material   { get; set; }           // Cardboard, Plastic, Wood, Metal
    public decimal? Width      { get; set; }           // cm
    public decimal? Height     { get; set; }           // cm
    public decimal? Length     { get; set; }           // cm
    public decimal? MaxWeight  { get; set; }           // kg max capacity
    public decimal? Tare       { get; set; }           // kg weight of empty package
    public string?  DimUnit    { get; set; } = "CM";  // dimension unit
    public string?  WeightUnit { get; set; } = "KG";  // weight unit
    public string?  Description{ get; set; }
    public bool     IsActive   { get; set; } = true;
    public string?  Remarks    { get; set; }
}

public class PackageTypeConfig : IEntityTypeConfiguration<PackageType>
{
    public void Configure(EntityTypeBuilder<PackageType> builder)
    {
        builder.ToTable("KPKT");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Material).HasMaxLength(50);
        builder.Property(x => x.DimUnit).HasMaxLength(10);
        builder.Property(x => x.WeightUnit).HasMaxLength(10);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.Property(x => x.Width).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Height).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Length).HasColumnType("decimal(18,2)");
        builder.Property(x => x.MaxWeight).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Tare).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
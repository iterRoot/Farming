using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.BarCode;

// ═══════════════════════════════════════════════════════════════════
// BAR CODE HEADER (KBCD)
// Linked to Item Master — one item can have multiple barcodes
// ═══════════════════════════════════════════════════════════════════
public class BarCode : AuditableEntity
{
    public string   ItemNo          { get; set; } = null!;   // Item Master code
    public string?  ItemDescription { get; set; }             // Auto-filled from item
    public string?  UoMGroup        { get; set; }             // Unit of Measure Group
    public int      VersionNum      { get; set; } = 1;

    // Navigation
    public ICollection<BarCodeLine> Lines { get; set; } = new List<BarCodeLine>();
}

// ═══════════════════════════════════════════════════════════════════
// BAR CODE LINE (BCD1)
// Each item can have multiple barcodes per UoM
// ═══════════════════════════════════════════════════════════════════
public class BarCodeLine : AuditableEntity
{
    public int     BarCodeId   { get; set; }
    public int     LineNum     { get; set; }
    public string? UoM         { get; set; }   // Unit of Measure
    public string  Code        { get; set; } = null!;  // The actual barcode value
    public string? FreeText    { get; set; }   // Optional description
    public bool    IsDefault   { get; set; } = false;  // Set As Default

    // Navigation
    public BarCode BarCode { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class BarCodeConfig : IEntityTypeConfiguration<BarCode>
{
    public void Configure(EntityTypeBuilder<BarCode> builder)
    {
        builder.ToTable("KBCD");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.ItemNo).IsUnique();
        builder.Property(m => m.ItemDescription).HasMaxLength(200);
        builder.Property(m => m.UoMGroup).HasMaxLength(50);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        builder.HasIndex(m => m.ItemNo);
    }
}

public class BarCodeLineConfig : IEntityTypeConfiguration<BarCodeLine>
{
    public void Configure(EntityTypeBuilder<BarCodeLine> builder)
    {
        builder.ToTable("BCD1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.UoM).HasMaxLength(50);
        builder.Property(m => m.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => m.Code);
        builder.Property(m => m.FreeText).HasMaxLength(200);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);

        builder.HasOne(m => m.BarCode)
            .WithMany(b => b.Lines)
            .HasForeignKey(m => m.BarCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.BarCodeId);
        builder.HasIndex(m => m.IsDefault);
    }
}
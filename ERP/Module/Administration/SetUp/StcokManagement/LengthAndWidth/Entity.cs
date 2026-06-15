using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.LengthWidth;

// ═══════════════════════════════════════════════════════════════
// LENGTH & WIDTH UOM (OLWU)
// SAP B1 equivalent: Length and Width UoM setup
// Units used to measure item dimensions: m, cm, mm, inch, foot
//   ConversionToMeter normalizes everything to meters
// ═══════════════════════════════════════════════════════════════
public class LengthWidthUom : AuditableEntity
{
    public string  Code              { get; set; } = null!;  // e.g. "CM"
    public string  Name              { get; set; } = null!;  // e.g. "Centimeter"
    public string? Symbol            { get; set; }           // e.g. "cm"
    public decimal ConversionToMeter { get; set; } = 1;      // 1 unit = X meters (CM = 0.01)
    public bool    IsActive          { get; set; } = true;
    public string? Remarks           { get; set; }
}

public class LengthWidthUomConfig : IEntityTypeConfiguration<LengthWidthUom>
{
    public void Configure(EntityTypeBuilder<LengthWidthUom> builder)
    {
        builder.ToTable("KLWU");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Symbol).HasMaxLength(20);
        builder.Property(x => x.ConversionToMeter).HasColumnType("decimal(18,6)");
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
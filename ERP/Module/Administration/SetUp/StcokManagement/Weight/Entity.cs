using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.Weight;

// ═══════════════════════════════════════════════════════════════
// WEIGHT UOM (OWTU)
// SAP B1 equivalent: Weight UoM setup
// Units used to measure item weight: KG, G, LB, OZ, TON
//   ConversionToKg normalizes everything to kilograms
// ═══════════════════════════════════════════════════════════════
public class WeightUom : AuditableEntity
{
    public string  Code             { get; set; } = null!;  // e.g. "KG"
    public string  Name             { get; set; } = null!;  // e.g. "Kilogram"
    public string? Symbol           { get; set; }           // e.g. "kg"
    public decimal ConversionToKg   { get; set; } = 1;      // 1 unit = X kg (G = 0.001)
    public bool    IsActive         { get; set; } = true;
    public string? Remarks          { get; set; }
}

public class WeightUomConfig : IEntityTypeConfiguration<WeightUom>
{
    public void Configure(EntityTypeBuilder<WeightUom> builder)
    {
        builder.ToTable("KWTU");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Symbol).HasMaxLength(20);
        builder.Property(x => x.ConversionToKg).HasColumnType("decimal(18,6)");
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
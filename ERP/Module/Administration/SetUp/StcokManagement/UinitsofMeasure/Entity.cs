using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.UnitOfMeasure;

// ═══════════════════════════════════════════════════════════════
// UNIT OF MEASURE (OUOM)
// SAP B1 equivalent: OUOM — base units used across items
// e.g. KG, PCS, BOX, LITER, BAG, TON
// ═══════════════════════════════════════════════════════════════
public class UnitOfMeasure : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "KG"
    public string  Name        { get; set; } = null!;  // e.g. "Kilogram"
    public string? Description { get; set; }
    public string? Category    { get; set; }           // Weight / Volume / Length / Count
    public decimal? DecimalPlaces { get; set; }        // precision for this unit
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class UnitOfMeasureConfig : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("KUOM");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Category).HasMaxLength(50);
        builder.Property(x => x.DecimalPlaces).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
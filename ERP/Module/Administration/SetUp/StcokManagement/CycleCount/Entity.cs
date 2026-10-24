using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.CycleCount;

// ═══════════════════════════════════════════════════════════════
// CYCLE COUNT DETERMINATION (KCCD)
// SAP B1 equivalent: Cycle Count Determination
// Defines rules for how often each item class (A/B/C) should
// be physically counted in the warehouse.
//
// ABC Classification:
//   A = High value / fast moving → count most frequently
//   B = Medium value / medium moving
//   C = Low value / slow moving → count least frequently
// ═══════════════════════════════════════════════════════════════
public class CycleCountDetermination : AuditableEntity
{
    public string  Code            { get; set; } = null!;  // e.g. "CCD-001"
    public string  Name            { get; set; } = null!;  // e.g. "Class A - Monthly"
    public string  ItemClass       { get; set; } = "A";    // A / B / C
    public string  AlertType       { get; set; } = "ByDate"; // ByDate / ByQuantity
    public int     CountFrequency  { get; set; }           // days between counts
    public string? WarehouseCode   { get; set; }           // specific warehouse (null = all)
    public string? ItemGroupCode   { get; set; }           // specific item group (null = all)
    public string? Description     { get; set; }
    public bool    IsActive        { get; set; } = true;
    public string? Remarks         { get; set; }
}

public class CycleCountDeterminationConfig
    : IEntityTypeConfiguration<CycleCountDetermination>
{
    public void Configure(EntityTypeBuilder<CycleCountDetermination> builder)
    {
        builder.ToTable("KCCD");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ItemClass).HasMaxLength(5);
        builder.Property(x => x.AlertType).HasMaxLength(20);
        builder.Property(x => x.WarehouseCode).HasMaxLength(50);
        builder.Property(x => x.ItemGroupCode).HasMaxLength(50);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
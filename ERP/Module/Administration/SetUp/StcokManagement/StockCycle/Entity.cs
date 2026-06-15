using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.StockCycles;

// ═══════════════════════════════════════════════════════════════
// STOCK CYCLE (KSTC)
// SAP B1 equivalent: Inventory Counting / Stock Cycle
// Represents a physical stock count cycle event:
// Open → In Progress → Completed / Cancelled
// ═══════════════════════════════════════════════════════════════
public class StockCycle : AuditableEntity
{
    public string    Code           { get; set; } = null!;  // e.g. "SC-2026-Q1"
    public string    Name           { get; set; } = null!;  // e.g. "Q1 2026 Stock Count"
    public string    CycleType      { get; set; } = "Monthly"; // Weekly/Monthly/Quarterly/Annual
    public string    Status         { get; set; } = "Open";   // Open/InProgress/Completed/Cancelled
    public DateTime  StartDate      { get; set; }
    public DateTime  EndDate        { get; set; }
    public DateTime? CompletedDate  { get; set; }
    public string?   WarehouseCode  { get; set; }
    public string?   ResponsibleBy  { get; set; }            // person in charge
    public string?   Description    { get; set; }
    public bool      IsActive       { get; set; } = true;
    public string?   Remarks        { get; set; }
}

public class StockCycleConfig : IEntityTypeConfiguration<StockCycle>
{
    public void Configure(EntityTypeBuilder<StockCycle> builder)
    {
        builder.ToTable("KSTC");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CycleType).HasMaxLength(20);
        builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
        builder.Property(x => x.WarehouseCode).HasMaxLength(50);
        builder.Property(x => x.ResponsibleBy).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
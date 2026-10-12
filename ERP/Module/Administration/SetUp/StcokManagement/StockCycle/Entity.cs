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
    public int?      JournalEntryId { get; set; }   // set when completing posted a value difference

    public ICollection<StockCycleLine> Lines { get; set; } = new List<StockCycleLine>();
}

// One item in one warehouse to count. SystemQty is the book quantity when the
// cycle started; CountedQty is what the counter found (null = not counted yet).
public class StockCycleLine
{
    public int      Id           { get; set; }
    public int      StockCycleId { get; set; }
    public string   ItemCode     { get; set; } = null!;
    public string   ItemName     { get; set; } = null!;
    public string?  Uom          { get; set; }
    public string   WhsCode      { get; set; } = null!;
    public decimal  SystemQty    { get; set; }
    public decimal? CountedQty   { get; set; }
    public decimal? PostedQty    { get; set; }      // difference actually posted at completion (+ found, − missing)
    public decimal? PostedUnitCost { get; set; }

    public StockCycle StockCycle { get; set; } = null!;
}

public class StockCycleLineConfig : IEntityTypeConfiguration<StockCycleLine>
{
    public void Configure(EntityTypeBuilder<StockCycleLine> b)
    {
        b.ToTable("STC1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.Uom).HasMaxLength(30);
        b.Property(x => x.WhsCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.SystemQty).HasPrecision(18, 4);
        b.Property(x => x.CountedQty).HasPrecision(18, 4);
        b.Property(x => x.PostedQty).HasPrecision(18, 4);
        b.Property(x => x.PostedUnitCost).HasPrecision(18, 4);
        b.HasIndex(x => new { x.StockCycleId, x.ItemCode, x.WhsCode }).IsUnique();
        b.HasOne(x => x.StockCycle).WithMany(c => c.Lines).HasForeignKey(x => x.StockCycleId).OnDelete(DeleteBehavior.Cascade);
    }
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
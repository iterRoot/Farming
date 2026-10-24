using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.ProductionOrder;

// ═══════════════════════════════════════════════════════════════════
// PRODUCTION ORDER  (KWOR / WOR1)
// SAP B1 equivalent: Production Order. Planned -> Released -> Closed
// (or Cancelled from either open state). Component lines are a scaled
// snapshot of the BOM taken at Create time — later BOM edits do not
// retroactively change an order already in flight.
// ═══════════════════════════════════════════════════════════════════
public class ProductionOrder : AuditableEntity
{
    public string   DocNum           { get; set; } = null!;   // WO-2026-00001
    public string   ItemCode         { get; set; } = null!;   // finished good produced
    public string   ItemName         { get; set; } = null!;
    public int      BillOfMaterialId { get; set; }
    public string   BomCode          { get; set; } = null!;   // display snapshot
    public decimal  PlannedQty       { get; set; }
    public string   Warehouse        { get; set; } = null!;
    public DateTime DueDate          { get; set; }
    public string   Status           { get; set; } = "Planned"; // Planned|Released|Closed|Cancelled
    public DateTime? ClosedDate      { get; set; }
    public int?      JournalEntryId  { get; set; }
    public string?   Remarks         { get; set; }

    public ICollection<ProductionOrderLine> Lines { get; set; } = new List<ProductionOrderLine>();
}

public class ProductionOrderLine
{
    public int     Id                { get; set; }
    public int     ProductionOrderId { get; set; }
    public int     LineNum           { get; set; }
    public string  ItemCode          { get; set; } = null!;
    public string  ItemName          { get; set; } = null!;
    public decimal PlannedQty        { get; set; }   // component qty needed, scaled from the BOM
    public string  Warehouse         { get; set; } = null!;

    public ProductionOrder ProductionOrder { get; set; } = null!;
}

public class ProductionOrderConfig : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> b)
    {
        b.ToTable("KWOR");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocNum).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.DocNum).IsUnique();
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.BomCode).HasMaxLength(30);
        b.Property(x => x.PlannedQty).HasPrecision(18, 4);
        b.Property(x => x.Warehouse).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("Planned");
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => x.Status);

        // No FK to BillOfMaterial — a BOM outlives the orders built from it,
        // and this codebase's own history (Landed Costs, GRPO) shows a
        // Restrict-FK here crashes deletes; the in-use check lives in the
        // BOM controller's own Delete action instead.
        b.HasMany(x => x.Lines).WithOne(l => l.ProductionOrder)
         .HasForeignKey(l => l.ProductionOrderId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductionOrderLineConfig : IEntityTypeConfiguration<ProductionOrderLine>
{
    public void Configure(EntityTypeBuilder<ProductionOrderLine> b)
    {
        b.ToTable("WOR1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.PlannedQty).HasPrecision(18, 4);
        b.Property(x => x.Warehouse).HasMaxLength(50).IsRequired();
    }
}

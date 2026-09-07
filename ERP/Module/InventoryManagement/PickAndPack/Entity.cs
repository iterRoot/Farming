using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.PickAndPack;

// ══════════════════════════════════════════════════════════════════════════════
// PICK & PACK — a saved selection criteria that generates a pick list from open
// source documents (Sale Orders, etc.). Table: OPKL (header) / PKL1 (lines) /
// filters stored as JSON on the header.
// ══════════════════════════════════════════════════════════════════════════════
public class PickPackCriteria : AuditableEntity
{
    public string  CriteriaName { get; set; } = "";
    public string  Status       { get; set; } = "Open";   // Open | Released | Picked | Closed
    public string? GroupBy      { get; set; }
    public string? SortBy       { get; set; }
    public bool    ReleaseManually { get; set; }

    // Manage — which source documents to pull from
    public bool ManageSalesOrders          { get; set; } = true;
    public bool ManageReserveInvoices       { get; set; }
    public bool ManageProductionOrders      { get; set; }
    public bool ManageStockTransferRequests { get; set; }

    public List<string> Warehouses { get; set; } = new();   // warehouse codes
    public List<PickPackFilter> Filters { get; set; } = new(); // JSON

    public ICollection<PickPackLine> Lines { get; set; } = new List<PickPackLine>();
}

public class PickPackFilter
{
    public string? Field { get; set; }
    public string? From  { get; set; }
    public string? To    { get; set; }
}

public class PickPackLine : AuditableEntity
{
    public int    PickPackCriteriaId { get; set; }
    public PickPackCriteria PickPackCriteria { get; set; } = null!;

    public int      LineNum      { get; set; }
    public string   BaseDocType  { get; set; } = "SaleOrder";
    public int      BaseDocEntry { get; set; }
    public string?  BaseDocNum   { get; set; }
    public string?  CustomerName { get; set; }
    public DateTime? DueDate     { get; set; }

    public string   ItemCode   { get; set; } = "";
    public string?  ItemName   { get; set; }
    public string?  WhsCode    { get; set; }
    public decimal  RequiredQty { get; set; }
    public decimal  PickedQty   { get; set; }
    public string   Status      { get; set; } = "Open";   // Open | Picked
}

public class PickPackCriteriaConfig : IEntityTypeConfiguration<PickPackCriteria>
{
    public void Configure(EntityTypeBuilder<PickPackCriteria> b)
    {
        b.ToTable("OPKL");
        b.HasKey(x => x.Id);
        b.Property(x => x.CriteriaName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.GroupBy).HasMaxLength(50);
        b.Property(x => x.SortBy).HasMaxLength(50);
        b.OwnsMany(x => x.Filters, nb => nb.ToJson());
        b.HasMany(x => x.Lines).WithOne(l => l.PickPackCriteria)
         .HasForeignKey(l => l.PickPackCriteriaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PickPackLineConfig : IEntityTypeConfiguration<PickPackLine>
{
    public void Configure(EntityTypeBuilder<PickPackLine> b)
    {
        b.ToTable("PKL1");
        b.HasKey(x => x.Id);
        b.Property(x => x.BaseDocType).HasMaxLength(30);
        b.Property(x => x.BaseDocNum).HasMaxLength(50);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.WhsCode).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.RequiredQty).HasPrecision(18, 2);
        b.Property(x => x.PickedQty).HasPrecision(18, 2);
    }
}

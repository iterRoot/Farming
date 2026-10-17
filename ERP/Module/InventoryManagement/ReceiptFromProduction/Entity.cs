using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.ReceiptFromProduction;

// ═══════════════════════════════════════════════════════════════════
// RECEIPT FROM PRODUCTION  (KIGN / IGN1)
// SAP B1 equivalent: Production > Receipt from Production. Brings
// finished goods into stock (outside the all-in-one Production Order
// "Close"). Posting moves stock IN at the given unit cost and writes a
// balanced journal entry (Dr Inventory / Cr WIP).
// ═══════════════════════════════════════════════════════════════════
public class ReceiptFromProduction : AuditableEntity
{
    public string   DocNum            { get; set; } = null!;   // RP-2026-00001
    public DateTime PostingDate       { get; set; }
    public string   Warehouse         { get; set; } = null!;
    public string?  ProductionOrderNo { get; set; }
    public string   Status            { get; set; } = "C";
    public int?     JournalEntryId    { get; set; }
    public decimal  TotalCost         { get; set; }
    public string?  Remarks           { get; set; }

    public ICollection<ReceiptFromProductionLine> Lines { get; set; } = new List<ReceiptFromProductionLine>();
}

public class ReceiptFromProductionLine
{
    public int     Id                      { get; set; }
    public int     ReceiptFromProductionId { get; set; }
    public int     LineNum                 { get; set; }
    public string  ItemCode                { get; set; } = null!;
    public string  ItemName                { get; set; } = null!;
    public decimal Quantity                { get; set; }
    public decimal UnitCost                { get; set; }
    public decimal Total                   { get; set; }

    public ReceiptFromProduction ReceiptFromProduction { get; set; } = null!;
}

public class ReceiptFromProductionConfig : IEntityTypeConfiguration<ReceiptFromProduction>
{
    public void Configure(EntityTypeBuilder<ReceiptFromProduction> b)
    {
        b.ToTable("KIGN");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocNum).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.DocNum).IsUnique();
        b.Property(x => x.Warehouse).HasMaxLength(50).IsRequired();
        b.Property(x => x.ProductionOrderNo).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(5);
        b.Property(x => x.TotalCost).HasPrecision(18, 2);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasMany(x => x.Lines).WithOne(l => l.ReceiptFromProduction)
         .HasForeignKey(l => l.ReceiptFromProductionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReceiptFromProductionLineConfig : IEntityTypeConfiguration<ReceiptFromProductionLine>
{
    public void Configure(EntityTypeBuilder<ReceiptFromProductionLine> b)
    {
        b.ToTable("IGN1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.UnitCost).HasPrecision(18, 4);
        b.Property(x => x.Total).HasPrecision(18, 2);
    }
}

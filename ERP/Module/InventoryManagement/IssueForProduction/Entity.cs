using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.IssueForProduction;

// ═══════════════════════════════════════════════════════════════════
// ISSUE FOR PRODUCTION  (KIGE / IGE1)
// SAP B1 equivalent: Production > Issue for Production. Consumes
// component stock for manufacturing outside the all-in-one Production
// Order "Close" (e.g. a partial or manual issue). Posting moves stock
// OUT at valuation cost and writes a balanced journal entry.
// ═══════════════════════════════════════════════════════════════════
public class IssueForProduction : AuditableEntity
{
    public string   DocNum            { get; set; } = null!;   // IP-2026-00001
    public DateTime PostingDate       { get; set; }
    public string   Warehouse         { get; set; } = null!;
    public string?  ProductionOrderNo { get; set; }            // optional link (display only)
    public string   Status            { get; set; } = "C";     // C = posted
    public int?     JournalEntryId    { get; set; }
    public decimal  TotalCost         { get; set; }
    public string?  Remarks           { get; set; }

    public ICollection<IssueForProductionLine> Lines { get; set; } = new List<IssueForProductionLine>();
}

public class IssueForProductionLine
{
    public int     Id                   { get; set; }
    public int     IssueForProductionId { get; set; }
    public int     LineNum              { get; set; }
    public string  ItemCode             { get; set; } = null!;
    public string  ItemName             { get; set; } = null!;
    public decimal Quantity             { get; set; }
    public decimal Cost                 { get; set; }   // valuation cost the issue was posted at

    public IssueForProduction IssueForProduction { get; set; } = null!;
}

public class IssueForProductionConfig : IEntityTypeConfiguration<IssueForProduction>
{
    public void Configure(EntityTypeBuilder<IssueForProduction> b)
    {
        b.ToTable("KIGE");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocNum).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.DocNum).IsUnique();
        b.Property(x => x.Warehouse).HasMaxLength(50).IsRequired();
        b.Property(x => x.ProductionOrderNo).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(5);
        b.Property(x => x.TotalCost).HasPrecision(18, 2);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasMany(x => x.Lines).WithOne(l => l.IssueForProduction)
         .HasForeignKey(l => l.IssueForProductionId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class IssueForProductionLineConfig : IEntityTypeConfiguration<IssueForProductionLine>
{
    public void Configure(EntityTypeBuilder<IssueForProductionLine> b)
    {
        b.ToTable("IGE1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Cost).HasPrecision(18, 2);
    }
}

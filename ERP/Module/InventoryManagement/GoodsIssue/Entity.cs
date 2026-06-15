using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.GoodsIssue;

// ═══════════════════════════════════════════════════════════════
// GOODS ISSUE HEADER (KGIS)
// ═══════════════════════════════════════════════════════════════
public class GoodsIssue : AuditableEntity
{
    public string?   DocNo        { get; set; }   // GI-2026-00001 (auto)
    public string?   Status       { get; set; }   // Open / Closed / Cancelled

    public string?   CustomerCode { get; set; }
    public string?   CustomerName { get; set; }

    public string?   Address      { get; set; }
    public string?   VendorTax    { get; set; }
    public string?   Remarks      { get; set; }

    public DateTime  PostingDate  { get; set; }
    public DateTime  DocumentDate { get; set; }
    public DateTime  DueDate      { get; set; }

    // Base document reference (e.g. from GoodsReceipt)
    public int?      BaseDocId    { get; set; }
    public string?   BaseDocType  { get; set; }

    public decimal TotalBeforeDiscount { get; set; }
    public decimal Discount            { get; set; }
    public decimal Tax                 { get; set; }
    public decimal GrandTotal          { get; set; }

    public ICollection<GoodsIssueLine> Lines { get; set; } = new List<GoodsIssueLine>();
}

// ═══════════════════════════════════════════════════════════════
// GOODS ISSUE LINE (GIS1)
// ═══════════════════════════════════════════════════════════════
public class GoodsIssueLine
{
    public int  Id           { get; set; }
    public int  GoodsIssueId { get; set; }
    public int  LineNum      { get; set; }

    public GoodsIssue? GoodsIssue { get; set; }

    public string  ItemCode { get; set; } = null!;
    public string  ItemName { get; set; } = null!;

    public decimal Qty   { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}

// ═══════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════
public class GoodsIssueConfig : IEntityTypeConfiguration<GoodsIssue>
{
    public void Configure(EntityTypeBuilder<GoodsIssue> builder)
    {
        builder.ToTable("GoodsIssue");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNo).HasMaxLength(50);
        builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
        builder.Property(x => x.CustomerCode).HasMaxLength(50);
        builder.Property(x => x.CustomerName).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(255);
        builder.Property(x => x.VendorTax).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(255);
        builder.Property(x => x.BaseDocType).HasMaxLength(50);

        builder.Property(x => x.TotalBeforeDiscount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Discount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Tax).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.GoodsIssue)
               .HasForeignKey(x => x.GoodsIssueId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GoodsIssueLineConfig : IEntityTypeConfiguration<GoodsIssueLine>
{
    public void Configure(EntityTypeBuilder<GoodsIssueLine> builder)
    {
        builder.ToTable("GoodsIssueLine");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(100);
        builder.Property(x => x.Qty).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
    }
}
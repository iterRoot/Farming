using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.GoodsReceipt;

// ═══════════════════════════════════════════════════════════════
// GOODS RECEIPT HEADER
// ═══════════════════════════════════════════════════════════════
public class GoodsReceipt : AuditableEntity
{
    public string? DocNo          { get; set; }   // ✅ GRC-2026-00001 (auto)
    public string? Status         { get; set; }   // ✅ Open / Closed / Cancelled

    public string? CustomerCode   { get; set; }
    public string? CustomerName   { get; set; }
    public int     DocumentNumber { get; set; }

    public string? Address        { get; set; }
    public string? VendorTax      { get; set; }

    public DateTime PostingDate   { get; set; }
    public DateTime DocumentDate  { get; set; }
    public DateTime DueDate       { get; set; }

    public decimal TotalBeforeDiscount { get; set; }
    public decimal Discount            { get; set; }
    public decimal Tax                 { get; set; }
    public decimal GrandTotal          { get; set; }

    public string? Remarks        { get; set; }

    public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}

// ═══════════════════════════════════════════════════════════════
// GOODS RECEIPT LINE
// ═══════════════════════════════════════════════════════════════
public class GoodsReceiptLine
{
    public int  Id             { get; set; }
    public int  GoodsReceiptId { get; set; }
    public int  LineNum        { get; set; }

    public GoodsReceipt? GoodsReceipt { get; set; }

    public string  ItemCode { get; set; } = null!;
    public string  ItemName { get; set; } = null!;

    public decimal Qty   { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}

// ═══════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════
public class GoodsReceiptConfig : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("GoodsReceipt");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNo).HasMaxLength(50);
        builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");

        builder.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(255);
        builder.Property(x => x.VendorTax).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(255);

        builder.Property(x => x.TotalBeforeDiscount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Discount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Tax).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.GoodsReceipt)
               .HasForeignKey(x => x.GoodsReceiptId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GoodsReceiptLineConfig : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("GoodsReceiptLine");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ItemName).HasMaxLength(100);
        builder.Property(x => x.Qty).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
    }
}
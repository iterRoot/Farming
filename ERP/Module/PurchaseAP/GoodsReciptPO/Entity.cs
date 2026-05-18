using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

// HEADER
public class GoodsReceiptPO : AuditableEntity
{
    public string DocNum { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public string Status { get; set; } = "O";
    public int VendorId { get; set; }
    public BPEntity Vendor { get; set; } = null!;
    public decimal Total { get; set; }
    public ICollection<GoodsReceiptPOLine> Lines { get; set; } = new List<GoodsReceiptPOLine>();
}

// LINE
public class GoodsReceiptPOLine : AuditableEntity
{
    public int LineNum { get; set; }

    public int GoodsReceiptPOId { get; set; }
    public GoodsReceiptPO GoodsReceiptPO { get; set; } = null!;

    public int ItemId { get; set; }
    public ItemsMaster Item { get; set; } = null!;

    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }

    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }

    // 🔥 Base = Purchase Order
    public int? BaseEntry { get; set; }
    public int? BaseLine { get; set; }
    public string? BaseType { get; set; } // "PPO"
}

// CONFIG
public class GoodsReceiptPOConfig : IEntityTypeConfiguration<GoodsReceiptPO>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptPO> builder)
    {
        builder.ToTable("KPGR");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DocNum).IsUnique();

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(1);

        builder.HasCheckConstraint("CK_GRPO_Status","\"Status\" IN ('O','C')");

        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.GoodsReceiptPO)
               .HasForeignKey(x => x.GoodsReceiptPOId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GoodsReceiptPOLineConfig : IEntityTypeConfiguration<GoodsReceiptPOLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptPOLine> builder)
    {
        builder.ToTable("PGR1");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.GoodsReceiptPOId, x.LineNum }).IsUnique();

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using GRPOEntity = FarmingApi.Modules.PurchaseAP.GoodsReceiptPO.GoodsReceiptPO;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReturnRequest;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class GoodsReturnRequest : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    public int      VendorId { get; set; }
    public BPEntity Vendor   { get; set; } = null!;

    public ICollection<GoodsReturnRequestLine> Items { get; set; } = new List<GoodsReturnRequestLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class GoodsReturnRequestLine : AuditableEntity
{
    public int                GoodsReturnRequestId { get; set; }
    public GoodsReturnRequest GoodsReturnRequest   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // ── SAP base doc (from GoodsReceiptPO) ───────────────────────────────
    public int?    BaseEntry { get; set; }   // FK → GoodsReceiptPO.Id
    public string? BaseType  { get; set; }   // "GoodsReceiptPO"

    public GRPOEntity? BaseGoodsReceipt { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class GoodsReturnRequestConfig : IEntityTypeConfiguration<GoodsReturnRequest>
{
    public void Configure(EntityTypeBuilder<GoodsReturnRequest> builder)
    {
        builder.ToTable("KPGQ");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(10);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
               .WithOne(x => x.GoodsReturnRequest)
               .HasForeignKey(x => x.GoodsReturnRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GoodsReturnRequestLineConfig : IEntityTypeConfiguration<GoodsReturnRequestLine>
{
    public void Configure(EntityTypeBuilder<GoodsReturnRequestLine> builder)
    {
        builder.ToTable("PGQ1");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).HasMaxLength(100);
        builder.Property(x => x.ItemName).HasMaxLength(200);
        builder.Property(x => x.BaseType).HasMaxLength(100);
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Item)
               .WithMany()
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BaseGoodsReceipt)
               .WithMany()
               .HasForeignKey(x => x.BaseEntry)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
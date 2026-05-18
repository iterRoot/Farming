using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using PQEntity = FarmingApi.Modules.PurchaseAP.PurchaseQuotation.PurchaseQuotation;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.PurchaseOrder;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseOrder : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";   // O=Open, C=Closed
    public decimal   Total       { get; set; }

    // ── FK → BusinessPartner (Vendor) ─────────────────────────────────────
    public int      VendorId { get; set; }
    public BPEntity Vendor   { get; set; } = null!;

    // ── Lines ─────────────────────────────────────────────────────────────
    public ICollection<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseOrderLine : AuditableEntity
{
    public int              PurchaseOrderId { get; set; }
    public PurchaseOrder    PurchaseOrder   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // ── SAP-style base document linkage (from PurchaseQuotation) ──────────
    public int?    BaseEntry { get; set; }   // FK → PurchaseQuotation.Id
    public string? BaseType  { get; set; }   // "PurchaseQuotation"

    public PQEntity? BaseQuotation { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseOrderConfig : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("KPPO");   // ✅ Purchase Order header
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(10);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.PurchaseOrder)
               .HasForeignKey(x => x.PurchaseOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseOrderLineConfig : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("PPO1");   // ✅ Purchase Order lines
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

        // ✅ Optional FK → PurchaseQuotation (nullable)
        builder.HasOne(x => x.BaseQuotation)
               .WithMany()
               .HasForeignKey(x => x.BaseEntry)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
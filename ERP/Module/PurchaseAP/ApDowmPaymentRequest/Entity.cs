using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using POEntity = FarmingApi.Modules.PurchaseAP.PurchaseOrder.PurchaseOrder;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentRequest;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentRequest : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";   // O=Open, C=Closed
    public string    Type        { get; set; } = "Item"; // Item | Service
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    // ── FK → BusinessPartner (Vendor) ─────────────────────────────────────
    public int      VendorId { get; set; }
    public BPEntity Vendor   { get; set; } = null!;

    // ── SAP-style base document linkage (from PurchaseOrder) ──────────────
    public int?    BaseEntry { get; set; }   // FK → PurchaseOrder.Id
    public string? BaseType  { get; set; }   // "PurchaseOrder"

    public POEntity? BaseOrder { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentRequestConfig : IEntityTypeConfiguration<APDownPaymentRequest>
{
    public void Configure(EntityTypeBuilder<APDownPaymentRequest> builder)
    {
        builder.ToTable("KPDR");   // ✅ AP Down Payment Request header
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(10);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        // ✅ Optional FK → PurchaseOrder (nullable)
        builder.HasOne(x => x.BaseOrder)
               .WithMany()
               .HasForeignKey(x => x.BaseEntry)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
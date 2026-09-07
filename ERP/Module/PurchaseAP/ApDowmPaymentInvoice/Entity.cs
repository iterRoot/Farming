using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using APDREntity = FarmingApi.Modules.PurchaseAP.APDownPaymentRequest.APDownPaymentRequest;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentInvoice : AuditableEntity
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

    // ── SAP-style base document linkage (from APDownPaymentRequest) ────────
    public int?    BaseEntry { get; set; }   // FK → APDownPaymentRequest.Id
    public string? BaseType  { get; set; }   // "APDownPaymentRequest"

    public APDREntity? BaseRequest { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentInvoiceConfig : IEntityTypeConfiguration<APDownPaymentInvoice>
{
    public void Configure(EntityTypeBuilder<APDownPaymentInvoice> builder)
    {
        builder.ToTable("KPDI");   // ✅ AP Down Payment Invoice header
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

        // ✅ Optional FK → APDownPaymentRequest (nullable)
        builder.HasOne(x => x.BaseRequest)
               .WithMany()
               .HasForeignKey(x => x.BaseEntry)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
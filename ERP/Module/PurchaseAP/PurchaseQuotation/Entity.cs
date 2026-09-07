using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using PBAEntity = FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement.PurchaseBlanketAgreement;

// using FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.PurchaseQuotation;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseQuotation : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";   // O=Open, C=Closed
    public string    Type        { get; set; } = "Item"; // Item | Service
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    // ── FK → BusinessPartner (Vendor) ─────────────────────────────────────
    public int      VendorId { get; set; }
    public BPEntity Vendor   { get; set; } = null!;

    // ── Lines ─────────────────────────────────────────────────────────────
    public ICollection<PurchaseQuotationLine> Items { get; set; } = new List<PurchaseQuotationLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseQuotationLine : AuditableEntity
{
    public int              PurchaseQuotationId { get; set; }
    public PurchaseQuotation PurchaseQuotation  { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // ── SAP-style base document linkage ───────────────────────────────────
    public int?    BaseEntry { get; set; }   // FK → PurchaseBlanketAgreement.Id
    public string? BaseType  { get; set; }   // "PurchaseBlanketAgreement"

    public PBAEntity? BaseBlanketAgreement { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseQuotationConfig : IEntityTypeConfiguration<PurchaseQuotation>
{
    public void Configure(EntityTypeBuilder<PurchaseQuotation> builder)
    {
        builder.ToTable("KPPQ");   // ✅ Purchase Quotation header
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();

        // Document numbers come from the reserved series, so a collision
        // means a bug in reservation — fail loudly rather than silently
        // storing two documents under one number.
        builder.HasIndex(x => x.DocNum).IsUnique();
        builder.Property(x => x.Status).HasMaxLength(10);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.Property(x => x.Discount).HasPrecision(18, 2);
        builder.Property(x => x.Tax).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
               .WithOne(x => x.PurchaseQuotation)
               .HasForeignKey(x => x.PurchaseQuotationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseQuotationLineConfig : IEntityTypeConfiguration<PurchaseQuotationLine>
{
    public void Configure(EntityTypeBuilder<PurchaseQuotationLine> builder)
    {
        builder.ToTable("PPQ1");   // ✅ Purchase Quotation lines
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

        // ✅ Optional FK → PurchaseBlanketAgreement (nullable)
        builder.HasOne(x => x.BaseBlanketAgreement)
               .WithMany()
               .HasForeignKey(x => x.BaseEntry)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
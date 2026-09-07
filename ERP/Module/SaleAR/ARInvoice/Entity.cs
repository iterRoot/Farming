using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;

// ✅ Alias to avoid namespace/class name conflict
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class ARInvoice : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";    // O=Open, C=Closed
    public string    Type        { get; set; } = "Item"; // Item | Service
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    // ── Logistics tab ───────────────────────────────────────────────────────
    public string?   ShipToAddress      { get; set; }
    public string?   BillToAddress      { get; set; }
    public string?   ShippingType       { get; set; }
    public string?   PickAndPackRemarks { get; set; }
    public bool      Approved           { get; set; }

    // ── Accounting tab ──────────────────────────────────────────────────────
    public string?   PaymentTerms          { get; set; }
    public string?   PaymentMethod         { get; set; }
    public int?      CashDiscountDateOffset { get; set; }
    public string?   BPProject             { get; set; }
    public string?   ControlAccount        { get; set; }
    public string?   JournalRemark         { get; set; }

    // ── FK → BusinessPartner ───────────────────────────────────────────────
    public int      CustomerId { get; set; }
    public BPEntity Customer   { get; set; } = null!;  // ✅ alias

    // ── Lines (Contents tab) ────────────────────────────────────────────────
    public ICollection<ARInvoiceLine> Items { get; set; } = new List<ARInvoiceLine>();

    // ── Attachments tab ─────────────────────────────────────────────────────
    public ICollection<ARInvoiceAttachment> Attachments { get; set; } = new List<ARInvoiceAttachment>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class ARInvoiceLine : AuditableEntity
{
    public int       ARInvoiceId { get; set; }
    public ARInvoice ARInvoice   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public string?  WhsCode  { get; set; }   // warehouse the stock is issued from
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // Source document this line was copied from. A line based on a Delivery has
    // already had its stock issued there, so the invoice must not post it again.
    public int?     BaseEntry { get; set; }
    public string?  BaseType  { get; set; }  // "Delivery" | "SaleOrder" | "SaleQuotation"
}

// ══════════════════════════════════════════════════════════════════════════════
// ATTACHMENT (Attachments tab)
// ══════════════════════════════════════════════════════════════════════════════
public class ARInvoiceAttachment : AuditableEntity
{
    public int       ARInvoiceId { get; set; }
    public ARInvoice ARInvoice   { get; set; } = null!;

    public string   FileName     { get; set; } = "";  // original file name
    public string   FilePath     { get; set; } = "";  // stored path on disk
    public long     FileSize     { get; set; }        // bytes
    public string?  ContentType  { get; set; }         // MIME type
    public DateTime UploadedDate { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class ARInvoiceConfig : IEntityTypeConfiguration<ARInvoice>
{
    public void Configure(EntityTypeBuilder<ARInvoice> builder)
    {
        builder.ToTable("KARI");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Status).HasMaxLength(10);
        builder.Property(m => m.Type).HasMaxLength(50);
        builder.Property(m => m.Remarks).HasMaxLength(500);
        builder.Property(m => m.Discount).HasPrecision(18, 2);
        builder.Property(m => m.Tax).HasPrecision(18, 2);
        builder.Property(m => m.TaxAmount).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        // ── Logistics tab ─────────────────────────────────────────────────
        builder.Property(m => m.ShipToAddress).HasMaxLength(300);
        builder.Property(m => m.BillToAddress).HasMaxLength(300);
        builder.Property(m => m.ShippingType).HasMaxLength(50);
        builder.Property(m => m.PickAndPackRemarks).HasMaxLength(500);

        // ── Accounting tab ────────────────────────────────────────────────
        builder.Property(m => m.PaymentTerms).HasMaxLength(50);
        builder.Property(m => m.PaymentMethod).HasMaxLength(50);
        builder.Property(m => m.BPProject).HasMaxLength(50);
        builder.Property(m => m.ControlAccount).HasMaxLength(50);
        builder.Property(m => m.JournalRemark).HasMaxLength(500);

        builder.HasOne(m => m.Customer)
               .WithMany()
               .HasForeignKey(m => m.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Items)
               .WithOne(l => l.ARInvoice)
               .HasForeignKey(l => l.ARInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Attachments)
               .WithOne(a => a.ARInvoice)
               .HasForeignKey(a => a.ARInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ARInvoiceAttachmentConfig : IEntityTypeConfiguration<ARInvoiceAttachment>
{
    public void Configure(EntityTypeBuilder<ARInvoiceAttachment> builder)
    {
        builder.ToTable("ARI2");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.FileName).HasMaxLength(260).IsRequired();
        builder.Property(m => m.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(m => m.ContentType).HasMaxLength(150);
    }
}

public class ARInvoiceLineConfig : IEntityTypeConfiguration<ARInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ARInvoiceLine> builder)
    {
        builder.ToTable("ARI1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(100);
        builder.Property(m => m.ItemName).HasMaxLength(200);
        builder.Property(m => m.WhsCode).HasMaxLength(50);
        builder.Property(m => m.BaseType).HasMaxLength(50);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        builder.HasOne(m => m.Item)
               .WithMany()
               .HasForeignKey(m => m.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
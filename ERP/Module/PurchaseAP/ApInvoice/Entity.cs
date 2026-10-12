using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class APInvoice : AuditableEntity
{
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string? BuyerCode { get; set; }   // buyer (KSEB code)

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
    public ICollection<APInvoiceLine> Items { get; set; } = new List<APInvoiceLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class APInvoiceLine : AuditableEntity
{
    // Batch / serial numbers entered for this line — used when the document posts stock; not stored on the line.
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchLine>? Batches { get; set; }

    public int          APInvoiceId { get; set; }
    public APInvoice    APInvoice   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public string?  WhsCode  { get; set; }   // warehouse the goods are received into
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // ── SAP-style base document linkage — polymorphic, like ARInvoice:
    // BaseType names which table BaseEntry points into ("GoodsReceiptPO" |
    // "PurchaseBlanketAgreement" | "PurchaseOrder" | "PurchaseQuotation" |
    // "Delivery"), resolved in code rather than a single hard-coded DB
    // foreign key, since the Create page can copy from any of them.
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class APInvoiceConfig : IEntityTypeConfiguration<APInvoice>
{
    public void Configure(EntityTypeBuilder<APInvoice> builder)
    {
        builder.ToTable("KPAI");   // ✅ AP Invoice header
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
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
               .WithOne(x => x.APInvoice)
               .HasForeignKey(x => x.APInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class APInvoiceLineConfig : IEntityTypeConfiguration<APInvoiceLine>
{
    public void Configure(EntityTypeBuilder<APInvoiceLine> builder)
    {
        builder.ToTable("PAI1");   // ✅ AP Invoice lines
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).HasMaxLength(100);
        builder.Property(x => x.ItemName).HasMaxLength(200);
        builder.Property(x => x.WhsCode).HasMaxLength(50);
        builder.Property(x => x.BaseType).HasMaxLength(100);
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Item)
               .WithMany()
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity    = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APCreditNote;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class APCreditNote : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public string    Type        { get; set; } = "Item"; // Item | Service
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public string?   BuyerCode   { get; set; }   // buyer (KSEB code)

    public int      VendorId { get; set; }
    public BPEntity Vendor   { get; set; } = null!;

    public ICollection<APCreditNoteLine> Items { get; set; } = new List<APCreditNoteLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class APCreditNoteLine : AuditableEntity
{
    public int           APCreditNoteId { get; set; }
    public APCreditNote  APCreditNote   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // ── SAP base doc — polymorphic, like ARCreditNote: BaseType names which
    // table BaseEntry points into ("APInvoice" | "GoodsReturn" |
    // "GoodsReturnRequest" | "APDownPaymentInvoice"), resolved in code rather
    // than a single hard-coded DB foreign key, since more than one table can
    // be the source of a copy.
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class APCreditNoteConfig : IEntityTypeConfiguration<APCreditNote>
{
    public void Configure(EntityTypeBuilder<APCreditNote> builder)
    {
        builder.ToTable("KPAC");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(10);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.Property(x => x.BuyerCode).HasMaxLength(50);
        builder.Property(x => x.Discount).HasPrecision(18, 2);
        builder.Property(x => x.Tax).HasPrecision(18, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
               .WithOne(x => x.APCreditNote)
               .HasForeignKey(x => x.APCreditNoteId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class APCreditNoteLineConfig : IEntityTypeConfiguration<APCreditNoteLine>
{
    public void Configure(EntityTypeBuilder<APCreditNoteLine> builder)
    {
        builder.ToTable("PAC1");
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
    }
}
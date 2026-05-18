using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using APINVEntity = FarmingApi.Modules.PurchaseAP.APInvoice.APInvoice;
using BPEntity    = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.APReserveInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class APReserveInvoice : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    public int      VendorId { get; set; }
    public BPEntity Vendor   { get; set; } = null!;

    public ICollection<APReserveInvoiceLine> Items { get; set; } = new List<APReserveInvoiceLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class APReserveInvoiceLine : AuditableEntity
{
    public int              APReserveInvoiceId { get; set; }
    public APReserveInvoice APReserveInvoice   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }

    // ── SAP base doc (from APInvoice) ────────────────────────────────────
    public int?    BaseEntry { get; set; }   // FK → APInvoice.Id
    public string? BaseType  { get; set; }   // "APInvoice"

    public APINVEntity? BaseInvoice { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class APReserveInvoiceConfig : IEntityTypeConfiguration<APReserveInvoice>
{
    public void Configure(EntityTypeBuilder<APReserveInvoice> builder)
    {
        builder.ToTable("KPAR");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(10);
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
               .WithOne(x => x.APReserveInvoice)
               .HasForeignKey(x => x.APReserveInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class APReserveInvoiceLineConfig : IEntityTypeConfiguration<APReserveInvoiceLine>
{
    public void Configure(EntityTypeBuilder<APReserveInvoiceLine> builder)
    {
        builder.ToTable("PAR1");
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

        builder.HasOne(x => x.BaseInvoice)
               .WithMany()
               .HasForeignKey(x => x.BaseEntry)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.SaleAR.SaleOrder;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class SaleOrder : AuditableEntity
{
    public string    DocNum       { get; set; } = "";
    public DateTime? PostingDate  { get; set; }
    public DateTime? DeliveryDate { get; set; }  // ✅ DeliveryDate (not DueDate)
    public string    Status       { get; set; } = "O";  // O=Open, C=Closed, D=Draft
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }

    // ── FK → BusinessPartnersMaster ───────────────────────────────────────────────
    public int             CustomerId { get; set; }
    public BusinessPartnersMaster Customer   { get; set; } = null!;

    // ── Lines ──────────────────────────────────────────────────────────────
    public ICollection<SaleOrderLine> Items { get; set; } = new List<SaleOrderLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class SaleOrderLine : AuditableEntity
{
    // ── FK → SaleOrder ─────────────────────────────────────────────────────
    public int       SaleOrderId { get; set; }
    public SaleOrder SaleOrder   { get; set; } = null!;

    // ── FK → ItemsMaster ───────────────────────────────────────────────────
    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    // ── Snapshot (saved at order time) ─────────────────────────────────────
    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class SaleOrderConfig : IEntityTypeConfiguration<SaleOrder>
{
    public void Configure(EntityTypeBuilder<SaleOrder> builder)
    {
        builder.ToTable("KASO");     // ✅ correct table name
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Status).HasMaxLength(10);
        builder.Property(m => m.Remarks).HasMaxLength(500);
        builder.Property(m => m.Discount).HasPrecision(18, 2);
        builder.Property(m => m.Tax).HasPrecision(18, 2);
        builder.Property(m => m.TaxAmount).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        // SaleOrder → BusinessPartnersMaster
        builder.HasOne(m => m.Customer)
               .WithMany()
               .HasForeignKey(m => m.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        // SaleOrder → Lines
        builder.HasMany(m => m.Items)
               .WithOne(l => l.SaleOrder)
               .HasForeignKey(l => l.SaleOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleOrderLineConfig : IEntityTypeConfiguration<SaleOrderLine>
{
    public void Configure(EntityTypeBuilder<SaleOrderLine> builder)
    {
        builder.ToTable("ASO1");  // ✅ correct table name
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(100);
        builder.Property(m => m.ItemName).HasMaxLength(200);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        // Line → ItemsMaster
        builder.HasOne(m => m.Item)
               .WithMany()
               .HasForeignKey(m => m.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
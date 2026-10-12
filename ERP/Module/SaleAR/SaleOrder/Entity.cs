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
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string? SalesEmployeeCode { get; set; }   // sales employee (KSEB code)

    public string    DocNum       { get; set; } = "";
    public DateTime? PostingDate  { get; set; }
    public DateTime? DeliveryDate { get; set; }  // ✅ DeliveryDate (not DueDate)
    public string    Status       { get; set; } = "O";  // O=Open, C=Closed, D=Draft
    public string    Type         { get; set; } = "Item"; // Item | Service
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
    // Document this line was copied from (Copy From) — quantities drawn from it are tracked.
    public int?    BaseEntry { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(50)]
    public string? BaseType  { get; set; }

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
    public string?  WhsCode      { get; set; }   // warehouse this line affects
    public string?  Branch       { get; set; }
    public decimal  Discount     { get; set; }   // line-level discount percent
    public decimal  DiscountSum  { get; set; }
    public decimal  Vat          { get; set; }   // line-level VAT percent
    public decimal  VatSum       { get; set; }
    public decimal  ItemCost     { get; set; }
    public string?  UomName      { get; set; }
    public string?  UomCode      { get; set; }
    public string?  DocCurrency  { get; set; }
    public decimal  GrossProfit  { get; set; }
    public string?  Dimension1   { get; set; }
    public string?  Dimension2   { get; set; }
    public string?  Dimension3   { get; set; }
    public string?  Dimension4   { get; set; }
    public string?  Dimension5   { get; set; }
    public string?  FreeText     { get; set; }
    public string?  Remark       { get; set; }
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
        builder.Property(m => m.Type).HasMaxLength(50);
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
        builder.Property(m => m.WhsCode).HasMaxLength(50);
        builder.Property(m => m.Branch).HasMaxLength(50);
        builder.Property(m => m.Discount).HasPrecision(18, 2);
        builder.Property(m => m.DiscountSum).HasPrecision(18, 2);
        builder.Property(m => m.Vat).HasPrecision(18, 2);
        builder.Property(m => m.VatSum).HasPrecision(18, 2);
        builder.Property(m => m.ItemCost).HasPrecision(18, 2);
        builder.Property(m => m.UomName).HasMaxLength(50);
        builder.Property(m => m.UomCode).HasMaxLength(50);
        builder.Property(m => m.DocCurrency).HasMaxLength(10);
        builder.Property(m => m.GrossProfit).HasPrecision(18, 2);
        builder.Property(m => m.Dimension1).HasMaxLength(50);
        builder.Property(m => m.Dimension2).HasMaxLength(50);
        builder.Property(m => m.Dimension3).HasMaxLength(50);
        builder.Property(m => m.Dimension4).HasMaxLength(50);
        builder.Property(m => m.Dimension5).HasMaxLength(50);
        builder.Property(m => m.FreeText).HasMaxLength(500);
        builder.Property(m => m.Remark).HasMaxLength(500);

        builder.HasOne(m => m.Item)
               .WithMany()
               .HasForeignKey(m => m.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
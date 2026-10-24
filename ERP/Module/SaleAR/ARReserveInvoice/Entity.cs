using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ARReserveInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class ARReserveInvoice : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";    // O=Open, C=Closed
    public string    Type        { get; set; } = "Item";
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public string?   SalesEmployeeCode { get; set; }   // sales employee (KSEB code)

    // ── FK → BusinessPartner ──────────────────────────────────────────────
    public int      CustomerId { get; set; }
    public BPEntity Customer   { get; set; } = null!;

    // ── Lines ─────────────────────────────────────────────────────────────
    public ICollection<ARReserveInvoiceLine> Items { get; set; } = new List<ARReserveInvoiceLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE  (unique class name)
// ══════════════════════════════════════════════════════════════════════════════
public class ARReserveInvoiceLine : AuditableEntity
{
    public int              ARReserveInvoiceId { get; set; }   // ✅ unique FK column
    public ARReserveInvoice ARReserveInvoice   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

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
public class ARReserveInvoiceConfig : IEntityTypeConfiguration<ARReserveInvoice>
{
    public void Configure(EntityTypeBuilder<ARReserveInvoice> builder)
    {
        builder.ToTable("KARV");   // ✅ own table
        builder.Property(x => x.SalesEmployeeCode).HasMaxLength(50);
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Status).HasMaxLength(10);
        builder.Property(m => m.Type).HasMaxLength(50);
        builder.Property(m => m.Remarks).HasMaxLength(500);
        builder.Property(m => m.Discount).HasPrecision(18, 2);
        builder.Property(m => m.Tax).HasPrecision(18, 2);
        builder.Property(m => m.TaxAmount).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        builder.HasOne(m => m.Customer)
               .WithMany()
               .HasForeignKey(m => m.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Items)
               .WithOne(l => l.ARReserveInvoice)
               .HasForeignKey(l => l.ARReserveInvoiceId)   // ✅ unique FK
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ARReserveInvoiceLineConfig : IEntityTypeConfiguration<ARReserveInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ARReserveInvoiceLine> builder)
    {
        builder.ToTable("ARV1");  // ✅ own table
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(100);
        builder.Property(m => m.ItemName).HasMaxLength(200);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

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
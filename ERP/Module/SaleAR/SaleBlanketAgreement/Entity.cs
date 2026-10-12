using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;
using ARInvoiceEntity = FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice;

namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

public class SaleBlanketAgreement : AuditableEntity
{
    public string    DocNum       { get; set; } = "";
    public DateTime? PostingDate  { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string    Status       { get; set; } = "O";
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }

    public int                    CustomerId { get; set; }
    public BusinessPartnersMaster Customer   { get; set; } = null!;

    public ICollection<SaleBlanketAgreementLine> Items { get; set; } = new List<SaleBlanketAgreementLine>();
}

public class SaleBlanketAgreementLine : AuditableEntity
{
    public int                  SaleBlanketAgreementId { get; set; }
    public SaleBlanketAgreement SaleBlanketAgreement   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public string?  WhsCode  { get; set; }   // warehouse the stock is issued from
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }
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

    // Which AR Invoice this line was drawn down/billed under, once invoiced.
    // Null until the customer is actually billed against this commitment.
    public int?             ARInvoiceId { get; set; }
    public ARInvoiceEntity? ARInvoice   { get; set; }

    // Source document this line was copied from (e.g. a Sale Quotation).
    public int?     BaseEntry { get; set; }
    public string?  BaseType  { get; set; }
}

public class SaleBlanketAgreementConfig : IEntityTypeConfiguration<SaleBlanketAgreement>
{
    public void Configure(EntityTypeBuilder<SaleBlanketAgreement> builder)
    {
        builder.ToTable("KABA");   // ✅ own table (was "sale_orders" — conflict!)
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Status).HasMaxLength(10);
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
               .WithOne(l => l.SaleBlanketAgreement)
               .HasForeignKey(l => l.SaleBlanketAgreementId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleBlanketAgreementLineConfig : IEntityTypeConfiguration<SaleBlanketAgreementLine>
{
    public void Configure(EntityTypeBuilder<SaleBlanketAgreementLine> builder)
    {
        builder.ToTable("ABA1");   // ✅ own table (was "sale_order_lines" — conflict!)
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(100);
        builder.Property(m => m.ItemName).HasMaxLength(200);
        builder.Property(m => m.WhsCode).HasMaxLength(50);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);
        builder.Property(m => m.BaseType).HasMaxLength(50);

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

        // Optional — only set once this line has actually been drawn down
        // into a real AR Invoice. Restrict so an invoice can't be deleted
        // out from under a line that still points at it.
        builder.HasOne(m => m.ARInvoice)
               .WithMany()
               .HasForeignKey(m => m.ARInvoiceId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
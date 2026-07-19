using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.SaleAR.SaleQuotion;

public class 
SaleQuotion : AuditableEntity
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

    public ICollection<SaleQuotionLine> Items { get; set; } = new List<SaleQuotionLine>();
}

public class SaleQuotionLine : AuditableEntity
{
    public int         SaleQuotionId { get; set; }
    public SaleQuotion SaleQuotion   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }
}

public class SaleQuotionConfig : IEntityTypeConfiguration<SaleQuotion>
{
    public void Configure(EntityTypeBuilder<SaleQuotion> builder)
    {
        builder.ToTable("KASQ");   // ✅ Sale Quotation header (was "sale_orders" — conflict!)
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
               .WithOne(l => l.SaleQuotion)
               .HasForeignKey(l => l.SaleQuotionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SaleQuotionLineConfig : IEntityTypeConfiguration<SaleQuotionLine>
{
    public void Configure(EntityTypeBuilder<SaleQuotionLine> builder)
    {
        builder.ToTable("ASQ1");   // ✅ Sale Quotation lines (was "sale_order_lines" — conflict!)
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(100);
        builder.Property(m => m.ItemName).HasMaxLength(200);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        builder.HasOne(m => m.Item)
               .WithMany()
               .HasForeignKey(m => m.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
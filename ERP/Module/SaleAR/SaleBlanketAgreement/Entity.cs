using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;

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
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }
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
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        builder.HasOne(m => m.Item)
               .WithMany()
               .HasForeignKey(m => m.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
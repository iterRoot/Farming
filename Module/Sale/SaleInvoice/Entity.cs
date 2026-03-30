using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.ItemsMaster;

namespace FarmingApi.Modules.Sale.SaleInvoice;

public class SaleInvoice : AuditableEntity
{
    public string ItemsCode { get; set; } = null!;
    public string? ItemsName { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal DisPct { get; set; }
    public decimal DisSum { get; set; }

    public decimal PriceBfDis { get; set; }
    public decimal PriceAtDis { get; set; }
    public decimal PriceAtVat { get; set; }

    public decimal Quantity { get; set; }
    public decimal InvQty { get; set; }

    public decimal LineTotalLC { get; set; }
    public decimal LineTotalFC { get; set; }
    public decimal LineTotalSys { get; set; }

    public decimal TotalLC { get; set; }
    public decimal TotalFC { get; set; }
    public decimal TotalSys { get; set; }

    public decimal LineStatus { get; set; }
    public decimal ItemsCost { get; set; }
    public decimal GTotal { get; set; }

    public decimal NetPrice { get; set; }
    public decimal GrossPrice { get; set; }

    public int BaseEntry { get; set; }

    public string? UomName { get; set; }
    public string? UomCode { get; set; }

    public string? Price { get; set; }
    public string? PriceList { get; set; }

    public string? Types { get; set; }
    public string? FreeTxt { get; set; }
    public string? Remarks { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Active;
}

public class SaleInvoiceConfig : IEntityTypeConfiguration<SaleInvoice>
{
    public void Configure(EntityTypeBuilder<SaleInvoice> builder)
    {
        builder.ToTable("sale_invoices");

        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemsCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.ItemsName)
            .HasMaxLength(200);

        builder.Property(m => m.UomName)
            .HasMaxLength(255);

        builder.Property(m => m.UomCode)
            .HasMaxLength(200);

        builder.Property(m => m.Price)
            .HasMaxLength(50);

        builder.Property(m => m.PriceList)
            .HasMaxLength(200);

        builder.Property(m => m.Types)
            .HasMaxLength(200);

        builder.Property(m => m.FreeTxt)
            .HasMaxLength(500);

        builder.Property(m => m.Remarks)
            .HasMaxLength(500);

        // Decimal precision
        builder.Property(m => m.UnitPrice).HasPrecision(18,2);
        builder.Property(m => m.DisPct).HasPrecision(18,2);
        builder.Property(m => m.DisSum).HasPrecision(18,2);
        builder.Property(m => m.Quantity).HasPrecision(18,2);
        builder.Property(m => m.LineTotalLC).HasPrecision(18,2);
        builder.Property(m => m.TotalLC).HasPrecision(18,2);
        builder.Property(m => m.GTotal).HasPrecision(18,2);

        // Enum stored as int
        builder.Property(m => m.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}
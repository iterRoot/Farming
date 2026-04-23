using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.BusinessPartner;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

// ─────────────────────────────────────────
// ENUMS
// ─────────────────────────────────────────
public enum InvoiceStatus { Open, Closed, Cancelled }
public enum LineItemType { Item, Service }

// ─────────────────────────────────────────
// HEADER
// ─────────────────────────────────────────
public class ARInvoice : AuditableEntity
{
    // Document Identity
    public string DocSeries { get; set; } = "IN2600";
    public string DocNumber { get; set; } = null!;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

    // ── Business Partner ──────────────────
    public int BusinessPartnerId { get; set; }              // FK
    public BusinessPartner BusinessPartner { get; set; } = null!;
    public string BPName { get; set; } = string.Empty;      // snapshot
    public string? ContactPerson { get; set; }
    public string? CustomerRefNo { get; set; }

    // ── Dates ─────────────────────────────
    public DateTime PostingDate { get; set; } = DateTime.Today;
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    // ── Currency & Branch ─────────────────
    public string Currency { get; set; } = "USD";
    public string? BranchCode { get; set; }

    // ── Logistics ─────────────────────────
    public string? ShipTo { get; set; }
    public string? ShipFrom { get; set; }
    public string? ShippingType { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentMethod { get; set; }

    // ── Totals ────────────────────────────
    public decimal DiscountPct { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TotalBeforeDiscount { get; set; } = 0;
    public decimal TaxTotal { get; set; } = 0;
    public decimal Rounding { get; set; } = 0;
    public decimal DownPayment { get; set; } = 0;
    public decimal GrandTotal { get; set; } = 0;
    public decimal AppliedAmount { get; set; } = 0;
    public decimal BalanceDue { get; set; } = 0;

    // ── Accounting ────────────────────────
    public string? ArAccount { get; set; }
    public string? RevenueAccount { get; set; }
    public string? TaxAccount { get; set; }
    public string? CostCenter { get; set; }
    public string? ProjectCode { get; set; }

    // ── Misc ──────────────────────────────
    public string? SalesEmployee { get; set; }
    public string? Owner { get; set; }
    public string? Remarks { get; set; }
    public bool PaymentOrderRun { get; set; } = false;

    // ── Lines ─────────────────────────────
    public ICollection<ARInvoiceLine> Lines { get; set; } = new List<ARInvoiceLine>();
}

// ─────────────────────────────────────────
// LINE ITEMS
// ─────────────────────────────────────────
public class ARInvoiceLine
{
    public int Id { get; set; }
    public int ARInvoiceId { get; set; }                    // FK → ARInvoice
    public ARInvoice ARInvoice { get; set; } = null!;

    public int LineNum { get; set; }
    public LineItemType ItemType { get; set; } = LineItemType.Item;

    public string? ItemCode { get; set; }
    public string? BPCatalogueNo { get; set; }
    public string ItemDescription { get; set; } = string.Empty;

    public decimal Quantity { get; set; } = 1;
    public string? StockUoM { get; set; }
    public string? UoMCode { get; set; }

    public decimal UnitPrice { get; set; } = 0;
    public decimal DiscountPct { get; set; } = 0;

    public string? VatCode { get; set; }
    public decimal VatRate { get; set; } = 0;               // snapshot e.g. 0.10

    public decimal LineTotal { get; set; } = 0;             // qty × price × (1 - disc)
    public decimal TaxAmount { get; set; } = 0;             // lineTotal × vatRate

    public string? RevenueAccount { get; set; }
    public string? CogsDepartment { get; set; }
    public string? WarehouseCode { get; set; }
    public string? CountryRegion { get; set; }
}

// ─────────────────────────────────────────
// CONFIGS
// ─────────────────────────────────────────
public class ARInvoiceConfig : IEntityTypeConfiguration<ARInvoice>
{
    public void Configure(EntityTypeBuilder<ARInvoice> builder)
    {
        builder.ToTable("ARInvoice");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DocNumber).IsUnique();

        builder.Property(x => x.DocSeries).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DocNumber).HasMaxLength(30).IsRequired();
        builder.Property(x => x.BPName).HasMaxLength(100);
        builder.Property(x => x.ContactPerson).HasMaxLength(100);
        builder.Property(x => x.CustomerRefNo).HasMaxLength(50);
        builder.Property(x => x.Currency).HasMaxLength(10);
        builder.Property(x => x.BranchCode).HasMaxLength(20);
        builder.Property(x => x.ShippingType).HasMaxLength(50);
        builder.Property(x => x.PaymentTerms).HasMaxLength(50);
        builder.Property(x => x.PaymentMethod).HasMaxLength(50);
        builder.Property(x => x.ArAccount).HasMaxLength(20);
        builder.Property(x => x.RevenueAccount).HasMaxLength(20);
        builder.Property(x => x.TaxAccount).HasMaxLength(20);
        builder.Property(x => x.CostCenter).HasMaxLength(50);
        builder.Property(x => x.ProjectCode).HasMaxLength(50);
        builder.Property(x => x.SalesEmployee).HasMaxLength(100);
        builder.Property(x => x.Owner).HasMaxLength(100);

        // all decimals — never use HasMaxLength on decimal!
        builder.Property(x => x.DiscountPct).HasColumnType("decimal(5,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalBeforeDiscount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Rounding).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DownPayment).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrandTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.AppliedAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.BalanceDue).HasColumnType("decimal(18,2)");

        // Status enum → string
        builder.Property(x => x.Status)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<InvoiceStatus>(v)
            ).HasMaxLength(20);

        // Relationship → BusinessPartner
        builder.HasOne(x => x.BusinessPartner)
               .WithMany()
               .HasForeignKey(x => x.BusinessPartnerId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relationship → Lines
        builder.HasMany(x => x.Lines)
               .WithOne(x => x.ARInvoice)
               .HasForeignKey(x => x.ARInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ARInvoiceLineConfig : IEntityTypeConfiguration<ARInvoiceLine>
{
    public void Configure(EntityTypeBuilder<ARInvoiceLine> builder)
    {
        builder.ToTable("ARInvoiceLine");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).HasMaxLength(50);
        builder.Property(x => x.BPCatalogueNo).HasMaxLength(50);
        builder.Property(x => x.ItemDescription).HasMaxLength(200);
        builder.Property(x => x.StockUoM).HasMaxLength(20);
        builder.Property(x => x.UoMCode).HasMaxLength(20);
        builder.Property(x => x.VatCode).HasMaxLength(10);
        builder.Property(x => x.RevenueAccount).HasMaxLength(20);
        builder.Property(x => x.CogsDepartment).HasMaxLength(50);
        builder.Property(x => x.WarehouseCode).HasMaxLength(20);
        builder.Property(x => x.CountryRegion).HasMaxLength(100);

        builder.Property(x => x.Quantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,4)");
        builder.Property(x => x.DiscountPct).HasColumnType("decimal(5,2)");
        builder.Property(x => x.VatRate).HasColumnType("decimal(5,4)");
        builder.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");

        builder.Property(x => x.ItemType)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<LineItemType>(v)
            ).HasMaxLength(10);

        builder.HasOne(x => x.ARInvoice)
               .WithMany(x => x.Lines)
               .HasForeignKey(x => x.ARInvoiceId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
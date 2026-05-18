using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.OutgoingPayment;

// ═══════════════════════════════════════════════════════════════════
// OUTGOING PAYMENT HEADER (KPYO)
// Payment to Suppliers (AP)
// ═══════════════════════════════════════════════════════════════════
public class OutgoingPayment : AuditableEntity
{
    public string DocNum { get; set; } = null!;              // Auto: PAY-OUT-2026-00001
    public DateTime DocDate { get; set; }
    public DateTime DueDate { get; set; }
    public string CardCode { get; set; } = null!;            // Supplier Code
    public string CardName { get; set; }                     // Supplier Name
    public decimal DocTotal { get; set; }                    // Total Payment Amount
    public decimal AppliedAmount { get; set; }               // Amount applied to AP invoices
    public decimal UnappliedAmount { get; set; }             // Advance payment to supplier
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public string PaymentMethod { get; set; } = "Cash";      // Cash, Check, Transfer, Card
    public string CheckNumber { get; set; }                  // Check number if applicable
    public DateTime? CheckDate { get; set; }
    public string BankAccount { get; set; }                  // Bank account code
    public string BankName { get; set; }
    public string Reference { get; set; }
    public string Memo { get; set; }
    public string Status { get; set; } = "O";                // O=Open, C=Closed, V=Void
    public int? JournalEntryId { get; set; }                 // Auto-created JE
    public string TransType { get; set; } = "KPYO";
    public int? UserSign { get; set; }
    public int? UserSign2 { get; set; }
    public int VersionNum { get; set; } = 1;

    // Navigation
    public ICollection<OutgoingPaymentInvoice> Invoices { get; set; } = new List<OutgoingPaymentInvoice>();
}

// ═══════════════════════════════════════════════════════════════════
// OUTGOING PAYMENT INVOICE ALLOCATION (PYO1)
// Tracks which AP invoices this payment is applied to
// ═══════════════════════════════════════════════════════════════════
public class OutgoingPaymentInvoice : AuditableEntity
{
    public int OutgoingPaymentId { get; set; }
    public int LineNum { get; set; }
    public int InvoiceId { get; set; }                       // FK to AP Invoice (KPAI)
    public string InvoiceDocNum { get; set; }                // Invoice Number (display)
    public DateTime InvoiceDocDate { get; set; }
    public DateTime InvoiceDueDate { get; set; }
    public decimal InvoiceTotal { get; set; }                // Original invoice amount
    public decimal InvoiceBalance { get; set; }              // Balance before this payment
    public decimal AppliedAmount { get; set; }               // Amount being paid on this invoice
    public decimal RemainingBalance { get; set; }            // Balance after this payment
    public string InvoiceStatus { get; set; }                // O=Open, C=Closed (paid)
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public decimal DiscountAmount { get; set; }              // Early payment discount
    public decimal WithholdingTax { get; set; }              // Tax withheld

    // Navigation
    public OutgoingPayment OutgoingPayment { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class OutgoingPaymentConfig : IEntityTypeConfiguration<OutgoingPayment>
{
    public void Configure(EntityTypeBuilder<OutgoingPayment> builder)
    {
        builder.ToTable("KPYO");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(50).IsRequired();
        builder.Property(m => m.CardCode).HasMaxLength(50).IsRequired();
        builder.Property(m => m.CardName).HasMaxLength(200);
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(m => m.PaymentMethod).HasMaxLength(20).HasDefaultValue("Cash");
        builder.Property(m => m.CheckNumber).HasMaxLength(50);
        builder.Property(m => m.BankAccount).HasMaxLength(50);
        builder.Property(m => m.BankName).HasMaxLength(100);
        builder.Property(m => m.Reference).HasMaxLength(100);
        builder.Property(m => m.Memo).HasMaxLength(500);
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("O");
        builder.Property(m => m.TransType).HasMaxLength(10).HasDefaultValue("KPYO");
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        builder.Property(m => m.DocTotal).HasPrecision(18, 2);
        builder.Property(m => m.AppliedAmount).HasPrecision(18, 2);
        builder.Property(m => m.UnappliedAmount).HasPrecision(18, 2);
        builder.Property(m => m.ExchangeRate).HasPrecision(18, 6);

        builder.HasIndex(m => m.DocNum).IsUnique();
        builder.HasIndex(m => m.DocDate);
        builder.HasIndex(m => m.CardCode);
        builder.HasIndex(m => m.Status);
    }
}

public class OutgoingPaymentInvoiceConfig : IEntityTypeConfiguration<OutgoingPaymentInvoice>
{
    public void Configure(EntityTypeBuilder<OutgoingPaymentInvoice> builder)
    {
        builder.ToTable("PYO1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.InvoiceDocNum).HasMaxLength(50).IsRequired();
        builder.Property(m => m.InvoiceStatus).HasMaxLength(1);
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");

        builder.Property(m => m.InvoiceTotal).HasPrecision(18, 2);
        builder.Property(m => m.InvoiceBalance).HasPrecision(18, 2);
        builder.Property(m => m.AppliedAmount).HasPrecision(18, 2);
        builder.Property(m => m.RemainingBalance).HasPrecision(18, 2);
        builder.Property(m => m.ExchangeRate).HasPrecision(18, 6);
        builder.Property(m => m.DiscountAmount).HasPrecision(18, 2);
        builder.Property(m => m.WithholdingTax).HasPrecision(18, 2);

        builder.HasOne(m => m.OutgoingPayment)
            .WithMany(p => p.Invoices)
            .HasForeignKey(m => m.OutgoingPaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.OutgoingPaymentId);
        builder.HasIndex(m => m.InvoiceId);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.CreditCard;

// ═══════════════════════════════════════════════════════════════════
// CREDIT CARD MANAGEMENT
//
// A card or card processor the company accepts (KBCS). Card receipts are
// incoming payments with method "Card" posted to the card's ClearingAccount;
// they sit there until the processor pays out. A settlement (KBCT / BCT1)
// records that payout:
//   Dr  Settlement bank account   net (gross − fee)
//   Dr  Fee account               fee
//   Cr  Clearing account          gross
// and marks the receipts settled. Void reverses it.
// ═══════════════════════════════════════════════════════════════════
public class CreditCardSetup : AuditableEntity
{
    public string  Code              { get; set; } = null!;   // e.g. VISA, ABA-PAYWAY
    public string  Name              { get; set; } = null!;
    public string  ClearingAccount   { get; set; } = null!;   // G/L account card receipts are posted to
    public string? SettlementAccount { get; set; }            // Bank account the processor pays into (default)
    public decimal FeePercent        { get; set; }            // Merchant fee, % of gross
    public string? FeeAccount        { get; set; }            // Expense account for the fee
    public bool    IsActive          { get; set; } = true;
}

public class CreditCardSettlement : AuditableEntity
{
    public string   DocNum         { get; set; } = null!;     // Auto: CS-2026-00001
    public string   CardCode       { get; set; } = null!;
    public DateTime PostingDate    { get; set; }
    public string   BankAccount    { get; set; } = null!;
    public string   ClearingAccount { get; set; } = null!;
    public decimal  GrossAmount    { get; set; }
    public decimal  FeeAmount      { get; set; }
    public decimal  NetAmount      { get; set; }
    public string?  FeeAccount     { get; set; }
    public string?  Reference      { get; set; }              // Processor payout / batch reference
    public string   Status         { get; set; } = "P";       // P=Posted, V=Void
    public int?     JournalEntryId { get; set; }
    public string?  JournalNo      { get; set; }

    public ICollection<CreditCardSettlementLine> Lines { get; set; } = new List<CreditCardSettlementLine>();
}

public class CreditCardSettlementLine : AuditableEntity
{
    public int     CreditCardSettlementId { get; set; }
    public int     IncomingPaymentId      { get; set; }
    public string  PaymentDocNum          { get; set; } = null!;
    public string? CustomerName           { get; set; }
    public decimal Amount                 { get; set; }

    public CreditCardSettlement CreditCardSettlement { get; set; } = null!;
}

public class CreditCardSetupConfig : IEntityTypeConfiguration<CreditCardSetup>
{
    public void Configure(EntityTypeBuilder<CreditCardSetup> b)
    {
        b.ToTable("KBCS");
        b.HasKey(x => x.Id);
        b.Property(m => m.Code).HasMaxLength(20).IsRequired();
        b.HasIndex(m => m.Code).IsUnique();
        b.Property(m => m.Name).HasMaxLength(100).IsRequired();
        b.Property(m => m.ClearingAccount).HasMaxLength(20).IsRequired();
        b.Property(m => m.SettlementAccount).HasMaxLength(20);
        b.Property(m => m.FeeAccount).HasMaxLength(20);
        b.Property(m => m.FeePercent).HasPrecision(9, 4);
        b.Property(m => m.IsActive).HasDefaultValue(true);
    }
}

public class CreditCardSettlementConfig : IEntityTypeConfiguration<CreditCardSettlement>
{
    public void Configure(EntityTypeBuilder<CreditCardSettlement> b)
    {
        b.ToTable("KBCT");
        b.HasKey(x => x.Id);
        b.Property(m => m.DocNum).HasMaxLength(50).IsRequired();
        b.HasIndex(m => m.DocNum).IsUnique();
        b.Property(m => m.CardCode).HasMaxLength(20).IsRequired();
        b.Property(m => m.BankAccount).HasMaxLength(20).IsRequired();
        b.Property(m => m.ClearingAccount).HasMaxLength(20).IsRequired();
        b.Property(m => m.FeeAccount).HasMaxLength(20);
        b.Property(m => m.Reference).HasMaxLength(100);
        b.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("P");
        b.Property(m => m.JournalNo).HasMaxLength(50);
        b.Property(m => m.GrossAmount).HasPrecision(18, 2);
        b.Property(m => m.FeeAmount).HasPrecision(18, 2);
        b.Property(m => m.NetAmount).HasPrecision(18, 2);
        b.HasIndex(m => m.CardCode);
        b.HasIndex(m => m.PostingDate);
    }
}

public class CreditCardSettlementLineConfig : IEntityTypeConfiguration<CreditCardSettlementLine>
{
    public void Configure(EntityTypeBuilder<CreditCardSettlementLine> b)
    {
        b.ToTable("BCT1");
        b.HasKey(x => x.Id);
        b.Property(m => m.PaymentDocNum).HasMaxLength(50).IsRequired();
        b.Property(m => m.CustomerName).HasMaxLength(200);
        b.Property(m => m.Amount).HasPrecision(18, 2);
        b.HasOne(m => m.CreditCardSettlement).WithMany(s => s.Lines)
            .HasForeignKey(m => m.CreditCardSettlementId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(m => m.IncomingPaymentId);
    }
}

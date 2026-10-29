using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.InternalTransfer;

// ═══════════════════════════════════════════════════════════════════
// INTERNAL TRANSFER (KBIT) — money moved between two of the company's own
// bank / cash G/L accounts (bank to bank, cash to bank, petty cash top-up).
//
// Saving posts the journal entry straight away:
//   Dr  To account      Amount
//   Cr  From account    Amount
// and, when the bank charged for the transfer:
//   Dr  Charge account  BankCharge
//   Cr  From account    BankCharge
// Void reverses it by voiding that journal entry.
// ═══════════════════════════════════════════════════════════════════
public class InternalTransfer : AuditableEntity
{
    public string   DocNum         { get; set; } = null!;   // Auto: IT-2026-00001
    public DateTime PostingDate    { get; set; }
    public string   FromAccount    { get; set; } = null!;
    public string   FromAccountName { get; set; } = string.Empty;
    public string   ToAccount      { get; set; } = null!;
    public string   ToAccountName  { get; set; } = string.Empty;
    public decimal  Amount         { get; set; }
    public decimal  BankCharge     { get; set; }
    public string?  ChargeAccount  { get; set; }
    public string   Currency       { get; set; } = "USD";
    public string?  Reference      { get; set; }             // Bank transfer / slip number
    public string?  Memo           { get; set; }
    public string   Status         { get; set; } = "P";      // P=Posted, V=Void
    public int?     JournalEntryId { get; set; }
    public string?  JournalNo      { get; set; }
    public int?     UserSign       { get; set; }
}

public class InternalTransferConfig : IEntityTypeConfiguration<InternalTransfer>
{
    public void Configure(EntityTypeBuilder<InternalTransfer> builder)
    {
        builder.ToTable("KBIT");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.DocNum).IsUnique();
        builder.Property(m => m.FromAccount).HasMaxLength(20).IsRequired();
        builder.Property(m => m.FromAccountName).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(m => m.ToAccount).HasMaxLength(20).IsRequired();
        builder.Property(m => m.ToAccountName).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(m => m.ChargeAccount).HasMaxLength(20);
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(m => m.Reference).HasMaxLength(100);
        builder.Property(m => m.Memo).HasMaxLength(500);
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("P");
        builder.Property(m => m.JournalNo).HasMaxLength(50);

        builder.Property(m => m.Amount).HasPrecision(18, 2);
        builder.Property(m => m.BankCharge).HasPrecision(18, 2);

        builder.HasIndex(m => m.PostingDate);
        builder.HasIndex(m => m.FromAccount);
        builder.HasIndex(m => m.ToAccount);
    }
}

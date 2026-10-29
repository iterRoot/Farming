using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.BankStatement;

// ═══════════════════════════════════════════════════════════════════
// BANK STATEMENT HEADER (KBST)
//
// PURPOSE: Record the statement the bank sends for one GL bank account
// and process each of its rows against the books.
//
// Flow:
//   1. Enter or paste the statement rows (opening/closing balance from the bank)
//   2. Match each row to the journal entry that already moved the bank
//      account (payment, deposit, manual JE) — or Auto Match
//   3. Rows with nothing in the books (bank fees, interest) are posted
//      as a new journal entry against an offset account
//   4. Close once every row is matched or posted and the rows add up
//      from the opening to the closing balance
// ═══════════════════════════════════════════════════════════════════
public class BankStatement : AuditableEntity
{
    public string   StatementNo    { get; set; } = null!;  // Auto: BS-2026-00001
    public string   AccountCode    { get; set; } = null!;  // GL bank account
    public string   AccountName    { get; set; } = string.Empty;
    public string?  BankName       { get; set; }
    public string?  BankAccountNo  { get; set; }           // Actual bank account number
    public string   Currency       { get; set; } = "USD";
    public DateTime StatementDate  { get; set; }
    public DateTime FromDate       { get; set; }           // Period the statement covers
    public DateTime ToDate         { get; set; }
    public decimal  OpeningBalance { get; set; }           // As printed on the statement
    public decimal  ClosingBalance { get; set; }           // As printed on the statement
    public string   Status         { get; set; } = "O";    // O=Open, C=Closed
    public string?  Remarks        { get; set; }
    public int?     UserSign       { get; set; }

    public ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();
}

// ═══════════════════════════════════════════════════════════════════
// BANK STATEMENT LINE (BST1) — one row of the statement
// ═══════════════════════════════════════════════════════════════════
public class BankStatementLine : AuditableEntity
{
    public int      BankStatementId { get; set; }
    public int      LineNum         { get; set; }
    public DateTime ValueDate       { get; set; }
    public string?  Reference       { get; set; }
    public string?  Description     { get; set; }
    public decimal  AmountIn        { get; set; }   // Money into the bank account
    public decimal  AmountOut       { get; set; }   // Money out of the bank account

    // U=Unmatched, M=Matched to an existing journal entry, P=Posted as a new journal entry
    public string   Status          { get; set; } = "U";
    public int?     JournalEntryId  { get; set; }   // The matched or posted journal entry
    public string?  JournalNo       { get; set; }
    public string?  Remarks         { get; set; }

    public BankStatement BankStatement { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class BankStatementConfig : IEntityTypeConfiguration<BankStatement>
{
    public void Configure(EntityTypeBuilder<BankStatement> builder)
    {
        builder.ToTable("KBST");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.StatementNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.StatementNo).IsUnique();
        builder.Property(m => m.AccountCode).HasMaxLength(20).IsRequired();
        builder.Property(m => m.AccountName).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(m => m.BankName).HasMaxLength(200);
        builder.Property(m => m.BankAccountNo).HasMaxLength(50);
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("O");
        builder.Property(m => m.Remarks).HasMaxLength(500);

        builder.Property(m => m.OpeningBalance).HasPrecision(18, 2);
        builder.Property(m => m.ClosingBalance).HasPrecision(18, 2);

        builder.HasIndex(m => m.AccountCode);
        builder.HasIndex(m => m.StatementDate);
        builder.HasIndex(m => m.Status);
    }
}

public class BankStatementLineConfig : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("BST1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.Reference).HasMaxLength(100);
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("U");
        builder.Property(m => m.JournalNo).HasMaxLength(50);
        builder.Property(m => m.Remarks).HasMaxLength(200);

        builder.Property(m => m.AmountIn).HasPrecision(18, 2);
        builder.Property(m => m.AmountOut).HasPrecision(18, 2);

        builder.HasOne(m => m.BankStatement)
            .WithMany(b => b.Lines)
            .HasForeignKey(m => m.BankStatementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.BankStatementId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.JournalEntryId);
    }
}

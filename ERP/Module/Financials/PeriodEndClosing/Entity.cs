using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.PeriodEndClosing;

// ═══════════════════════════════════════════════════════════════════
// PERIOD-END / YEAR-END CLOSING (KPEC / PEC1)
//
// At the end of a financial year the profit-and-loss accounts (Revenue and
// Expenses) are emptied into retained earnings, so the new year starts from
// zero and the profit for the year lands in equity:
//   Dr  each revenue account      its credit balance
//   Cr  each expense account      its debit balance
//   Cr  retained earnings         the profit  (or Dr for a loss)
//
// Each account closes to its own ClosingAcc when one is set on the chart of
// accounts, otherwise to the retained earnings account from GL Account
// Determination. A run can be previewed before it posts, and voided after.
// ═══════════════════════════════════════════════════════════════════
public class PeriodEndClosingRun : AuditableEntity
{
    public string   DocNum           { get; set; } = null!;   // Auto: PEC-2026-00001
    public DateTime FromDate         { get; set; }
    public DateTime ToDate           { get; set; }
    public DateTime PostingDate      { get; set; }            // Normally the last day of the year
    public string   RetainedEarnings { get; set; } = null!;
    public decimal  TotalRevenue     { get; set; }            // Credit-positive
    public decimal  TotalExpense     { get; set; }            // Debit-positive
    public decimal  NetProfit        { get; set; }            // Revenue − Expense
    public int      AccountCount     { get; set; }
    public string?  Remarks          { get; set; }
    public string   Status           { get; set; } = "P";     // P=Posted, V=Void
    public int?     JournalEntryId   { get; set; }
    public string?  JournalNo        { get; set; }
    public int?     UserSign         { get; set; }

    public ICollection<PeriodEndClosingLine> Lines { get; set; } = new List<PeriodEndClosingLine>();
}

// One line per P&L account closed by the run.
public class PeriodEndClosingLine : AuditableEntity
{
    public int     PeriodEndClosingRunId { get; set; }
    public string  AccountCode           { get; set; } = null!;
    public string  AccountName           { get; set; } = string.Empty;
    public string  AccountType           { get; set; } = null!;   // I=Revenue, X=Expense
    public decimal Balance               { get; set; }            // Debit − credit before closing
    public string  ClosedTo              { get; set; } = null!;

    public PeriodEndClosingRun PeriodEndClosingRun { get; set; } = null!;
}

public class PeriodEndClosingRunConfig : IEntityTypeConfiguration<PeriodEndClosingRun>
{
    public void Configure(EntityTypeBuilder<PeriodEndClosingRun> b)
    {
        b.ToTable("KPEC");
        b.HasKey(x => x.Id);
        b.Property(m => m.DocNum).HasMaxLength(50).IsRequired();
        b.HasIndex(m => m.DocNum).IsUnique();
        b.Property(m => m.RetainedEarnings).HasMaxLength(20).IsRequired();
        b.Property(m => m.Remarks).HasMaxLength(500);
        b.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("P");
        b.Property(m => m.JournalNo).HasMaxLength(50);
        b.Property(m => m.TotalRevenue).HasPrecision(18, 2);
        b.Property(m => m.TotalExpense).HasPrecision(18, 2);
        b.Property(m => m.NetProfit).HasPrecision(18, 2);
        b.HasIndex(m => m.ToDate);
        b.HasIndex(m => m.Status);
    }
}

public class PeriodEndClosingLineConfig : IEntityTypeConfiguration<PeriodEndClosingLine>
{
    public void Configure(EntityTypeBuilder<PeriodEndClosingLine> b)
    {
        b.ToTable("PEC1");
        b.HasKey(x => x.Id);
        b.Property(m => m.AccountCode).HasMaxLength(20).IsRequired();
        b.Property(m => m.AccountName).HasMaxLength(200).HasDefaultValue(string.Empty);
        b.Property(m => m.AccountType).HasMaxLength(1).IsRequired();
        b.Property(m => m.ClosedTo).HasMaxLength(20).IsRequired();
        b.Property(m => m.Balance).HasPrecision(18, 2);
        b.HasOne(m => m.PeriodEndClosingRun).WithMany(r => r.Lines)
            .HasForeignKey(m => m.PeriodEndClosingRunId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(m => m.PeriodEndClosingRunId);
    }
}

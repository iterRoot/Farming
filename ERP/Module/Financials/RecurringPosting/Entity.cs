using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.RecurringPosting;

// ══════════════════════════════════════════════════════════════════════════════
// RECURRING POSTING — a Journal Entry template that repeats on a schedule.
// Executing it generates a real Journal Entry and advances NextExecution.
// Table: ORCR (header) / RCR1 (template lines)
// ══════════════════════════════════════════════════════════════════════════════
public class RecurringPosting : AuditableEntity
{
    public string    Code          { get; set; } = "";      // auto: RCR-2026-00000001
    public string    Description   { get; set; } = "";
    public string    Frequency     { get; set; } = "Monthly"; // Daily|Weekly|Monthly|Quarterly|Annually
    public DateTime   StartDate     { get; set; }
    public DateTime   NextExecution { get; set; }             // when it fires next
    public DateTime?  ValidUntil    { get; set; }             // null = no end
    public string     Status        { get; set; } = "Active"; // Active|Suspended|Expired
    public string     Currency      { get; set; } = "USD";
    public string?    Reference     { get; set; }
    public string?    Memo          { get; set; }
    public int        TimesExecuted { get; set; }

    public ICollection<RecurringPostingLine> Lines { get; set; } = new List<RecurringPostingLine>();
}

public class RecurringPostingLine : AuditableEntity
{
    public int    RecurringPostingId { get; set; }
    public RecurringPosting RecurringPosting { get; set; } = null!;

    public int      LineNum     { get; set; }
    public string   AccountCode { get; set; } = "";
    public string   AccountName { get; set; } = "";
    public decimal  Debit       { get; set; }
    public decimal  Credit      { get; set; }
    public string?  LineMemo    { get; set; }
}

public class RecurringPostingConfig : IEntityTypeConfiguration<RecurringPosting>
{
    public void Configure(EntityTypeBuilder<RecurringPosting> b)
    {
        b.ToTable("ORCR");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(200).IsRequired();
        b.Property(x => x.Frequency).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.Reference).HasMaxLength(100);
        b.Property(x => x.Memo).HasMaxLength(500);

        b.HasMany(x => x.Lines)
         .WithOne(l => l.RecurringPosting)
         .HasForeignKey(l => l.RecurringPostingId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RecurringPostingLineConfig : IEntityTypeConfiguration<RecurringPostingLine>
{
    public void Configure(EntityTypeBuilder<RecurringPostingLine> b)
    {
        b.ToTable("RCR1");
        b.HasKey(x => x.Id);
        b.Property(x => x.AccountCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AccountName).HasMaxLength(200);
        b.Property(x => x.LineMemo).HasMaxLength(300);
        b.Property(x => x.Debit).HasPrecision(18, 2);
        b.Property(x => x.Credit).HasPrecision(18, 2);
    }
}

using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.FixedAssets.DepreciationRun;

// ═══════════════════════════════════════════════════════════════
// DEPRECIATION RUN  (KDPR / DPR1)  ·  SAP B1 equivalent: Depreciation Run
// One run per month. Posting it books one journal entry (Dr depreciation
// expense, Cr accumulated depreciation per asset) and moves each asset's
// AccumulatedDepreciation / RemainingLifeMonths forward. Reversing voids
// that entry and moves the assets back — only the latest posted run can
// be reversed, so asset figures always unwind in order.
// AssetId is a plain indexed int, not an EF FK, so a run never blocks
// deleting an asset (the asset controller refuses that itself once the
// asset has posted depreciation).
// ═══════════════════════════════════════════════════════════════
public class DepreciationRun : AuditableEntity
{
    public string   RunNo          { get; set; } = null!;   // "DEPR-2026-00001"
    public int      PeriodYear     { get; set; }
    public int      PeriodMonth    { get; set; }
    public DateTime PostingDate    { get; set; }
    public string   Status         { get; set; } = "Posted"; // Posted | Reversed
    public decimal  TotalAmount    { get; set; }
    public int?     JournalEntryId { get; set; }
    public string?  Remarks        { get; set; }

    public ICollection<DepreciationRunLine> Lines { get; set; } = new List<DepreciationRunLine>();
}

public class DepreciationRunLine
{
    public int     Id                  { get; set; }
    public int     DepreciationRunId   { get; set; }
    public int     LineNum             { get; set; }
    public int     AssetId             { get; set; }
    public string  ItemNo              { get; set; } = null!;
    public string  Description         { get; set; } = null!;
    public string  Method              { get; set; } = null!;
    public decimal Amount              { get; set; }
    public decimal AccumulatedBefore   { get; set; }
    public decimal AccumulatedAfter    { get; set; }
    public int     RemainingLifeBefore { get; set; }
    public int     RemainingLifeAfter  { get; set; }
    public string  StatusBefore        { get; set; } = null!;   // restored on reverse (a first run moves New → Active)
    public string  ExpenseAccount      { get; set; } = null!;
    public string  AccumulatedAccount  { get; set; } = null!;

    public DepreciationRun DepreciationRun { get; set; } = null!;
}

public class DepreciationRunConfig : IEntityTypeConfiguration<DepreciationRun>
{
    public void Configure(EntityTypeBuilder<DepreciationRun> b)
    {
        b.ToTable("KDPR");
        b.HasKey(x => x.Id);
        b.Property(x => x.RunNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.RunNo).IsUnique();
        b.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("Posted");
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => new { x.PeriodYear, x.PeriodMonth });

        b.HasMany(x => x.Lines).WithOne(l => l.DepreciationRun)
         .HasForeignKey(l => l.DepreciationRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DepreciationRunLineConfig : IEntityTypeConfiguration<DepreciationRunLine>
{
    public void Configure(EntityTypeBuilder<DepreciationRunLine> b)
    {
        b.ToTable("DPR1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Description).HasMaxLength(200);
        b.Property(x => x.Method).HasMaxLength(30);
        b.Property(x => x.StatusBefore).HasMaxLength(20);
        b.Property(x => x.ExpenseAccount).HasMaxLength(50);
        b.Property(x => x.AccumulatedAccount).HasMaxLength(50);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.AccumulatedBefore).HasPrecision(18, 2);
        b.Property(x => x.AccumulatedAfter).HasPrecision(18, 2);
        b.HasIndex(x => x.AssetId);
    }
}

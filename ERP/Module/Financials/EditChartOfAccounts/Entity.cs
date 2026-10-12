using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.EditChartOfAccounts;

// ═══════════════════════════════════════════════════════════════
// EDIT CHART OF ACCOUNTS — BATCH (KCOE)
// SAP B1 equivalent: Financials > Edit Chart of Accounts
//
// Restructuring a live chart of accounts one account at a time is
// how charts get broken: a half-finished reparenting leaves orphans,
// and there is no record of what moved or why. This module stages the
// whole restructure as a batch — every intended change is listed,
// checked as a set, and only then written to OACT.
//
// A batch is Open while being prepared, Applied once written (each
// line keeping the value it overwrote, so the change is auditable and
// reversible), or Cancelled if abandoned. Applied batches are
// immutable.
// ═══════════════════════════════════════════════════════════════
public class ChartOfAccountsEditBatch : AuditableEntity
{
    public string?  DocNo       { get; set; }            // COE-2026-00001 (auto)
    public string   Status      { get; set; } = "Open";  // Open / Applied / Cancelled

    public DateTime BatchDate   { get; set; }
    public string?  Description { get; set; }

    public DateTime? AppliedAt  { get; set; }
    public string?   AppliedBy  { get; set; }

    public string?  Remarks     { get; set; }

    public ICollection<ChartOfAccountsEditLine> Lines { get; set; }
        = new List<ChartOfAccountsEditLine>();
}

// ═══════════════════════════════════════════════════════════════
// EDIT CHART OF ACCOUNTS — LINE (COE1)
// One intended change to one account. OldValue is blank until the
// batch is applied; it is captured at that moment from the live
// account, so it always reflects what was actually overwritten
// rather than what the screen last showed.
// ═══════════════════════════════════════════════════════════════
public class ChartOfAccountsEditLine
{
    public int Id      { get; set; }
    public int BatchId { get; set; }
    public int LineNum { get; set; }

    public ChartOfAccountsEditBatch? Batch { get; set; }

    /// <summary>Account being changed (OACT.AcctCode).</summary>
    public string  AcctCode   { get; set; } = null!;
    /// <summary>Account name at the time the line was added, for display only.</summary>
    public string? AcctName   { get; set; }

    /// <summary>
    /// Rename / Move / Activate / Deactivate / AllowPosting / BlockPosting.
    /// Rename and Move consume NewValue; the rest are flag flips.
    /// </summary>
    public string  ChangeType { get; set; } = null!;

    /// <summary>New name for Rename, new parent account code for Move.</summary>
    public string? NewValue   { get; set; }
    /// <summary>What the field held before the batch was applied.</summary>
    public string? OldValue   { get; set; }

    public bool    Applied    { get; set; }
    public string? Note       { get; set; }
}

// ─── EF configuration ────────────────────────────────────────
public class ChartOfAccountsEditBatchConfig : IEntityTypeConfiguration<ChartOfAccountsEditBatch>
{
    public void Configure(EntityTypeBuilder<ChartOfAccountsEditBatch> b)
    {
        b.ToTable("KCOE");
        b.HasKey(x => x.Id);

        b.Property(x => x.DocNo).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
        b.Property(x => x.Description).HasMaxLength(300);
        b.Property(x => x.AppliedBy).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.DocNo).IsUnique();
        b.HasIndex(x => new { x.Status, x.BatchDate });

        b.HasMany(x => x.Lines)
            .WithOne(l => l.Batch)
            .HasForeignKey(l => l.BatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChartOfAccountsEditLineConfig : IEntityTypeConfiguration<ChartOfAccountsEditLine>
{
    public void Configure(EntityTypeBuilder<ChartOfAccountsEditLine> b)
    {
        b.ToTable("COE1");
        b.HasKey(x => x.Id);

        b.Property(x => x.AcctCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AcctName).HasMaxLength(200);
        b.Property(x => x.ChangeType).HasMaxLength(30).IsRequired();
        b.Property(x => x.NewValue).HasMaxLength(200);
        b.Property(x => x.OldValue).HasMaxLength(200);
        b.Property(x => x.Note).HasMaxLength(500);

        b.HasIndex(x => x.BatchId);
        b.HasIndex(x => x.AcctCode);
    }
}

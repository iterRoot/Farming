using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.FinancialReportTemplates;

// ═══════════════════════════════════════════════════════════════
// FINANCIAL REPORT TEMPLATE — HEADER (KFRT)
// SAP B1 equivalent: Financials > Financial Report Templates
//
// A template is the SHAPE of a financial statement, held apart from
// the numbers. It says which accounts roll into "Current Assets",
// what order the sections print in, and which lines are computed
// from other lines. Running a Balance Sheet then means pouring
// period balances into a template rather than hard-coding a layout
// into the report.
//
// One template per report type may be marked default; that is the
// one a report reaches for when the user picks no template.
// ═══════════════════════════════════════════════════════════════
public class FinancialReportTemplate : AuditableEntity
{
    public string  Code        { get; set; } = null!;   // BS-STD, PL-2026 …
    public string  Name        { get; set; } = null!;

    /// <summary>BalanceSheet / ProfitAndLoss / TrialBalance / CashFlow.</summary>
    public string  ReportType  { get; set; } = null!;

    public string  Currency    { get; set; } = "USD";
    public string? Description { get; set; }

    /// <summary>The template a report of this type uses when none is chosen.</summary>
    public bool    IsDefault   { get; set; }
    public bool    IsActive    { get; set; } = true;

    public string? Remarks     { get; set; }

    public ICollection<FinancialReportTemplateRow> Rows { get; set; }
        = new List<FinancialReportTemplateRow>();
}

// ═══════════════════════════════════════════════════════════════
// FINANCIAL REPORT TEMPLATE — ROW (FRT1)
// One printed line. RowType decides which of the optional columns
// carry meaning:
//
//   Title     — a heading; children nest under it
//   Account   — pulls the balance of AccountFrom..AccountTo
//   Formula   — arithmetic over other rows, by RowCode
//   Text      — a literal line, no value
//   PageBreak — layout only
// ═══════════════════════════════════════════════════════════════
public class FinancialReportTemplateRow
{
    public int Id         { get; set; }
    public int TemplateId { get; set; }
    public int LineNum    { get; set; }

    public FinancialReportTemplate? Template { get; set; }

    /// <summary>Stable handle for this row, unique inside the template. Formulas cite it.</summary>
    public string  RowCode     { get; set; } = null!;
    public string  Description { get; set; } = null!;
    public string  RowType     { get; set; } = "Account";

    /// <summary>Indent depth when printed; 1 is the outermost level.</summary>
    public int     Level         { get; set; } = 1;
    /// <summary>RowCode of the heading this row sits under, if any.</summary>
    public string? ParentRowCode { get; set; }

    // ── Account rows ─────────────────────────────────────────
    public string? AccountFrom { get; set; }
    public string? AccountTo   { get; set; }

    // ── Formula rows ─────────────────────────────────────────
    /// <summary>Arithmetic over row codes, e.g. "A100 + A200 - A300".</summary>
    public string? Formula     { get; set; }

    // ── Presentation ─────────────────────────────────────────
    /// <summary>1 prints the balance as-is, -1 flips it (income shown positive).</summary>
    public int  Sign      { get; set; } = 1;
    public bool Bold      { get; set; }
    public bool Underline { get; set; }
    /// <summary>Print the line even when it evaluates to zero.</summary>
    public bool PrintZero { get; set; } = true;

    public string? Remarks { get; set; }
}

// ─── EF configuration ────────────────────────────────────────
public class FinancialReportTemplateConfig : IEntityTypeConfiguration<FinancialReportTemplate>
{
    public void Configure(EntityTypeBuilder<FinancialReportTemplate> b)
    {
        b.ToTable("KFRT");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ReportType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => new { x.ReportType, x.IsDefault });

        b.HasMany(x => x.Rows)
            .WithOne(r => r.Template)
            .HasForeignKey(r => r.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FinancialReportTemplateRowConfig : IEntityTypeConfiguration<FinancialReportTemplateRow>
{
    public void Configure(EntityTypeBuilder<FinancialReportTemplateRow> b)
    {
        b.ToTable("FRT1");
        b.HasKey(x => x.Id);

        b.Property(x => x.RowCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300).IsRequired();
        b.Property(x => x.RowType).HasMaxLength(20).HasDefaultValue("Account");
        b.Property(x => x.ParentRowCode).HasMaxLength(50);
        b.Property(x => x.AccountFrom).HasMaxLength(50);
        b.Property(x => x.AccountTo).HasMaxLength(50);
        b.Property(x => x.Formula).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.TemplateId);
        // A row code is a handle within one template, so it is only unique there.
        b.HasIndex(x => new { x.TemplateId, x.RowCode }).IsUnique();
    }
}

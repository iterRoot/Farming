using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.HumanResources.Payroll;

// ═══════════════════════════════════════════════════════════════
// PAYROLL  (KPAY)
// One row per employee per pay period (year + month).
//
// Cambodian statutory pieces are COMPUTED on save, not typed:
//   • TaxOnSalary   — progressive Tax on Salary (TOS), KHR brackets
//   • NssfEmployee  — 2% pension, wage capped 200,000–1,200,000 KHR
//   • NssfEmployer  — 5.4% employer NSSF (informational, not deducted)
//   • OvertimePay / UnpaidLeaveDeduction — pulled from that month's
//     attendance summary
// GrossPay / NetPay are derived in the response.
// ═══════════════════════════════════════════════════════════════
public class Payroll : AuditableEntity
{
    public string   EmployeeCode     { get; set; } = null!;
    public string?  EmployeeName     { get; set; }
    public int      PeriodYear       { get; set; }
    public int      PeriodMonth      { get; set; }   // 1-12

    // ── Inputs (USD) ──────────────────────────────────────────
    public decimal  BasicSalary      { get; set; }
    public decimal? TaxableAllowance { get; set; }   // into the TOS base
    public decimal? ExemptAllowance  { get; set; }   // meal / transport within limits — untaxed
    public decimal? Bonus            { get; set; }   // taxable
    public decimal? OtherDeductions  { get; set; }   // advances etc. — non-statutory

    // ── Period parameters ─────────────────────────────────────
    public decimal  ExchangeRate     { get; set; } = 4100m;  // KHR per 1 USD
    public int      Dependents       { get; set; }           // snapshot from EmployeeMaster

    // ── Computed on save ──────────────────────────────────────
    public decimal? OvertimePay          { get; set; }  // from attendance OT hours
    public decimal? UnpaidLeaveDeduction { get; set; }  // from attendance unpaid days
    public decimal? TaxOnSalary          { get; set; }  // progressive TOS, in USD
    public decimal? NssfEmployee         { get; set; }  // 2% pension, USD
    public decimal? NssfEmployer         { get; set; }  // 5.4% employer NSSF, USD

    // ── Payment ───────────────────────────────────────────────
    public DateTime? PaymentDate     { get; set; }
    public string?  PaymentMethod    { get; set; }   // Bank / Cash / Cheque
    public string   Status           { get; set; } = "Draft"; // Draft / Approved / Paid
    public string?  Remarks          { get; set; }
}

public class PayrollConfig : IEntityTypeConfiguration<Payroll>
{
    public void Configure(EntityTypeBuilder<Payroll> b)
    {
        b.ToTable("KPAY");
        b.HasKey(x => x.Id);

        b.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.EmployeeName).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.PaymentMethod).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Property(x => x.BasicSalary).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxableAllowance).HasColumnType("decimal(18,2)");
        b.Property(x => x.ExemptAllowance).HasColumnType("decimal(18,2)");
        b.Property(x => x.Bonus).HasColumnType("decimal(18,2)");
        b.Property(x => x.OtherDeductions).HasColumnType("decimal(18,2)");
        b.Property(x => x.OvertimePay).HasColumnType("decimal(18,2)");
        b.Property(x => x.UnpaidLeaveDeduction).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxOnSalary).HasColumnType("decimal(18,2)");
        b.Property(x => x.NssfEmployee).HasColumnType("decimal(18,2)");
        b.Property(x => x.NssfEmployer).HasColumnType("decimal(18,2)");
        b.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");

        b.HasIndex(x => new { x.EmployeeCode, x.PeriodYear, x.PeriodMonth }).IsUnique();
    }
}

// ═══════════════════════════════════════════════════════════════
// PAYROLL RUN LOG  (KPRL)
// One row per monthly run — manual or automatic. Doubles as the
// scheduler's "have I already done this period?" marker, so a
// restart, a redeploy or a server that was down on the 1st cannot
// cause a double run or a silently missed one.
// ═══════════════════════════════════════════════════════════════
public class PayrollRunLog : AuditableEntity
{
    public int      PeriodYear  { get; set; }
    public int      PeriodMonth { get; set; }
    public string   Trigger     { get; set; } = "Manual";  // Manual / Auto
    public string   Status      { get; set; } = "Success"; // Success / Failed
    public DateTime RunAt       { get; set; }
    public int      DurationMs  { get; set; }

    public decimal  ExchangeRate { get; set; }
    public int      Created      { get; set; }
    public int      Refreshed    { get; set; }
    public int      SkippedPaid  { get; set; }
    public int      SkippedNotEmployed { get; set; }

    public decimal  TotalGross        { get; set; }
    public decimal  TotalNet          { get; set; }
    public decimal  TotalTax          { get; set; }
    public decimal  TotalNssfEmployee { get; set; }
    public decimal  TotalNssfEmployer { get; set; }

    public string?  Error { get; set; }
}

public class PayrollRunLogConfig : IEntityTypeConfiguration<PayrollRunLog>
{
    public void Configure(EntityTypeBuilder<PayrollRunLog> b)
    {
        b.ToTable("KPRL");
        b.HasKey(x => x.Id);

        b.Property(x => x.Trigger).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Error).HasMaxLength(2000);

        b.Property(x => x.ExchangeRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.TotalGross).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalNet).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalTax).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalNssfEmployee).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalNssfEmployer).HasColumnType("decimal(18,2)");

        b.HasIndex(x => new { x.PeriodYear, x.PeriodMonth, x.Trigger });
    }
}

// ═══════════════════════════════════════════════════════════════
// CAMBODIAN PAYROLL MATH
// Constants are the 2023 Sub-Decree 196 TOS brackets and current
// NSSF rates. Change here if the law changes.
// ═══════════════════════════════════════════════════════════════
public static class PayrollMath
{
    public const decimal DependentReliefKhr = 150_000m;      // per dependent, monthly
    public const decimal NssfBaseMinKhr     = 200_000m;
    public const decimal NssfBaseMaxKhr     = 1_200_000m;
    public const decimal NssfEmployeeRate   = 0.02m;         // pension, employee
    public const decimal NssfEmployerRate   = 0.054m;        // 0.8 ORS + 2.6 HC + 2.0 pension
    public const decimal StandardDaysPerMonth = 26m;
    public const decimal StandardHoursPerDay  = 8m;
    public const decimal OvertimeMultiplier   = 1.5m;

    /// <summary>Monthly Tax on Salary in KHR (progressive, quick-deduction form). Floors at 0.</summary>
    public static decimal TosKhr(decimal taxableKhr)
    {
        if (taxableKhr <= 1_500_000m) return 0m;
        decimal t = taxableKhr switch
        {
            <= 2_000_000m  => taxableKhr * 0.05m -    75_000m,
            <= 8_500_000m  => taxableKhr * 0.10m -   175_000m,
            <= 12_500_000m => taxableKhr * 0.15m -   600_000m,
            _              => taxableKhr * 0.20m - 1_225_000m,
        };
        return t < 0m ? 0m : t;
    }
}

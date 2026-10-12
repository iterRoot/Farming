namespace FarmingApi.Modules.HumanResources.Payroll;

public class PayrollResponse
{
    public int      Id               { get; set; }
    public string   EmployeeCode     { get; set; } = null!;
    public string?  EmployeeName     { get; set; }
    public int      PeriodYear       { get; set; }
    public int      PeriodMonth      { get; set; }
    public string   Period           { get; set; } = null!;  // "2026-03"

    public decimal  BasicSalary      { get; set; }
    public decimal? TaxableAllowance { get; set; }
    public decimal? ExemptAllowance  { get; set; }
    public decimal? Bonus            { get; set; }
    public decimal? OtherDeductions  { get; set; }

    public decimal  ExchangeRate     { get; set; }
    public int      Dependents       { get; set; }

    // Computed / derived
    public decimal? OvertimePay          { get; set; }
    public decimal? UnpaidLeaveDeduction { get; set; }
    public decimal? TaxOnSalary          { get; set; }   // TOS, USD
    public decimal? NssfEmployee         { get; set; }   // 2% pension, USD
    public decimal? NssfEmployer         { get; set; }   // employer NSSF, USD
    public decimal  TaxableIncomeKhr     { get; set; }   // the base the brackets were applied to
    public decimal  GrossPay             { get; set; }
    public decimal  TotalDeductions      { get; set; }
    public decimal  NetPay               { get; set; }
    public decimal  EmployerCost         { get; set; }   // GrossPay + NssfEmployer

    public DateTime? PaymentDate     { get; set; }
    public string?  PaymentMethod    { get; set; }
    public string   Status           { get; set; } = null!;
    public string?  Remarks          { get; set; }
    public DateTime CreatedAt        { get; set; }
    public DateTime? UpdatedAt       { get; set; }
}

// ── Monthly run: one draft payslip per active employee ──────────────
public class PayrollGenerateRequest
{
    public int      PeriodYear   { get; set; }
    public int      PeriodMonth  { get; set; }
    /// <summary>KHR per 1 USD. Null → latest USD rate, else 4,100.</summary>
    public decimal? ExchangeRate { get; set; }
    /// <summary>Limit the run to these employee codes. Empty/null → every active employee.</summary>
    public List<string>? EmployeeCodes { get; set; }
    /// <summary>Also refresh payslips that already exist for the period, as long as they are not Paid.</summary>
    public bool     RefreshExisting { get; set; } = true;
}

public class PayrollGenerateResponse
{
    public int    PeriodYear  { get; set; }
    public int    PeriodMonth { get; set; }
    public string Period      { get; set; } = null!;
    public decimal ExchangeRate { get; set; }
    public string Trigger     { get; set; } = "Manual";

    public int Created     { get; set; }
    public int Refreshed   { get; set; }
    public int SkippedPaid { get; set; }
    public int SkippedNotEmployed { get; set; }

    public decimal TotalGross { get; set; }
    public decimal TotalNet   { get; set; }
    public decimal TotalTax   { get; set; }
    public decimal TotalNssfEmployee { get; set; }
    public decimal TotalNssfEmployer { get; set; }

    public List<PayrollResponse> Payslips { get; set; } = new();
    public List<string>          Notes    { get; set; } = new();
}

// ── Run history ─────────────────────────────────────────────────────
public class PayrollRunLogResponse
{
    public int      Id          { get; set; }
    public int      PeriodYear  { get; set; }
    public int      PeriodMonth { get; set; }
    public string   Period      { get; set; } = null!;
    public string   Trigger     { get; set; } = null!;
    public string   Status      { get; set; } = null!;
    public DateTime RunAt       { get; set; }
    public int      DurationMs  { get; set; }
    public decimal  ExchangeRate { get; set; }
    public int      Created     { get; set; }
    public int      Refreshed   { get; set; }
    public int      SkippedPaid { get; set; }
    public int      SkippedNotEmployed { get; set; }
    public decimal  TotalGross        { get; set; }
    public decimal  TotalNet          { get; set; }
    public decimal  TotalTax          { get; set; }
    public decimal  TotalNssfEmployee { get; set; }
    public decimal  TotalNssfEmployer { get; set; }
    public string?  Error       { get; set; }
}

/// <summary>What the scheduler is configured to do and what it last did.</summary>
public class PayrollAutoRunStatusResponse
{
    public bool      Enabled     { get; set; }
    public int       DayOfMonth  { get; set; }
    public int       HourUtc     { get; set; }
    public string    Target      { get; set; } = null!;   // PreviousMonth / CurrentMonth
    public string?   NextPeriod  { get; set; }            // period the next run will cover
    public DateTime? NextRunUtc  { get; set; }
    public PayrollRunLogResponse? LastRun { get; set; }
}

// Tax / NSSF / OT / unpaid-leave are computed server-side; they are not accepted here.
public class PayrollRequest
{
    public string   EmployeeCode     { get; set; } = null!;
    public string?  EmployeeName     { get; set; }
    public int      PeriodYear       { get; set; }
    public int      PeriodMonth      { get; set; }
    public decimal  BasicSalary      { get; set; }
    public decimal? TaxableAllowance { get; set; }
    public decimal? ExemptAllowance  { get; set; }
    public decimal? Bonus            { get; set; }
    public decimal? OtherDeductions  { get; set; }
    public decimal? ExchangeRate     { get; set; }   // null → latest USD rate, else 4100
    public DateTime? PaymentDate     { get; set; }
    public string?  PaymentMethod    { get; set; }
    public string   Status           { get; set; } = "Draft";
    public string?  Remarks          { get; set; }
}

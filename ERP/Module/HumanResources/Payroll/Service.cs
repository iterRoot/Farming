using AutoMapper;
using FarmingApi.Modules.HumanResources.Attendance;
using FarmingApi.Modules.HumanResources.EmployeeMaster;
using FarmingApi.Modules.Financials.ExchangeRate;

namespace FarmingApi.Modules.HumanResources.Payroll;

// ═══════════════════════════════════════════════════════════════
// PAYROLL SERVICE
// The calculation and the monthly run live here rather than in the
// controller, because two callers need them: the "Generate" button
// and the scheduler (PayrollAutoRunWorker). One implementation means
// an automatic run can never drift from a manual one.
// ═══════════════════════════════════════════════════════════════
public interface IPayrollService
{
    /// <summary>Fill OT, unpaid leave, TOS and NSSF from the employee, the period's attendance and the rate.</summary>
    void Recalculate(Payroll p, decimal? requestedRate);

    PayrollResponse ToResponse(Payroll p);

    decimal LatestUsdRate();

    /// <summary>One draft payslip per active employee for the period. Idempotent; never rewrites a Paid slip.</summary>
    PayrollGenerateResponse Generate(PayrollGenerateRequest dto, string trigger = "Manual");
}

public class PayrollService : IPayrollService
{
    private readonly IPayrollRepository        _repo;
    private readonly IPayrollRunLogRepository  _log;
    private readonly IEmployeeMasterRepository _employees;
    private readonly IAttendanceRepository     _attendance;
    private readonly IExchangeRateRepository   _rates;
    private readonly IMapper                   _mapper;

    public PayrollService(IPayrollRepository repo,
                          IPayrollRunLogRepository log,
                          IEmployeeMasterRepository employees,
                          IAttendanceRepository attendance,
                          IExchangeRateRepository rates,
                          IMapper mapper)
    {
        _repo       = repo;
        _log        = log;
        _employees  = employees;
        _attendance = attendance;
        _rates      = rates;
        _mapper     = mapper;
    }

    // ═══════════════════════════════════════════════════════════
    // PER-PAYSLIP CALCULATION
    // ═══════════════════════════════════════════════════════════
    public void Recalculate(Payroll p, decimal? requestedRate)
    {
        // 1 — exchange rate: request value, else latest USD rate, else default.
        var rate = requestedRate is > 0 ? requestedRate!.Value : LatestUsdRate();
        p.ExchangeRate = rate;

        // 2 — dependents come from the employee master (one place to maintain).
        var emp = _employees.GetSingle(e => e.Code == p.EmployeeCode);
        p.Dependents = emp?.Dependents ?? 0;
        if (emp != null && string.IsNullOrWhiteSpace(p.EmployeeName))
            p.EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim();

        // 3 — attendance roll-up for this period.
        var rows = _attendance.GetAll().Where(a => a.EmployeeCode == p.EmployeeCode).ToList();
        var att  = AttendanceMath.Summarize(rows, p.EmployeeCode, p.PeriodYear, p.PeriodMonth);

        var dailyRate  = p.BasicSalary / PayrollMath.StandardDaysPerMonth;
        var hourlyRate = dailyRate / PayrollMath.StandardHoursPerDay;

        p.OvertimePay          = Money(att.TotalOvertimeHours * hourlyRate * PayrollMath.OvertimeMultiplier);
        p.UnpaidLeaveDeduction = Money(att.UnpaidDays * dailyRate);

        // 4 — Tax on Salary: progressive KHR brackets on the taxable base.
        var taxableUsd = p.BasicSalary + (p.TaxableAllowance ?? 0m)
                       + (p.OvertimePay ?? 0m) + (p.Bonus ?? 0m);
        var taxableKhr = taxableUsd * rate - p.Dependents * PayrollMath.DependentReliefKhr;
        if (taxableKhr < 0m) taxableKhr = 0m;
        p.TaxOnSalary = Money(PayrollMath.TosKhr(taxableKhr) / rate);

        // 5 — NSSF: 2% employee / 5.4% employer on the capped contributory wage.
        var baseKhr = Math.Clamp(p.BasicSalary * rate,
            PayrollMath.NssfBaseMinKhr, PayrollMath.NssfBaseMaxKhr);
        p.NssfEmployee = Money(baseKhr * PayrollMath.NssfEmployeeRate / rate);
        p.NssfEmployer = Money(baseKhr * PayrollMath.NssfEmployerRate / rate);
    }

    public PayrollResponse ToResponse(Payroll p)
    {
        var dto = _mapper.Map<PayrollResponse>(p);
        var taxableUsd = p.BasicSalary + (p.TaxableAllowance ?? 0m)
                       + (p.OvertimePay ?? 0m) + (p.Bonus ?? 0m);
        var taxableKhr = taxableUsd * p.ExchangeRate - p.Dependents * PayrollMath.DependentReliefKhr;
        dto.TaxableIncomeKhr = taxableKhr < 0m ? 0m : Math.Round(taxableKhr, 0);
        return dto;
    }

    public decimal LatestUsdRate()
    {
        var r = _rates.GetAll()
            .Where(x => x.Currency == "USD")
            .OrderByDescending(x => x.EffectiveDate)
            .FirstOrDefault();
        return r is { Rate: > 0 } ? r.Rate : 4100m;
    }

    // ═══════════════════════════════════════════════════════════
    // THE MONTHLY RUN
    // ═══════════════════════════════════════════════════════════
    public PayrollGenerateResponse Generate(PayrollGenerateRequest dto, string trigger = "Manual")
    {
        var startedAt = DateTime.UtcNow;
        var rate = dto.ExchangeRate is > 0 ? dto.ExchangeRate!.Value : LatestUsdRate();

        // Period boundaries — used to skip people hired later or already gone.
        var periodStart = new DateTime(dto.PeriodYear, dto.PeriodMonth, 1);
        var periodEnd   = periodStart.AddMonths(1).AddDays(-1);

        var wanted = dto.EmployeeCodes?
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var employees = _employees.GetAll()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Code)
            .ToList();

        if (wanted is { Count: > 0 })
            employees = employees.Where(e => wanted.Contains(e.Code)).ToList();

        var existing = _repo.GetAll()
            .Where(p => p.PeriodYear == dto.PeriodYear && p.PeriodMonth == dto.PeriodMonth)
            .ToList();

        var result = new PayrollGenerateResponse
        {
            PeriodYear   = dto.PeriodYear,
            PeriodMonth  = dto.PeriodMonth,
            Period       = $"{dto.PeriodYear:D4}-{dto.PeriodMonth:D2}",
            ExchangeRate = rate,
            Trigger      = trigger,
        };

        // Collected here and mapped after the commit, so new rows carry their Id.
        var touched = new List<Payroll>();

        foreach (var emp in employees)
        {
            // Not employed during this period → nothing to pay.
            if (emp.HireDate.HasValue && emp.HireDate.Value.Date > periodEnd)
            {
                result.SkippedNotEmployed++;
                result.Notes.Add($"{emp.Code}: hired {emp.HireDate:yyyy-MM-dd}, after this period.");
                continue;
            }
            if (emp.TerminationDate.HasValue && emp.TerminationDate.Value.Date < periodStart)
            {
                result.SkippedNotEmployed++;
                result.Notes.Add($"{emp.Code}: terminated {emp.TerminationDate:yyyy-MM-dd}, before this period.");
                continue;
            }

            var slip = existing.FirstOrDefault(p =>
                string.Equals(p.EmployeeCode, emp.Code, StringComparison.OrdinalIgnoreCase));

            if (slip == null)
            {
                slip = new Payroll
                {
                    EmployeeCode = emp.Code,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    PeriodYear   = dto.PeriodYear,
                    PeriodMonth  = dto.PeriodMonth,
                    BasicSalary  = emp.Salary ?? 0m,
                    Status       = "Draft",
                    CreatedAt    = DateTime.UtcNow,
                    InActive     = false,
                };
                Recalculate(slip, rate);
                _repo.Add(slip);
                result.Created++;
            }
            else if (slip.Status == "Paid")
            {
                // A paid payslip is a settled record — never rewrite it.
                result.SkippedPaid++;
                result.Notes.Add($"{emp.Code}: already Paid for this period, left untouched.");
            }
            else if (dto.RefreshExisting)
            {
                // Basic salary follows the employee master unless it was
                // deliberately overridden to something non-zero already.
                if (slip.BasicSalary == 0m && emp.Salary is > 0) slip.BasicSalary = emp.Salary!.Value;
                slip.EmployeeName ??= $"{emp.FirstName} {emp.LastName}".Trim();
                Recalculate(slip, rate);
                slip.UpdatedAt = DateTime.UtcNow;
                _repo.Update(slip);
                result.Refreshed++;
            }

            touched.Add(slip);
        }

        _repo.Commit();

        result.Payslips = touched
            .OrderBy(p => p.EmployeeCode)
            .Select(ToResponse)
            .ToList();

        result.TotalGross        = result.Payslips.Sum(p => p.GrossPay);
        result.TotalNet          = result.Payslips.Sum(p => p.NetPay);
        result.TotalTax          = result.Payslips.Sum(p => p.TaxOnSalary ?? 0m);
        result.TotalNssfEmployee = result.Payslips.Sum(p => p.NssfEmployee ?? 0m);
        result.TotalNssfEmployer = result.Payslips.Sum(p => p.NssfEmployer ?? 0m);

        WriteLog(result, startedAt, trigger, null);
        return result;
    }

    /// <summary>Record the run — the scheduler reads this back to know a period is done.</summary>
    public void WriteLog(PayrollGenerateResponse r, DateTime startedAt, string trigger, string? error)
    {
        _log.Add(new PayrollRunLog
        {
            PeriodYear         = r.PeriodYear,
            PeriodMonth        = r.PeriodMonth,
            Trigger            = trigger,
            Status             = error == null ? "Success" : "Failed",
            RunAt              = startedAt,
            DurationMs         = (int)(DateTime.UtcNow - startedAt).TotalMilliseconds,
            ExchangeRate       = r.ExchangeRate,
            Created            = r.Created,
            Refreshed          = r.Refreshed,
            SkippedPaid        = r.SkippedPaid,
            SkippedNotEmployed = r.SkippedNotEmployed,
            TotalGross         = r.TotalGross,
            TotalNet           = r.TotalNet,
            TotalTax           = r.TotalTax,
            TotalNssfEmployee  = r.TotalNssfEmployee,
            TotalNssfEmployer  = r.TotalNssfEmployer,
            Error              = error,
            CreatedAt          = DateTime.UtcNow,
            InActive           = false,
        });
        _log.Commit();
    }

    private static decimal Money(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}

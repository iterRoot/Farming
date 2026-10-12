using System.Globalization;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using FarmingApi.Modules.HumanResources.EmployeeMaster;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.HumanResources.Payroll;

[ApiController]
[Route("[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollRepository        _repo;
    private readonly IPayrollRunLogRepository  _log;
    private readonly IPayrollService           _service;
    private readonly IEmployeeMasterRepository _employees;
    private readonly MyDbContext               _db;
    private readonly IConfiguration            _config;
    private readonly IMapper                   _mapper;

    private static readonly string[] Statuses = { "Draft", "Approved", "Paid" };

    public PayrollController(IPayrollRepository repo,
                             IPayrollRunLogRepository log,
                             IPayrollService service,
                             IEmployeeMasterRepository employees,
                             MyDbContext db,
                             IConfiguration config,
                             IMapper mapper)
    {
        _repo      = repo;
        _log       = log;
        _service   = service;
        _employees = employees;
        _db        = db;
        _config    = config;
        _mapper    = mapper;
    }

    // ── GET /Payroll ──────────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? employeeCode = null,
                                [FromQuery] int? year = null,
                                [FromQuery] int? month = null,
                                [FromQuery] string? status = null)
    {
        var list = _repo.GetAll()
            .Where(x => employeeCode == null || x.EmployeeCode == employeeCode)
            .Where(x => year == null || x.PeriodYear == year)
            .Where(x => month == null || x.PeriodMonth == month)
            .Where(x => status == null || x.Status == status)
            .OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth)
            .ThenBy(x => x.EmployeeCode)
            .ToList();
        return Ok(list.Select(_service.ToResponse).ToList());
    }

    // ── GET /Payroll/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_service.ToResponse(e));
    }

    // ── POST /Payroll ────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] PayrollRequest dto)
    {
        var check = Validate(dto);
        if (check != null) return BadRequest(check);

        var code = dto.EmployeeCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.EmployeeCode == code
                                 && x.PeriodYear == dto.PeriodYear
                                 && x.PeriodMonth == dto.PeriodMonth))
            return BadRequest($"A payroll record for '{code}' in {dto.PeriodYear:D4}-{dto.PeriodMonth:D2} already exists");

        var entity          = _mapper.Map<Payroll>(dto);
        entity.EmployeeCode = code;
        entity.CreatedAt    = DateTime.UtcNow;
        entity.InActive     = false;
        _service.Recalculate(entity, dto.ExchangeRate);
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_service.ToResponse(entity));
    }

    // ── PUT /Payroll/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PayrollRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status == "Paid")
            return BadRequest("This payslip is Paid and settled — it can no longer be edited.");

        var check = Validate(dto);
        if (check != null) return BadRequest(check);

        var code = dto.EmployeeCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.EmployeeCode == code
                                 && x.PeriodYear == dto.PeriodYear
                                 && x.PeriodMonth == dto.PeriodMonth
                                 && x.Id != id))
            return BadRequest($"A payroll record for '{code}' in {dto.PeriodYear:D4}-{dto.PeriodMonth:D2} already exists");

        _mapper.Map(dto, entity);
        entity.EmployeeCode = code;
        entity.UpdatedAt    = DateTime.UtcNow;
        _service.Recalculate(entity, dto.ExchangeRate);
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_service.ToResponse(entity));
    }

    // ── DELETE /Payroll/{id} ────────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status == "Paid")
            return BadRequest("This payslip is Paid and settled — it can no longer be deleted.");

        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════
    // POST /Payroll/Generate — the monthly run, on demand.
    // Identical code path to the scheduler; see PayrollService.
    // ═══════════════════════════════════════════════════════════
    [HttpPost("Generate")]
    public IActionResult Generate([FromBody] PayrollGenerateRequest dto)
    {
        if (dto == null) return BadRequest("No period supplied.");
        if (dto.PeriodYear < 2000 || dto.PeriodYear > 2100) return BadRequest("Period year is not valid");
        if (dto.PeriodMonth < 1 || dto.PeriodMonth > 12)    return BadRequest("Period month must be 1-12");
        if (dto.ExchangeRate is <= 0)                       return BadRequest("Exchange rate must be greater than zero");

        return Ok(_service.Generate(dto, trigger: "Manual"));
    }

    // ═══════════════════════════════════════════════════════════
    // GET /Payroll/{id}/Payslip — one payslip as a PDF
    // ═══════════════════════════════════════════════════════════
    [HttpGet("{id:int}/Payslip")]
    public IActionResult Payslip(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var dto      = _service.ToResponse(entity);
        var employee = _employees.GetSingle(e => e.Code == entity.EmployeeCode);
        var company  = _db.Set<CompanyEntity>().OrderBy(c => c.Id).FirstOrDefault();

        var bytes = new PayslipPdfDocument(dto, employee, company).GeneratePdf();
        var name  = $"Payslip_{entity.EmployeeCode}_{entity.PeriodYear:D4}-{entity.PeriodMonth:D2}.pdf";
        return File(bytes, "application/pdf", name);
    }

    // ═══════════════════════════════════════════════════════════
    // GET /Payroll/Payslips?year=&month=&status=
    // Every payslip for the period in one PDF, one per page — what
    // you actually print at the end of the month.
    // ═══════════════════════════════════════════════════════════
    [HttpGet("Payslips")]
    public IActionResult Payslips([FromQuery] int year, [FromQuery] int month,
                                  [FromQuery] string? status = null)
    {
        if (year < 2000 || year > 2100) return BadRequest("Period year is not valid");
        if (month < 1 || month > 12)    return BadRequest("Period month must be 1-12");

        var list = _repo.GetAll()
            .Where(p => p.PeriodYear == year && p.PeriodMonth == month)
            .Where(p => status == null || p.Status == status)
            .OrderBy(p => p.EmployeeCode)
            .ToList();

        if (list.Count == 0)
            return BadRequest($"No payslips for {year:D4}-{month:D2}" +
                              (status == null ? "." : $" with status {status}."));

        var company   = _db.Set<CompanyEntity>().OrderBy(c => c.Id).FirstOrDefault();
        var employees = _employees.GetAll().ToList();

        // Merge every payslip into one document, one page each.
        var merged = Document.Merge(list.Select(p =>
        {
            var emp = employees.FirstOrDefault(e =>
                string.Equals(e.Code, p.EmployeeCode, StringComparison.OrdinalIgnoreCase));
            return (IDocument)new PayslipPdfDocument(_service.ToResponse(p), emp, company);
        }).ToArray());

        var bytes = merged.GeneratePdf();
        return File(bytes, "application/pdf", $"Payslips_{year:D4}-{month:D2}.pdf");
    }

    // ═══════════════════════════════════════════════════════════
    // GET /Payroll/BankFile?year=&month=&status=&format=
    // Batch transfer file for the bank. Defaults to Approved+Paid
    // rows only — you do not send Drafts to a bank.
    //
    // format=csv   generic columns, opens in Excel (default)
    // format=aba   ACLEDA-style bulk-transfer layout
    // format=wing  Wing / mobile-wallet payout layout
    // ═══════════════════════════════════════════════════════════
    [HttpGet("BankFile")]
    public IActionResult BankFile([FromQuery] int year, [FromQuery] int month,
                                  [FromQuery] string? status = null,
                                  [FromQuery] string format = "csv",
                                  [FromQuery] string currency = "USD",
                                  [FromQuery] string? valueDate = null)
    {
        if (year < 2000 || year > 2100) return BadRequest("Period year is not valid");
        if (month < 1 || month > 12)    return BadRequest("Period month must be 1-12");

        format = (format ?? "csv").Trim().ToLowerInvariant();
        if (format is not ("csv" or "aba" or "wing"))
            return BadRequest("format must be csv, aba or wing.");

        var query = _repo.GetAll().Where(p => p.PeriodYear == year && p.PeriodMonth == month);

        // Drafts are not payable — exclude them unless explicitly asked for.
        query = status != null
            ? query.Where(p => p.Status == status)
            : query.Where(p => p.Status == "Approved" || p.Status == "Paid");

        var list = query.OrderBy(p => p.EmployeeCode).ToList();
        if (list.Count == 0)
            return BadRequest($"No payable payslips for {year:D4}-{month:D2}. " +
                              "Approve the payroll run first, or pass an explicit status.");

        var employees = _employees.GetAll().ToList();
        EmployeeMaster.EmployeeMaster? Emp(string code) => employees.FirstOrDefault(e =>
            string.Equals(e.Code, code, StringComparison.OrdinalIgnoreCase));

        // Anyone without a bank account cannot be paid by transfer — say so
        // up front rather than silently producing a short file.
        var missing = list
            .Where(p => string.IsNullOrWhiteSpace(Emp(p.EmployeeCode)?.BankAccountNo))
            .Select(p => p.EmployeeCode)
            .ToList();
        if (missing.Count > 0)
            return BadRequest("No bank account on file for: " + string.Join(", ", missing) +
                              ". Add it in Employee Master, or exclude them from the transfer.");

        var period = $"{year:D4}-{month:D2}";
        var vDate  = valueDate ?? DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var cur    = (currency ?? "USD").Trim().ToUpperInvariant();

        var sb = new StringBuilder();

        switch (format)
        {
            case "aba":
                sb.AppendLine(Csv("No", "AccountNumber", "AccountName", "Amount",
                                  "Currency", "Remark", "ValueDate"));
                for (var i = 0; i < list.Count; i++)
                {
                    var p = list[i]; var e = Emp(p.EmployeeCode);
                    sb.AppendLine(Csv((i + 1).ToString(), e!.BankAccountNo!,
                        p.EmployeeName ?? p.EmployeeCode, Amount(p),
                        cur, $"Salary {period}", vDate));
                }
                break;

            case "wing":
                sb.AppendLine(Csv("PhoneNumber", "ReceiverName", "Amount", "Currency", "Remark"));
                foreach (var p in list)
                {
                    var e = Emp(p.EmployeeCode);
                    sb.AppendLine(Csv(e!.Mobile ?? e.Phone ?? "", p.EmployeeName ?? p.EmployeeCode,
                        Amount(p), cur, $"Salary {period}"));
                }
                break;

            default: // csv
                sb.AppendLine(Csv("EmployeeCode", "EmployeeName", "BankName", "BankAccountNo",
                                  "Currency", "Amount", "Period", "ValueDate", "Reference"));
                foreach (var p in list)
                {
                    var e = Emp(p.EmployeeCode);
                    sb.AppendLine(Csv(p.EmployeeCode, p.EmployeeName ?? "",
                        e!.BankName ?? "", e.BankAccountNo!, cur, Amount(p),
                        period, vDate, $"Salary {period}"));
                }
                break;
        }

        // UTF-8 with BOM so Excel does not mangle non-ASCII names.
        var bytes = new UTF8Encoding(true).GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"BankTransfer_{period}_{format}.csv");
    }

    private static string Amount(Payroll p) =>
        _service_NetOf(p).ToString("F2", CultureInfo.InvariantCulture);

    // Net pay recomputed from the stored components — the same arithmetic
    // the response mapper uses, without needing the mapper here.
    private static decimal _service_NetOf(Payroll p) =>
        p.BasicSalary + (p.TaxableAllowance ?? 0m) + (p.ExemptAllowance ?? 0m)
      + (p.OvertimePay ?? 0m) + (p.Bonus ?? 0m)
      - (p.TaxOnSalary ?? 0m) - (p.NssfEmployee ?? 0m)
      - (p.UnpaidLeaveDeduction ?? 0m) - (p.OtherDeductions ?? 0m);

    private static string Csv(params string?[] values) =>
        string.Join(",", values.Select(v =>
        {
            var s = v ?? "";
            return s.Contains(',') || s.Contains('"') || s.Contains('\n')
                ? $"\"{s.Replace("\"", "\"\"")}\""
                : s;
        }));

    // ═══════════════════════════════════════════════════════════
    // GET /Payroll/RunLog — history of every run, manual and automatic
    // ═══════════════════════════════════════════════════════════
    [HttpGet("RunLog")]
    public IActionResult RunLog([FromQuery] int take = 24)
    {
        var list = _log.GetAll()
            .OrderByDescending(x => x.RunAt)
            .Take(Math.Clamp(take, 1, 200))
            .ToList()
            .Select(MapLog)
            .ToList();
        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════
    // GET /Payroll/AutoRun — what the scheduler will do, and its last run
    // ═══════════════════════════════════════════════════════════
    [HttpGet("AutoRun")]
    public IActionResult AutoRun()
    {
        var s   = PayrollAutoRunWorker.ReadSettings(_config);
        var now = DateTime.UtcNow;

        var last = _log.GetAll()
            .OrderByDescending(x => x.RunAt)
            .FirstOrDefault(x => x.Trigger == "Auto");

        var nextRun = PayrollAutoRunWorker.NextRunUtc(now, s);
        var (ny, nm) = PayrollAutoRunWorker.TargetPeriod(nextRun, s);

        return Ok(new PayrollAutoRunStatusResponse
        {
            Enabled    = s.Enabled,
            DayOfMonth = s.DayOfMonth,
            HourUtc    = s.HourUtc,
            Target     = s.Target,
            NextRunUtc = s.Enabled ? nextRun : null,
            NextPeriod = s.Enabled ? $"{ny:D4}-{nm:D2}" : null,
            LastRun    = last == null ? null : MapLog(last),
        });
    }

    private static PayrollRunLogResponse MapLog(PayrollRunLog l) => new()
    {
        Id                 = l.Id,
        PeriodYear         = l.PeriodYear,
        PeriodMonth        = l.PeriodMonth,
        Period             = $"{l.PeriodYear:D4}-{l.PeriodMonth:D2}",
        Trigger            = l.Trigger,
        Status             = l.Status,
        RunAt              = l.RunAt,
        DurationMs         = l.DurationMs,
        ExchangeRate       = l.ExchangeRate,
        Created            = l.Created,
        Refreshed          = l.Refreshed,
        SkippedPaid        = l.SkippedPaid,
        SkippedNotEmployed = l.SkippedNotEmployed,
        TotalGross         = l.TotalGross,
        TotalNet           = l.TotalNet,
        TotalTax           = l.TotalTax,
        TotalNssfEmployee  = l.TotalNssfEmployee,
        TotalNssfEmployer  = l.TotalNssfEmployer,
        Error              = l.Error,
    };

    private static string? Validate(PayrollRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmployeeCode))     return "Employee code is required";
        if (dto.PeriodYear < 2000 || dto.PeriodYear > 2100)  return "Period year is not valid";
        if (dto.PeriodMonth < 1 || dto.PeriodMonth > 12)     return "Period month must be 1-12";
        if (dto.BasicSalary < 0)                             return "Basic salary cannot be negative";
        if (dto.ExchangeRate is <= 0)                        return "Exchange rate must be greater than zero";
        if (string.IsNullOrWhiteSpace(dto.Status) || !Statuses.Contains(dto.Status))
            return $"Status must be one of: {string.Join(", ", Statuses)}";
        return null;
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Modules.HumanResources.Attendance;
using FarmingApi.Modules.HumanResources.Payroll;
using FarmingApi.Modules.Financials.ExchangeRate;

namespace FarmingApi.Modules.HumanResources.EmployeeMaster;

[ApiController]
[Route("[controller]")]
public class EmployeeMasterController : ControllerBase
{
    private readonly IEmployeeMasterRepository _repo;
    private readonly IPayrollRepository        _payroll;
    private readonly IAttendanceRepository     _attendance;
    private readonly IExchangeRateRepository   _rates;
    private readonly IMapper                   _mapper;

    public EmployeeMasterController(IEmployeeMasterRepository repo,
                                    IPayrollRepository payroll,
                                    IAttendanceRepository attendance,
                                    IExchangeRateRepository rates,
                                    IMapper mapper)
    {
        _repo       = repo;
        _payroll    = payroll;
        _attendance = attendance;
        _rates      = rates;
        _mapper     = mapper;
    }

    // ── GET /EmployeeMaster ────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? type = null, [FromQuery] bool? active = null)
    {
        var list = _repo.GetAll()
            .Where(x => type == null || x.EmployeeType == type)
            .Where(x => active == null || x.IsActive == active)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .ToList();
        return Ok(_mapper.Map<List<EmployeeMasterResponse>>(list));
    }

    // ── GET /EmployeeMaster/{id} ───────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<EmployeeMasterResponse>(e));
    }

    // ── GET /EmployeeMaster/ByCode/{code} ─────────────────────
    [HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var e = _repo.GetSingle(x => x.Code == code.Trim().ToUpper());
        if (e == null) return NotFound();
        return Ok(_mapper.Map<EmployeeMasterResponse>(e));
    }

    // ── POST /EmployeeMaster ───────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] EmployeeMasterRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Code))      return BadRequest("Code is required");
        if (string.IsNullOrWhiteSpace(dto.FirstName)) return BadRequest("First name is required");
        if (string.IsNullOrWhiteSpace(dto.LastName))  return BadRequest("Last name is required");

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Employee code '{code}' already exists");

        var entity       = _mapper.Map<EmployeeMaster>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();

        Provision(entity);

        return Ok(_mapper.Map<EmployeeMasterResponse>(entity));
    }

    // ── Auto-setup: give a new employee a current-month draft payslip
    //    and today's attendance row so they show up in both modules.
    private void Provision(EmployeeMaster emp)
    {
        var today = DateTime.UtcNow.Date;

        try
        {
            if (!_attendance.GetAll().Any(a => a.EmployeeCode == emp.Code && a.Date == today))
            {
                _attendance.Add(new Attendance.Attendance
                {
                    EmployeeCode = emp.Code,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    Date         = today,
                    Status       = "Present",
                    CreatedAt    = DateTime.UtcNow,
                    InActive     = false,
                });
                _attendance.Commit();
            }

            var y = today.Year;
            var m = today.Month;
            if (!_payroll.GetAll().Any(p => p.EmployeeCode == emp.Code
                                         && p.PeriodYear == y && p.PeriodMonth == m))
            {
                var basic = emp.Salary ?? 0m;
                var rate  = LatestUsdRate();
                var pay = new Payroll.Payroll
                {
                    EmployeeCode = emp.Code,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    PeriodYear   = y,
                    PeriodMonth  = m,
                    BasicSalary  = basic,
                    ExchangeRate = rate,
                    Dependents   = emp.Dependents,
                    Status       = "Draft",
                    CreatedAt    = DateTime.UtcNow,
                    InActive     = false,
                };

                // Seed the statutory lines so the draft is realistic before
                // anyone opens it; a full recompute happens on first save.
                var taxableKhr = basic * rate - emp.Dependents * PayrollMath.DependentReliefKhr;
                if (taxableKhr < 0m) taxableKhr = 0m;
                pay.TaxOnSalary = Math.Round(PayrollMath.TosKhr(taxableKhr) / rate, 2, MidpointRounding.AwayFromZero);

                var baseKhr = Math.Clamp(basic * rate, PayrollMath.NssfBaseMinKhr, PayrollMath.NssfBaseMaxKhr);
                pay.NssfEmployee = Math.Round(baseKhr * PayrollMath.NssfEmployeeRate / rate, 2, MidpointRounding.AwayFromZero);
                pay.NssfEmployer = Math.Round(baseKhr * PayrollMath.NssfEmployerRate / rate, 2, MidpointRounding.AwayFromZero);

                _payroll.Add(pay);
                _payroll.Commit();
            }
        }
        catch
        {
            // Provisioning is best-effort — never fail the employee create over it.
        }
    }

    private decimal LatestUsdRate()
    {
        var r = _rates.GetAll()
            .Where(x => x.Currency == "USD")
            .OrderByDescending(x => x.EffectiveDate)
            .FirstOrDefault();
        return r is { Rate: > 0 } ? r.Rate : 4100m;
    }

    // ── PUT /EmployeeMaster/{id} ───────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] EmployeeMasterRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Code))      return BadRequest("Code is required");
        if (string.IsNullOrWhiteSpace(dto.FirstName)) return BadRequest("First name is required");
        if (string.IsNullOrWhiteSpace(dto.LastName))  return BadRequest("Last name is required");

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Employee code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<EmployeeMasterResponse>(entity));
    }

    // ── DELETE /EmployeeMaster/{id} ───────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }
}

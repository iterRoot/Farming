using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Modules.HumanResources.EmployeeMaster;

namespace FarmingApi.Modules.HumanResources.Attendance;

[ApiController]
[Route("[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceRepository      _repo;
    private readonly IEmployeeMasterRepository  _employees;
    private readonly IMapper                    _mapper;

    private static readonly string[] Statuses =
        { "Present", "Absent", "Late", "HalfDay", "Leave", "Holiday" };

    public AttendanceController(IAttendanceRepository repo,
                                IEmployeeMasterRepository employees,
                                IMapper mapper)
    {
        _repo      = repo;
        _employees = employees;
        _mapper    = mapper;
    }

    // ── GET /Attendance ───────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? employeeCode = null,
                                [FromQuery] string? status = null,
                                [FromQuery] DateTime? from = null,
                                [FromQuery] DateTime? to = null)
    {
        var list = _repo.GetAll()
            .Where(x => employeeCode == null || x.EmployeeCode == employeeCode)
            .Where(x => status == null || x.Status == status)
            .Where(x => from == null || x.Date >= from)
            .Where(x => to == null || x.Date <= to)
            .OrderByDescending(x => x.Date).ThenBy(x => x.EmployeeCode)
            .ToList();
        return Ok(_mapper.Map<List<AttendanceResponse>>(list));
    }

    // ── GET /Attendance/{id} ─────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<AttendanceResponse>(e));
    }

    // ── GET /Attendance/Summary?employeeCode=&year=&month= ────
    // Monthly roll-up used by the Payroll calculation and the tracker view.
    [HttpGet("Summary")]
    public IActionResult Summary([FromQuery] string employeeCode,
                                 [FromQuery] int year, [FromQuery] int month)
    {
        if (string.IsNullOrWhiteSpace(employeeCode)) return BadRequest("employeeCode is required");
        if (year < 2000 || year > 2100) return BadRequest("year is not valid");
        if (month < 1 || month > 12)    return BadRequest("month must be 1-12");

        var rows = _repo.GetAll()
            .Where(x => x.EmployeeCode == employeeCode.Trim().ToUpper())
            .ToList();
        return Ok(AttendanceMath.Summarize(rows, employeeCode, year, month));
    }

    // ═══════════════════════════════════════════════════════════
    // GET /Attendance/Roster?date=YYYY-MM-DD
    // Every active employee for one day, whether or not attendance
    // has been recorded — the sheet you actually fill in.
    // ═══════════════════════════════════════════════════════════
    [HttpGet("Roster")]
    public IActionResult Roster([FromQuery] DateTime? date = null)
    {
        var day = (date ?? DateTime.UtcNow).Date;

        var employees = EmployedOn(day, day);
        var recorded  = _repo.GetAll().Where(a => a.Date == day).ToList();

        var rows = employees.Select(e =>
        {
            var hit = recorded.FirstOrDefault(a =>
                string.Equals(a.EmployeeCode, e.Code, StringComparison.OrdinalIgnoreCase));

            return new AttendanceRosterRow
            {
                AttendanceId  = hit?.Id,
                Recorded      = hit != null,
                EmployeeCode  = e.Code,
                EmployeeName  = $"{e.FirstName} {e.LastName}".Trim(),
                JobTitle      = e.JobTitle,
                Department    = e.Department,
                Status        = hit?.Status ?? "Present",
                CheckIn       = hit?.CheckIn,
                CheckOut      = hit?.CheckOut,
                HoursWorked   = hit?.HoursWorked,
                OvertimeHours = hit?.OvertimeHours,
                LeaveType     = hit?.LeaveType,
                Remarks       = hit?.Remarks,
            };
        }).ToList();

        return Ok(new AttendanceRosterResponse
        {
            Date           = day,
            TotalEmployees = rows.Count,
            Recorded       = rows.Count(r => r.Recorded),
            NotRecorded    = rows.Count(r => !r.Recorded),
            StatusCounts   = rows.Where(r => r.Recorded)
                                 .GroupBy(r => r.Status)
                                 .ToDictionary(g => g.Key, g => g.Count()),
            Rows           = rows,
        });
    }

    // ═══════════════════════════════════════════════════════════
    // POST /Attendance/Bulk — save the whole day in one call.
    // Upserts on (EmployeeCode, Date); Clear=true removes a record.
    // All-or-nothing on validation: one bad row rejects the batch,
    // so a half-saved day can't happen.
    // ═══════════════════════════════════════════════════════════
    [HttpPost("Bulk")]
    public IActionResult Bulk([FromBody] AttendanceBulkRequest dto)
    {
        if (dto == null)             return BadRequest("No data supplied.");
        if (dto.Date == default)     return BadRequest("Date is required");
        if (dto.Rows.Count == 0)     return BadRequest("No rows supplied.");

        var day = dto.Date.Date;
        var result = new AttendanceBulkResponse { Date = day };

        // ── validate everything first ──
        var known = _employees.GetAll().Select(e => e.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in dto.Rows)
        {
            var code = (r.EmployeeCode ?? "").Trim().ToUpperInvariant();
            if (code.Length == 0)     { result.Errors.Add("A row has no employee code."); continue; }
            if (!known.Contains(code)) result.Errors.Add($"{code}: no such employee.");
            if (!seen.Add(code))       result.Errors.Add($"{code}: listed more than once.");
            if (!r.Clear && !Statuses.Contains(r.Status))
                result.Errors.Add($"{code}: status must be one of {string.Join(", ", Statuses)}.");
        }

        if (result.Errors.Count > 0) return BadRequest(result);

        // ── then write ──
        var existing = _repo.GetAll().Where(a => a.Date == day).ToList();

        foreach (var r in dto.Rows)
        {
            var code = r.EmployeeCode.Trim().ToUpperInvariant();
            var hit  = existing.FirstOrDefault(a =>
                string.Equals(a.EmployeeCode, code, StringComparison.OrdinalIgnoreCase));

            if (r.Clear)
            {
                if (hit != null) { _repo.Remove(hit); result.Cleared++; }
                continue;
            }

            if (hit == null)
            {
                _repo.Add(new Attendance
                {
                    EmployeeCode  = code,
                    EmployeeName  = r.EmployeeName,
                    Date          = day,
                    Status        = r.Status,
                    CheckIn       = r.CheckIn,
                    CheckOut      = r.CheckOut,
                    HoursWorked   = r.HoursWorked,
                    OvertimeHours = r.OvertimeHours,
                    LeaveType     = r.LeaveType,
                    Remarks       = r.Remarks,
                    CreatedAt     = DateTime.UtcNow,
                    InActive      = false,
                });
                result.Created++;
            }
            else
            {
                hit.EmployeeName  = r.EmployeeName ?? hit.EmployeeName;
                hit.Status        = r.Status;
                hit.CheckIn       = r.CheckIn;
                hit.CheckOut      = r.CheckOut;
                hit.HoursWorked   = r.HoursWorked;
                hit.OvertimeHours = r.OvertimeHours;
                hit.LeaveType     = r.LeaveType;
                hit.Remarks       = r.Remarks;
                hit.UpdatedAt     = DateTime.UtcNow;
                _repo.Update(hit);
                result.Updated++;
            }
        }

        _repo.Commit();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════
    // GET /Attendance/Monthly?year=&month=
    // Every employee × every day, plus the per-employee totals that
    // Payroll uses. This is the tracker view.
    // ═══════════════════════════════════════════════════════════
    [HttpGet("Monthly")]
    public IActionResult Monthly([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;

        if (y < 2000 || y > 2100) return BadRequest("year is not valid");
        if (m < 1 || m > 12)      return BadRequest("month must be 1-12");

        var daysInMonth = DateTime.DaysInMonth(y, m);
        var start = new DateTime(y, m, 1);
        var end   = start.AddDays(daysInMonth - 1);

        var employees = EmployedOn(start, end);
        var codes     = employees.Select(e => e.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var all = _repo.GetAll()
            .Where(a => a.Date >= start && a.Date <= end)
            .ToList()
            .Where(a => codes.Contains(a.EmployeeCode))
            .ToList();

        var rows = employees.Select(e =>
        {
            var mine = all.Where(a =>
                string.Equals(a.EmployeeCode, e.Code, StringComparison.OrdinalIgnoreCase)).ToList();

            var days = new List<string>(daysInMonth);
            for (var d = 1; d <= daysInMonth; d++)
            {
                var hit = mine.FirstOrDefault(a => a.Date.Day == d);
                days.Add(hit == null ? "" : AttendanceMath.DayCode(hit));
            }

            return new AttendanceMonthlyRow
            {
                EmployeeCode = e.Code,
                EmployeeName = $"{e.FirstName} {e.LastName}".Trim(),
                Department   = e.Department,
                Days         = days,
                Summary      = AttendanceMath.Summarize(mine, e.Code, y, m),
            };
        }).ToList();

        return Ok(new AttendanceMonthlyResponse
        {
            Year               = y,
            Month              = m,
            Period             = $"{y:D4}-{m:D2}",
            DaysInMonth        = daysInMonth,
            TotalEmployees     = rows.Count,
            TotalRecordedDays  = rows.Sum(r => r.Summary.RecordedDays),
            TotalOvertimeHours = rows.Sum(r => r.Summary.TotalOvertimeHours),
            TotalUnpaidDays    = rows.Sum(r => r.Summary.UnpaidDays),
            Rows               = rows,
        });
    }

    /// <summary>Active employees whose employment overlaps the window.</summary>
    private List<EmployeeMaster.EmployeeMaster> EmployedOn(DateTime from, DateTime to) =>
        _employees.GetAll()
            .Where(e => e.IsActive)
            .Where(e => e.HireDate == null || e.HireDate <= to)
            .Where(e => e.TerminationDate == null || e.TerminationDate >= from)
            .OrderBy(e => e.Code)
            .ToList();

    // ── POST /Attendance ─────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] AttendanceRequest dto)
    {
        var check = Validate(dto);
        if (check != null) return BadRequest(check);

        var code = dto.EmployeeCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.EmployeeCode == code && x.Date.Date == dto.Date.Date))
            return BadRequest($"An attendance record for '{code}' on {dto.Date:yyyy-MM-dd} already exists");

        var entity          = _mapper.Map<Attendance>(dto);
        entity.EmployeeCode = code;
        entity.Date         = dto.Date.Date;
        entity.CreatedAt    = DateTime.UtcNow;
        entity.InActive     = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<AttendanceResponse>(entity));
    }

    // ── PUT /Attendance/{id} ─────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] AttendanceRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var check = Validate(dto);
        if (check != null) return BadRequest(check);

        var code = dto.EmployeeCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.EmployeeCode == code && x.Date.Date == dto.Date.Date && x.Id != id))
            return BadRequest($"An attendance record for '{code}' on {dto.Date:yyyy-MM-dd} already exists");

        _mapper.Map(dto, entity);
        entity.EmployeeCode = code;
        entity.Date         = dto.Date.Date;
        entity.UpdatedAt    = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<AttendanceResponse>(entity));
    }

    // ── DELETE /Attendance/{id} ─────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    private static string? Validate(AttendanceRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.EmployeeCode)) return "Employee code is required";
        if (dto.Date == default)                          return "Date is required";
        if (string.IsNullOrWhiteSpace(dto.Status) || !Statuses.Contains(dto.Status))
            return $"Status must be one of: {string.Join(", ", Statuses)}";
        return null;
    }
}

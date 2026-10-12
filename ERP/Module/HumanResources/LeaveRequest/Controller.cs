using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Modules.HumanResources.Attendance;
using FarmingApi.Modules.HumanResources.EmployeeMaster;

namespace FarmingApi.Modules.HumanResources.LeaveRequest;

[ApiController]
[Route("[controller]")]
public class LeaveRequestController : ControllerBase
{
    private readonly ILeaveRequestRepository   _repo;
    private readonly IEmployeeMasterRepository _employees;
    private readonly IAttendanceRepository     _attendance;
    private readonly IMapper                   _mapper;

    public LeaveRequestController(ILeaveRequestRepository repo,
                                  IEmployeeMasterRepository employees,
                                  IAttendanceRepository attendance,
                                  IMapper mapper)
    {
        _repo       = repo;
        _employees  = employees;
        _attendance = attendance;
        _mapper     = mapper;
    }

    // ── GET /LeaveRequest ─────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? employeeCode = null,
                                [FromQuery] string? status = null,
                                [FromQuery] string? leaveType = null,
                                [FromQuery] int? year = null)
    {
        var list = _repo.GetAll()
            .Where(x => employeeCode == null || x.EmployeeCode == employeeCode)
            .Where(x => status == null || x.Status == status)
            .Where(x => leaveType == null || x.LeaveType == leaveType)
            .Where(x => year == null || x.FromDate.Year == year)
            .OrderByDescending(x => x.FromDate).ThenByDescending(x => x.Id)
            .ToList();
        return Ok(_mapper.Map<List<LeaveRequestResponse>>(list));
    }

    // ── GET /LeaveRequest/{id} ───────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<LeaveRequestResponse>(e));
    }

    // ── POST /LeaveRequest ───────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] LeaveRequestRequest dto)
    {
        var check = Validate(dto, null);
        if (check != null) return BadRequest(check);

        var code = dto.EmployeeCode.Trim().ToUpper();
        var emp  = _employees.GetSingle(e => e.Code == code);

        var entity = _mapper.Map<LeaveRequest>(dto);
        entity.EmployeeCode = code;
        entity.EmployeeName = dto.EmployeeName
                           ?? (emp == null ? null : $"{emp.FirstName} {emp.LastName}".Trim());
        entity.FromDate     = dto.FromDate.Date;
        entity.ToDate       = dto.ToDate.Date;
        entity.Days         = CountDays(dto.FromDate, dto.ToDate, dto.HalfDay);
        entity.RequestNo    = _repo.NextRequestNo();
        entity.Status       = "Pending";
        entity.CreatedAt    = DateTime.UtcNow;
        entity.InActive     = false;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<LeaveRequestResponse>(entity));
    }

    // ── PUT /LeaveRequest/{id} — only while still Pending ────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] LeaveRequestRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status != "Pending")
            return BadRequest($"This request is {entity.Status} — only a Pending request can be edited.");

        var check = Validate(dto, id);
        if (check != null) return BadRequest(check);

        var code = dto.EmployeeCode.Trim().ToUpper();
        var emp  = _employees.GetSingle(e => e.Code == code);

        _mapper.Map(dto, entity);
        entity.EmployeeCode = code;
        entity.EmployeeName = dto.EmployeeName
                           ?? (emp == null ? null : $"{emp.FirstName} {emp.LastName}".Trim());
        entity.FromDate  = dto.FromDate.Date;
        entity.ToDate    = dto.ToDate.Date;
        entity.Days      = CountDays(dto.FromDate, dto.ToDate, dto.HalfDay);
        entity.UpdatedAt = DateTime.UtcNow;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<LeaveRequestResponse>(entity));
    }

    // ═══════════════════════════════════════════════════════════
    // POST /LeaveRequest/{id}/Approve
    // Approving is the point where a request becomes real: the leave
    // days are written into Attendance, so the monthly tracker shows
    // them and Payroll deducts unpaid ones automatically.
    // ═══════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Approve")]
    public IActionResult Approve(int id, [FromBody] LeaveDecisionRequest? dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status == "Approved") return BadRequest("This request is already approved.");
        if (entity.Status != "Pending")
            return BadRequest($"This request is {entity.Status} and can no longer be approved.");

        var result = new LeaveDecisionResponse();
        var days   = LeaveMath.WorkingDays(entity.FromDate, entity.ToDate);

        var existing = _attendance.GetAll()
            .Where(a => a.EmployeeCode == entity.EmployeeCode)
            .ToList()
            .Where(a => days.Contains(a.Date.Date))
            .ToList();

        foreach (var day in days)
        {
            var hit = existing.FirstOrDefault(a => a.Date.Date == day);

            if (hit == null)
            {
                _attendance.Add(new Attendance.Attendance
                {
                    EmployeeCode = entity.EmployeeCode,
                    EmployeeName = entity.EmployeeName,
                    Date         = day,
                    Status       = entity.HalfDay ? "HalfDay" : "Leave",
                    LeaveType    = entity.LeaveType,
                    Remarks      = $"Leave {entity.RequestNo}",
                    CreatedAt    = DateTime.UtcNow,
                    InActive     = false,
                });
                result.AttendanceWritten++;
            }
            else
            {
                // A day already recorded is overwritten with the approved
                // leave, and we say so rather than silently changing history.
                result.Notes.Add(
                    $"{day:yyyy-MM-dd}: replaced existing '{hit.Status}' record with approved leave.");
                hit.Status    = entity.HalfDay ? "HalfDay" : "Leave";
                hit.LeaveType = entity.LeaveType;
                hit.Remarks   = $"Leave {entity.RequestNo}";
                hit.UpdatedAt = DateTime.UtcNow;
                _attendance.Update(hit);
                result.AttendanceWritten++;
            }
        }
        _attendance.Commit();

        entity.Status       = "Approved";
        entity.DecidedBy    = dto?.DecidedBy;
        entity.DecidedAt    = DateTime.UtcNow;
        entity.DecisionNote = dto?.Note;
        entity.UpdatedAt    = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();

        if (!LeaveMath.IsPaid(entity.LeaveType))
            result.Notes.Add(
                $"Unpaid leave: {entity.Days} day(s) will be deducted on the {entity.FromDate:yyyy-MM} payroll run.");

        result.Request = _mapper.Map<LeaveRequestResponse>(entity);
        return Ok(result);
    }

    // ── POST /LeaveRequest/{id}/Reject ───────────────────────
    [HttpPost("{id:int}/Reject")]
    public IActionResult Reject(int id, [FromBody] LeaveDecisionRequest? dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status != "Pending")
            return BadRequest($"This request is {entity.Status} and can no longer be rejected.");

        entity.Status       = "Rejected";
        entity.DecidedBy    = dto?.DecidedBy;
        entity.DecidedAt    = DateTime.UtcNow;
        entity.DecisionNote = dto?.Note;
        entity.UpdatedAt    = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();

        return Ok(new LeaveDecisionResponse
        {
            Request = _mapper.Map<LeaveRequestResponse>(entity),
        });
    }

    // ═══════════════════════════════════════════════════════════
    // POST /LeaveRequest/{id}/Cancel
    // Cancelling an approved request takes the leave days back out of
    // the calendar, so payroll stops deducting for them.
    // ═══════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id, [FromBody] LeaveDecisionRequest? dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status == "Cancelled") return BadRequest("This request is already cancelled.");
        if (entity.Status == "Rejected")  return BadRequest("A rejected request cannot be cancelled.");

        var result = new LeaveDecisionResponse();

        if (entity.Status == "Approved")
        {
            var days = LeaveMath.WorkingDays(entity.FromDate, entity.ToDate);
            var mine = _attendance.GetAll()
                .Where(a => a.EmployeeCode == entity.EmployeeCode)
                .ToList()
                // Only remove rows this request created — matched by its stamp.
                .Where(a => days.Contains(a.Date.Date) && a.Remarks == $"Leave {entity.RequestNo}")
                .ToList();

            foreach (var a in mine) { _attendance.Remove(a); result.AttendanceRemoved++; }
            _attendance.Commit();

            var untouched = days.Count - result.AttendanceRemoved;
            if (untouched > 0)
                result.Notes.Add(
                    $"{untouched} day(s) were edited after approval and were left as they are.");
        }

        entity.Status       = "Cancelled";
        entity.DecidedBy    = dto?.DecidedBy ?? entity.DecidedBy;
        entity.DecidedAt    = DateTime.UtcNow;
        entity.DecisionNote = dto?.Note ?? entity.DecisionNote;
        entity.UpdatedAt    = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();

        result.Request = _mapper.Map<LeaveRequestResponse>(entity);
        return Ok(result);
    }

    // ── DELETE /LeaveRequest/{id} ────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (entity.Status == "Approved")
            return BadRequest("Cancel the request first — deleting it would leave the approved days on the calendar.");

        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── GET /LeaveRequest/Balance?employeeCode=&year= ────────
    [HttpGet("Balance")]
    public IActionResult Balance([FromQuery] string employeeCode, [FromQuery] int? year = null)
    {
        if (string.IsNullOrWhiteSpace(employeeCode)) return BadRequest("employeeCode is required");

        var code = employeeCode.Trim().ToUpper();
        var y    = year ?? DateTime.UtcNow.Year;
        var emp  = _employees.GetSingle(e => e.Code == code);

        var mine = _repo.GetAll()
            .Where(r => r.EmployeeCode == code && r.FromDate.Year == y)
            .ToList();

        decimal Sum(string type, string status) => mine
            .Where(r => r.LeaveType == type && r.Status == status)
            .Sum(r => r.Days);

        var taken   = Sum("Annual", "Approved");
        var pending = Sum("Annual", "Pending");

        return Ok(new LeaveBalanceResponse
        {
            EmployeeCode    = code,
            EmployeeName    = emp == null ? null : $"{emp.FirstName} {emp.LastName}".Trim(),
            Year            = y,
            Entitlement     = LeaveMath.AnnualEntitlementDays,
            AnnualTaken     = taken,
            AnnualPending   = pending,
            AnnualRemaining = LeaveMath.AnnualEntitlementDays - taken - pending,
            SickTaken       = Sum("Sick", "Approved"),
            UnpaidTaken     = Sum("Unpaid", "Approved"),
            OtherTaken      = Sum("Maternity", "Approved") + Sum("Other", "Approved"),
        });
    }

    // ═══════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════
    private static decimal CountDays(DateTime from, DateTime to, bool halfDay)
    {
        var working = LeaveMath.WorkingDays(from, to).Count;
        return halfDay ? working * 0.5m : working;
    }

    private string? Validate(LeaveRequestRequest dto, int? ignoreId)
    {
        if (dto == null) return "No request supplied.";
        if (string.IsNullOrWhiteSpace(dto.EmployeeCode)) return "Employee is required";
        if (dto.FromDate == default || dto.ToDate == default) return "From and To dates are required";
        if (dto.ToDate.Date < dto.FromDate.Date) return "The To date cannot be before the From date";
        if (!LeaveMath.Types.Contains(dto.LeaveType))
            return $"Leave type must be one of: {string.Join(", ", LeaveMath.Types)}";

        var code = dto.EmployeeCode.Trim().ToUpper();
        if (_employees.GetSingle(e => e.Code == code) == null)
            return $"No such employee: {code}";

        if (CountDays(dto.FromDate, dto.ToDate, false) == 0)
            return "That range contains no working days (Sundays are not counted).";

        if (dto.HalfDay && dto.FromDate.Date != dto.ToDate.Date)
            return "A half day applies to a single date — set From and To to the same day.";

        // Overlap with another live request for the same person.
        var clash = _repo.GetAll()
            .Where(r => r.EmployeeCode == code)
            .Where(r => r.Status == "Pending" || r.Status == "Approved")
            .Where(r => ignoreId == null || r.Id != ignoreId)
            .ToList()
            .FirstOrDefault(r => r.FromDate.Date <= dto.ToDate.Date
                              && r.ToDate.Date   >= dto.FromDate.Date);

        if (clash != null)
            return $"Overlaps {clash.RequestNo} ({clash.Status}, " +
                   $"{clash.FromDate:yyyy-MM-dd} to {clash.ToDate:yyyy-MM-dd}).";

        return null;
    }
}

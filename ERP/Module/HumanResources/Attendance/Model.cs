namespace FarmingApi.Modules.HumanResources.Attendance;

public class AttendanceResponse
{
    public int      Id            { get; set; }
    public string   EmployeeCode  { get; set; } = null!;
    public string?  EmployeeName  { get; set; }
    public DateTime Date          { get; set; }
    public string   Status        { get; set; } = null!;
    public string?  CheckIn       { get; set; }
    public string?  CheckOut      { get; set; }
    public decimal? HoursWorked   { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string?  LeaveType     { get; set; }
    public string?  Remarks       { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime? UpdatedAt    { get; set; }
}

public class AttendanceRequest
{
    public string   EmployeeCode  { get; set; } = null!;
    public string?  EmployeeName  { get; set; }
    public DateTime Date          { get; set; }
    public string   Status        { get; set; } = "Present";
    public string?  CheckIn       { get; set; }
    public string?  CheckOut      { get; set; }
    public decimal? HoursWorked   { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string?  LeaveType     { get; set; }
    public string?  Remarks       { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// DAILY ROSTER — every active employee for one date, recorded or not.
// This is how attendance is actually taken: one screen, whole team.
// ═══════════════════════════════════════════════════════════════════
public class AttendanceRosterRow
{
    public int?     AttendanceId  { get; set; }   // null → nothing recorded yet
    public bool     Recorded      { get; set; }
    public string   EmployeeCode  { get; set; } = null!;
    public string   EmployeeName  { get; set; } = null!;
    public string?  JobTitle      { get; set; }
    public string?  Department    { get; set; }
    public string   Status        { get; set; } = "Present";
    public string?  CheckIn       { get; set; }
    public string?  CheckOut      { get; set; }
    public decimal? HoursWorked   { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string?  LeaveType     { get; set; }
    public string?  Remarks       { get; set; }
}

public class AttendanceRosterResponse
{
    public DateTime Date           { get; set; }
    public int      TotalEmployees { get; set; }
    public int      Recorded       { get; set; }
    public int      NotRecorded    { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
    public List<AttendanceRosterRow> Rows { get; set; } = new();
}

// ── Bulk save: one POST for the whole day ──────────────────────────
public class AttendanceBulkRow
{
    public string   EmployeeCode  { get; set; } = null!;
    public string?  EmployeeName  { get; set; }
    public string   Status        { get; set; } = "Present";
    public string?  CheckIn       { get; set; }
    public string?  CheckOut      { get; set; }
    public decimal? HoursWorked   { get; set; }
    public decimal? OvertimeHours { get; set; }
    public string?  LeaveType     { get; set; }
    public string?  Remarks       { get; set; }
    /// <summary>True → delete any existing record for this employee/date instead of saving one.</summary>
    public bool     Clear         { get; set; }
}

public class AttendanceBulkRequest
{
    public DateTime Date { get; set; }
    public List<AttendanceBulkRow> Rows { get; set; } = new();
}

public class AttendanceBulkResponse
{
    public DateTime     Date    { get; set; }
    public int          Created { get; set; }
    public int          Updated { get; set; }
    public int          Cleared { get; set; }
    public List<string> Errors  { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// MONTHLY TRACKER — every employee × every day of the month.
// Day codes: P present · L late · H half day · A absent
//            V paid leave · U unpaid leave · O holiday · "" not recorded
// ═══════════════════════════════════════════════════════════════════
public class AttendanceMonthlyRow
{
    public string   EmployeeCode { get; set; } = null!;
    public string   EmployeeName { get; set; } = null!;
    public string?  Department   { get; set; }
    /// <summary>One entry per day of the month, index 0 = day 1.</summary>
    public List<string> Days     { get; set; } = new();
    public AttendanceSummaryResponse Summary { get; set; } = null!;
}

public class AttendanceMonthlyResponse
{
    public int    Year        { get; set; }
    public int    Month       { get; set; }
    public string Period      { get; set; } = null!;
    public int    DaysInMonth { get; set; }
    public int    TotalEmployees { get; set; }

    public int     TotalRecordedDays  { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal TotalUnpaidDays    { get; set; }

    public List<AttendanceMonthlyRow> Rows { get; set; } = new();
}

// ── Monthly roll-up for one employee — feeds the Payroll calculation ──
public class AttendanceSummaryResponse
{
    public string EmployeeCode      { get; set; } = null!;
    public int    Year              { get; set; }
    public int    Month             { get; set; }

    public int     DaysPresent      { get; set; }
    public int     DaysLate         { get; set; }
    public int     DaysHalfDay      { get; set; }
    public int     DaysAbsent       { get; set; }
    public int     DaysLeavePaid    { get; set; }   // Leave, type != "Unpaid"
    public int     DaysLeaveUnpaid  { get; set; }   // Leave, type == "Unpaid"
    public int     DaysHoliday      { get; set; }
    public int     RecordedDays     { get; set; }

    public decimal TotalHoursWorked   { get; set; }
    public decimal TotalOvertimeHours { get; set; }

    /// <summary>Absent + unpaid leave + half of each half-day — drives the unpaid-leave deduction.</summary>
    public decimal UnpaidDays        { get; set; }
}

public static class AttendanceMath
{
    public static AttendanceSummaryResponse Summarize(
        IEnumerable<Attendance> rows, string employeeCode, int year, int month)
    {
        var list = rows.Where(r =>
            string.Equals(r.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase)
            && r.Date.Year == year && r.Date.Month == month).ToList();

        int Count(string s) => list.Count(r => r.Status == s);

        var s = new AttendanceSummaryResponse
        {
            EmployeeCode       = employeeCode.ToUpperInvariant(),
            Year               = year,
            Month              = month,
            DaysPresent        = Count("Present"),
            DaysLate           = Count("Late"),
            DaysHalfDay        = Count("HalfDay"),
            DaysAbsent         = Count("Absent"),
            DaysHoliday        = Count("Holiday"),
            DaysLeavePaid      = list.Count(r => r.Status == "Leave"
                                    && !string.Equals(r.LeaveType, "Unpaid", StringComparison.OrdinalIgnoreCase)),
            DaysLeaveUnpaid    = list.Count(r => r.Status == "Leave"
                                    && string.Equals(r.LeaveType, "Unpaid", StringComparison.OrdinalIgnoreCase)),
            RecordedDays       = list.Count,
            TotalHoursWorked   = list.Sum(r => r.HoursWorked ?? 0m),
            TotalOvertimeHours = list.Sum(r => r.OvertimeHours ?? 0m),
        };

        s.UnpaidDays = s.DaysAbsent + s.DaysLeaveUnpaid + 0.5m * s.DaysHalfDay;
        return s;
    }

    /// <summary>Single-letter code for the monthly tracker grid.</summary>
    public static string DayCode(Attendance a) => a.Status switch
    {
        "Present"  => "P",
        "Late"     => "L",
        "HalfDay"  => "H",
        "Absent"   => "A",
        "Holiday"  => "O",
        "Leave"    => string.Equals(a.LeaveType, "Unpaid", StringComparison.OrdinalIgnoreCase) ? "U" : "V",
        _          => "?",
    };
}

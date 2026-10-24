namespace FarmingApi.Modules.HumanResources.LeaveRequest;

public class LeaveRequestResponse
{
    public int       Id           { get; set; }
    public string    RequestNo    { get; set; } = null!;
    public string    EmployeeCode { get; set; } = null!;
    public string?   EmployeeName { get; set; }
    public string    LeaveType    { get; set; } = null!;
    public bool      Paid         { get; set; }
    public DateTime  FromDate     { get; set; }
    public DateTime  ToDate       { get; set; }
    public decimal   Days         { get; set; }
    public bool      HalfDay      { get; set; }
    public string?   Reason       { get; set; }
    public string    Status       { get; set; } = null!;
    public string?   DecidedBy    { get; set; }
    public DateTime? DecidedAt    { get; set; }
    public string?   DecisionNote { get; set; }
    public DateTime  CreatedAt    { get; set; }
    public DateTime? UpdatedAt    { get; set; }
}

public class LeaveRequestRequest
{
    public string   EmployeeCode { get; set; } = null!;
    public string?  EmployeeName { get; set; }
    public string   LeaveType    { get; set; } = "Annual";
    public DateTime FromDate     { get; set; }
    public DateTime ToDate       { get; set; }
    public bool     HalfDay      { get; set; }
    public string?  Reason       { get; set; }
}

/// <summary>Approve / Reject / Cancel payload.</summary>
public class LeaveDecisionRequest
{
    public string? DecidedBy { get; set; }
    public string? Note      { get; set; }
}

/// <summary>What approving actually did to the attendance calendar.</summary>
public class LeaveDecisionResponse
{
    public LeaveRequestResponse Request { get; set; } = null!;
    public int    AttendanceWritten { get; set; }
    public int    AttendanceRemoved { get; set; }
    public List<string> Notes       { get; set; } = new();
}

public class LeaveBalanceResponse
{
    public string  EmployeeCode  { get; set; } = null!;
    public string? EmployeeName  { get; set; }
    public int     Year          { get; set; }
    public decimal Entitlement   { get; set; }   // annual leave allowance
    public decimal AnnualTaken   { get; set; }   // approved Annual days this year
    public decimal AnnualPending { get; set; }   // still awaiting a decision
    public decimal AnnualRemaining { get; set; }
    public decimal SickTaken     { get; set; }
    public decimal UnpaidTaken   { get; set; }
    public decimal OtherTaken    { get; set; }
}

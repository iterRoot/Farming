namespace FarmingApi.Modules.HumanResources.Candidate;

public static class CandidateOptions
{
    public static readonly string[] Stages = { "Applied", "Screening", "Interviewing", "Offered", "Hired", "Rejected" };
}

public class CandidateResponse
{
    public int      Id              { get; set; }
    public string   CandidateNo     { get; set; } = null!;
    public int      JobPostingId    { get; set; }
    public string?  JobPostingTitle { get; set; }
    public string   FirstName       { get; set; } = null!;
    public string   LastName        { get; set; } = null!;
    public string?  Email           { get; set; }
    public string?  Phone           { get; set; }
    public string   Stage           { get; set; } = null!;
    public DateTime AppliedDate     { get; set; }
    public decimal? ExpectedSalary  { get; set; }
    public string?  Notes           { get; set; }
    public string?  RejectionReason { get; set; }
    public int?     EmployeeId      { get; set; }
    public string?  EmployeeCode    { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime? UpdatedAt      { get; set; }
}

public class CandidateRequest
{
    public int      JobPostingId   { get; set; }
    public string   FirstName      { get; set; } = null!;
    public string   LastName       { get; set; } = null!;
    public string?  Email          { get; set; }
    public string?  Phone          { get; set; }
    public decimal? ExpectedSalary { get; set; }
    public string?  Notes          { get; set; }
}

public class CandidateStageRequest
{
    public string  Stage           { get; set; } = null!;
    public string? RejectionReason { get; set; }
}

// Fields not already on the candidate that Employee Master requires.
public class HireRequest
{
    public string   EmployeeCode { get; set; } = null!;
    public decimal? Salary       { get; set; }
    public DateTime? HireDate    { get; set; }
    public string?  JobTitle     { get; set; }
    public string?  Department   { get; set; }
    public string   EmployeeType { get; set; } = "Permanent";
}

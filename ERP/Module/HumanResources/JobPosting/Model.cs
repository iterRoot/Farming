namespace FarmingApi.Modules.HumanResources.JobPosting;

public static class JobPostingOptions
{
    public static readonly string[] EmploymentTypes = { "Permanent", "Contract", "Temporary", "Intern" };
    public static readonly string[] Statuses = { "Open", "OnHold", "Closed", "Cancelled" };
}

public class JobPostingResponse
{
    public int      Id               { get; set; }
    public string   PostingNo        { get; set; } = null!;
    public string   Title            { get; set; } = null!;
    public string?  Department       { get; set; }
    public string   EmploymentType   { get; set; } = null!;
    public string   Status           { get; set; } = null!;
    public int      NumberOfOpenings { get; set; }
    public DateTime OpenDate         { get; set; }
    public DateTime? TargetCloseDate { get; set; }
    public string?  HiringManager    { get; set; }
    public string?  Description      { get; set; }
    public string?  Requirements     { get; set; }
    public string?  Remarks          { get; set; }
    public int      CandidateCount   { get; set; }
    public int      HiredCount       { get; set; }
    public DateTime CreatedAt        { get; set; }
    public DateTime? UpdatedAt       { get; set; }
}

public class JobPostingRequest
{
    public string   Title            { get; set; } = null!;
    public string?  Department       { get; set; }
    public string   EmploymentType   { get; set; } = "Permanent";
    public string   Status           { get; set; } = "Open";
    public int      NumberOfOpenings { get; set; } = 1;
    public DateTime? TargetCloseDate { get; set; }
    public string?  HiringManager    { get; set; }
    public string?  Description      { get; set; }
    public string?  Requirements     { get; set; }
    public string?  Remarks          { get; set; }
}

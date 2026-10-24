namespace FarmingApi.Modules.Administration.ImplementationCentre;

public class ImplementationTaskDto
{
    public int      Id            { get; set; }
    public string   Phase         { get; set; } = null!;
    public string   Title         { get; set; } = null!;
    public string?  Description   { get; set; }
    public string?  Responsible   { get; set; }
    public string   Status        { get; set; } = "Open";
    public string   Priority      { get; set; } = "Medium";
    public DateOnly? PlannedDate  { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string?  LinkPath      { get; set; }
    public string?  Notes         { get; set; }
    public int      SortOrder     { get; set; }
    public bool     InActive      { get; set; }
}

public class ImplementationTaskSaveRequest
{
    public string   Phase         { get; set; } = "Getting Started";
    public string   Title         { get; set; } = null!;
    public string?  Description   { get; set; }
    public string?  Responsible   { get; set; }
    public string   Status        { get; set; } = "Open";
    public string   Priority      { get; set; } = "Medium";
    public DateOnly? PlannedDate  { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string?  LinkPath      { get; set; }
    public string?  Notes         { get; set; }
    public int      SortOrder     { get; set; }
    public bool     InActive      { get; set; }
}

// Overview counters shown on the List page.
public class ImplementationSummary
{
    public int Total      { get; set; }
    public int Completed  { get; set; }
    public int InProcess  { get; set; }
    public int Open       { get; set; }
    public int Skipped    { get; set; }
    public int PercentComplete { get; set; }
}

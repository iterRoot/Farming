using System.Text.Json;
using System.Text.Json.Serialization;

namespace FarmingApi.Modules.ProjectManagement.Project;

// Row shape for the list grid.
public class ProjectListResponse
{
    public int       Id              { get; set; }
    public string    ProjectNo       { get; set; } = null!;
    public string    ProjectName     { get; set; } = null!;
    public string    ProjectType     { get; set; } = null!;
    public string    Status          { get; set; } = null!;
    public string?   BpCode          { get; set; }
    public string?   SalesEmployee   { get; set; }
    public DateTime? StartDate       { get; set; }
    public DateTime? DueDate         { get; set; }
    public decimal   PercentComplete { get; set; }
    public int       OpenActivities  { get; set; }
    public DateTime  CreatedAt       { get; set; }
}

// Full record for the editor. The nested grids come back as raw JSON nodes
// under the same names the create screen sends.
public class ProjectDetailResponse
{
    public int       Id               { get; set; }
    public string    ProjectNo        { get; set; } = null!;
    public string    ProjectName      { get; set; } = null!;
    public string    ProjectType      { get; set; } = null!;
    public string    Status           { get; set; } = null!;
    public string?   Series           { get; set; }
    public DateTime? StartDate        { get; set; }
    public DateTime? DueDate          { get; set; }
    public DateTime? FinishedDate     { get; set; }
    public string?   BpCode           { get; set; }
    public string?   BpName           { get; set; }
    public string?   ContactPerson    { get; set; }
    public string?   Territory        { get; set; }
    public string?   SalesEmployee    { get; set; }
    public string?   Owner            { get; set; }
    public bool      WithSubprojects  { get; set; }
    public string?   FinancialProject { get; set; }
    public string?   RiskLevel        { get; set; }
    public string?   Industry         { get; set; }
    public string?   Comments         { get; set; }
    public string?   Remarks          { get; set; }
    public int       OpenActivities   { get; set; }
    public decimal   PercentComplete  { get; set; }

    public JsonElement? Subprojects { get; set; }
    public List<ProjectStageDto> Stages { get; set; } = new();
    public JsonElement? Sections    { get; set; }
    public JsonElement? Attachments { get; set; }
}

public class ProjectStageDto
{
    public string    Stage           { get; set; } = "";
    public string?   Task            { get; set; }
    public string?   Description     { get; set; }
    public string?   UniqueId        { get; set; }
    public string?   Assignee        { get; set; }
    public DateTime? StartDate       { get; set; }
    public DateTime? DueDate         { get; set; }
    public DateTime? FinishedDate    { get; set; }
    public decimal   PlannedCost     { get; set; }
    public decimal   EstimatedHours  { get; set; }
    public decimal   PercentComplete { get; set; }
}

public class TimelineStage
{
    public int       LineNo          { get; set; }
    public string    Stage           { get; set; } = null!;
    public string?   Task            { get; set; }
    public string?   Assignee        { get; set; }
    public DateTime? StartDate       { get; set; }
    public DateTime? DueDate         { get; set; }
    public DateTime? FinishedDate    { get; set; }
    public decimal   PercentComplete { get; set; }
    public string    State           { get; set; } = null!;   // Done | Overdue | In progress | Not started
    public int       DaysLate        { get; set; }            // Overdue: days past due; Done: days finished after due
}

public class TimelineResponse
{
    public int       ProjectId       { get; set; }
    public string    ProjectNo       { get; set; } = null!;
    public string    Status          { get; set; } = null!;
    public DateTime  AsOf            { get; set; }
    public DateTime? StartDate       { get; set; }
    public DateTime? DueDate         { get; set; }
    public int?      PlannedDays     { get; set; }
    public int?      ElapsedDays     { get; set; }
    public decimal?  TimeElapsedPct  { get; set; }
    public decimal   PercentComplete { get; set; }
    public int       OpenActivities  { get; set; }
    public int       OverdueStages   { get; set; }
    public string    Schedule        { get; set; } = null!;   // Not started | On track | Behind | Overdue | Complete | Closed | Cancelled
    public decimal   PlannedCost     { get; set; }
    public List<TimelineStage> Stages { get; set; } = new();
}

public class ProjectRequest
{
    public string?   ProjectType      { get; set; }
    public string    ProjectName      { get; set; } = null!;
    public string?   Status           { get; set; }
    public string?   Series           { get; set; }
    public string?   ProjectNo        { get; set; }   // ignored — assigned server-side
    public DateTime? StartDate        { get; set; }
    public DateTime? DueDate          { get; set; }
    public DateTime? FinishedDate     { get; set; }
    public string?   BpCode           { get; set; }
    public string?   BpName           { get; set; }
    public string?   ContactPerson    { get; set; }
    public string?   Territory        { get; set; }
    public string?   SalesEmployee    { get; set; }
    public string?   Owner            { get; set; }
    public bool      WithSubprojects  { get; set; }
    public string?   FinancialProject { get; set; }
    public string?   RiskLevel        { get; set; }
    public string?   Industry         { get; set; }
    public string?   Comments         { get; set; }
    public string?   Remarks          { get; set; }
    public int       OpenActivities   { get; set; }
    public decimal   PercentComplete  { get; set; }

    public JsonElement? Subprojects { get; set; }
    public List<ProjectStageDto>? Stages { get; set; }
    public JsonElement? Sections    { get; set; }
    public JsonElement? Attachments { get; set; }
}

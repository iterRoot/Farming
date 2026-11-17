namespace FarmingApi.Modules.Administration.SetUp.ProcessChecklistTemplate;

// ── Requests ─────────────────────────────────────────────────────
public class ChecklistStepRequest
{
    public int     StepNo             { get; set; }
    public string  Title              { get; set; } = "";
    public string? Description        { get; set; }
    public string? StepType           { get; set; }
    public string? GroupName          { get; set; }
    public string? AssignedRole       { get; set; }
    public string? AssignedTo         { get; set; }
    public int?    EstimatedMinutes   { get; set; }   // a cleared input arrives as null
    public bool    IsRequired         { get; set; } = true;
    public bool    RequiresAttachment { get; set; }
    public bool    RequiresNote       { get; set; }
    public int?    DependsOnStepNo    { get; set; }
    public string? LinkedRoute        { get; set; }
    public string? Instructions       { get; set; }
    public string? AcceptanceCriteria { get; set; }
}

public class ChecklistTemplateSaveRequest
{
    public string  Code           { get; set; } = "";
    public string  Name           { get; set; } = "";
    public string? Category       { get; set; }
    public string? Module         { get; set; }
    public string? SubCategory    { get; set; }
    public string? Description    { get; set; }
    public string? TriggerType    { get; set; }
    public string? TriggerEvent   { get; set; }
    public string? LinkedDocType  { get; set; }
    public string? AssignedRole   { get; set; }
    public string? AssignedTo     { get; set; }
    public bool    RequireSignOff { get; set; }
    public string? SignOffRole    { get; set; }
    public string? Tags           { get; set; }
    public string? IconName       { get; set; }
    public string? Version        { get; set; }
    public bool    IsActive       { get; set; } = true;
    public int     SortOrder      { get; set; }
    public string? Remarks        { get; set; }
    public List<ChecklistStepRequest> Steps { get; set; } = new();
}

public class ChecklistStartRequest
{
    public string    Name       { get; set; } = "";
    public string?   ContextRef { get; set; }
    public string?   StartedBy  { get; set; }
    public DateTime? DueDate    { get; set; }
    public string?   Notes      { get; set; }
}

public class ChecklistStepUpdateRequest
{
    public string  StepStatus { get; set; } = "Done";   // Done | Skipped | Failed | Pending
    public string? Note       { get; set; }
}

// ── Responses ────────────────────────────────────────────────────
public class ChecklistStepDto : ChecklistStepRequest
{
    public int Id { get; set; }
}

public class ChecklistTemplateDto
{
    public int     Id             { get; set; }
    public string  Code           { get; set; } = null!;
    public string  Name           { get; set; } = null!;
    public string  Category       { get; set; } = null!;
    public string? Module         { get; set; }
    public string? SubCategory    { get; set; }
    public string? Description    { get; set; }
    public string  TriggerType    { get; set; } = null!;
    public string? TriggerEvent   { get; set; }
    public string? LinkedDocType  { get; set; }
    public string? AssignedRole   { get; set; }
    public string? AssignedTo     { get; set; }
    public bool    RequireSignOff { get; set; }
    public string? SignOffRole    { get; set; }
    public string? Tags           { get; set; }
    public string? IconName       { get; set; }
    public string? Version        { get; set; }
    public bool    IsActive       { get; set; }
    public int     SortOrder      { get; set; }
    public string? Remarks        { get; set; }
    public int     StepCount      { get; set; }
    public string  EstimatedTime  { get; set; } = "0m";  // "45m", "2h 10m"
    public int     TotalRunCount  { get; set; }
    public List<ChecklistStepDto>? Steps { get; set; }   // only on GET /{id}
}

public class ChecklistRunStepDto
{
    public int       Id              { get; set; }
    public int       StepNo          { get; set; }
    public string    Title           { get; set; } = null!;
    public string    StepType        { get; set; } = null!;
    public string?   GroupName       { get; set; }
    public bool      IsRequired      { get; set; }
    public int?      DependsOnStepNo { get; set; }
    public string?   LinkedRoute     { get; set; }
    public string    StepStatus      { get; set; } = null!;
    public string?   CompletedBy     { get; set; }
    public DateTime? CompletedAt     { get; set; }
    public string?   Note            { get; set; }
}

public class ChecklistRunDto
{
    public int       Id                  { get; set; }
    public string    RunNo               { get; set; } = null!;
    public int       ChecklistTemplateId { get; set; }
    public string    TemplateCode        { get; set; } = null!;
    public string    Name                { get; set; } = null!;
    public string?   ContextRef          { get; set; }
    public string?   StartedBy           { get; set; }
    public DateTime? DueDate             { get; set; }
    public string?   Notes               { get; set; }
    public string    Status              { get; set; } = null!;
    public DateTime  StartedAt           { get; set; }
    public DateTime? CompletedAt         { get; set; }
    public bool      RequireSignOff      { get; set; }
    public string?   SignOffRole         { get; set; }
    public string?   SignedOffBy         { get; set; }
    public DateTime? SignedOffAt         { get; set; }
    public int       TotalSteps          { get; set; }
    public int       CompletedSteps      { get; set; }   // Done + Skipped
    public int       ProgressPct         { get; set; }
    public List<ChecklistRunStepDto>? StepResults { get; set; }   // on GET /Runs/{id} and run actions
}

public class ChecklistSummary
{
    public int Total         { get; set; }
    public int Active        { get; set; }
    public int ActiveRuns    { get; set; }
    public int CompletedRuns { get; set; }
    public Dictionary<string, int> ByCategory { get; set; } = new();
}

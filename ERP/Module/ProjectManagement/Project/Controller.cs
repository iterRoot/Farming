using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.FinancialProjects;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.ProjectManagement.Project;

// ═══════════════════════════════════════════════════════════════════
// PROJECT  (route: /Project, also reached as /projects)
// GET    /projects                 → list rows
// GET    /projects/{id}            → full record (flat fields + stages + nested grids)
// POST   /projects                 → create
// PUT    /projects/{id}            → update
// POST   /projects/{id}/cancel     → set status Cancelled
// GET    /projects/{id}/Timeline   → schedule: per-stage state, overdue, time vs progress
// GET    /projects/{id}/Budget     → budget vs actual from the linked financial project
// DELETE /projects/{id}
//
// Stages are real rows (KPRS). The project's Percent Complete (weighted by
// estimated hours, 1 when a stage has none) and Open Activities (stages not
// finished) are derived from them and can't be typed in. The remaining nested
// grids (subprojects, sections, attachments) are kept as one JSON document.
// ═══════════════════════════════════════════════════════════════════
[Route("Project")]
[Route("projects")]   // the Project Management UI calls the plural form
public class ProjectController : MyController
{
    private static readonly string[] Statuses    = { "Open", "Started", "On Hold", "Closed" };   // Cancelled only via /cancel
    private static readonly string[] Types       = { "External", "Internal" };
    private static readonly string[] StageNames  =
    {
        "Conception/Initiation", "Definition/Planning", "Launch/Execution", "Performance/Control", "Close",
    };

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IMapper            _mapper;
    private readonly IProjectRepository _repository;
    private readonly MyDbContext        _db;

    public ProjectController(IProjectRepository repository, IMapper mapper, MyDbContext db)
    {
        _mapper     = mapper;
        _repository = repository;
        _db         = db;
    }

    private string NextNumber()
    {
        var year   = DateTime.UtcNow.Year;
        var prefix = $"PRJ-{year}-";
        var used   = _repository.GetAll()
            .Where(p => p.ProjectNo.StartsWith(prefix))
            .Select(p => p.ProjectNo)
            .ToList();
        var highest = 0;
        foreach (var n in used)
            if (int.TryParse(n[prefix.Length..], out var v) && v > highest) highest = v;
        return $"{prefix}{highest + 1:D5}";
    }

    // Pull the nested grids that stay as JSON out of one request.
    private static string BuildDetails(ProjectRequest r)
    {
        var doc = new Dictionary<string, object?>
        {
            ["subprojects"] = r.Subprojects,
            ["sections"]    = r.Sections,
            ["attachments"] = r.Attachments,
        };
        return JsonSerializer.Serialize(doc, JsonOpts);
    }

    private static void ApplyScalars(ProjectRequest r, Project p)
    {
        p.ProjectType      = string.IsNullOrWhiteSpace(r.ProjectType) ? "External" : r.ProjectType;
        p.ProjectName      = r.ProjectName.Trim();
        p.Status           = string.IsNullOrWhiteSpace(r.Status) ? "Started" : r.Status;
        p.Series           = r.Series;
        p.StartDate        = r.StartDate;
        p.DueDate          = r.DueDate;
        p.FinishedDate     = r.FinishedDate;
        p.BpCode           = r.BpCode;
        p.BpName           = r.BpName;
        p.ContactPerson    = r.ContactPerson;
        p.Territory        = r.Territory;
        p.SalesEmployee    = r.SalesEmployee;
        p.Owner            = r.Owner;
        p.WithSubprojects  = r.WithSubprojects;
        p.FinancialProject = string.IsNullOrWhiteSpace(r.FinancialProject) ? null : r.FinancialProject.Trim();
        p.RiskLevel        = r.RiskLevel;
        p.Industry         = r.Industry;
        p.Comments         = r.Comments;
        p.Remarks          = r.Remarks;
    }

    private static JsonElement? Node(JsonElement doc, string name) =>
        doc.ValueKind == JsonValueKind.Object && doc.TryGetProperty(name, out var v)
        && v.ValueKind is not JsonValueKind.Null
            ? v.Clone()
            : null;

    private static ProjectStageDto ToDto(ProjectStage s) => new()
    {
        Stage = s.Stage, Task = s.Task, Description = s.Description, UniqueId = s.UniqueId, Assignee = s.Assignee,
        StartDate = s.StartDate, DueDate = s.DueDate, FinishedDate = s.FinishedDate,
        PlannedCost = s.PlannedCost, EstimatedHours = s.EstimatedHours, PercentComplete = s.PercentComplete,
    };

    private static ProjectDetailResponse ToDetail(Project p)
    {
        JsonElement doc;
        try { doc = JsonSerializer.Deserialize<JsonElement>(string.IsNullOrWhiteSpace(p.Details) ? "{}" : p.Details); }
        catch { doc = JsonSerializer.Deserialize<JsonElement>("{}"); }

        return new ProjectDetailResponse
        {
            Id               = p.Id,
            ProjectNo        = p.ProjectNo,
            ProjectName      = p.ProjectName,
            ProjectType      = p.ProjectType,
            Status           = p.Status,
            Series           = p.Series,
            StartDate        = p.StartDate,
            DueDate          = p.DueDate,
            FinishedDate     = p.FinishedDate,
            BpCode           = p.BpCode,
            BpName           = p.BpName,
            ContactPerson    = p.ContactPerson,
            Territory        = p.Territory,
            SalesEmployee    = p.SalesEmployee,
            Owner            = p.Owner,
            WithSubprojects  = p.WithSubprojects,
            FinancialProject = p.FinancialProject,
            RiskLevel        = p.RiskLevel,
            Industry         = p.Industry,
            Comments         = p.Comments,
            Remarks          = p.Remarks,
            OpenActivities   = p.OpenActivities,
            PercentComplete  = p.PercentComplete,
            Subprojects      = Node(doc, "subprojects"),
            Stages           = p.Stages.OrderBy(s => s.LineNo).Select(ToDto).ToList(),
            Sections         = Node(doc, "sections"),
            Attachments      = Node(doc, "attachments"),
        };
    }

    private Project? Load(int id) =>
        _db.Set<Project>().Include(p => p.Stages).FirstOrDefault(p => p.Id == id);

    // ── Validation ────────────────────────────────────────────
    private string? Validate(ProjectRequest r, Project? existing)
    {
        if (string.IsNullOrWhiteSpace(r.ProjectName)) return "Project name is required";
        if (r.Status == "Cancelled") return "Use the Cancel action to cancel a project";
        if (!string.IsNullOrWhiteSpace(r.Status) && !Statuses.Contains(r.Status))
            return $"Status must be one of: {string.Join(", ", Statuses)}";
        if (!string.IsNullOrWhiteSpace(r.ProjectType) && !Types.Contains(r.ProjectType))
            return $"Project type must be one of: {string.Join(", ", Types)}";
        var type = string.IsNullOrWhiteSpace(r.ProjectType) ? "External" : r.ProjectType;
        if (type == "External" && string.IsNullOrWhiteSpace(r.BpCode))
            return "BP code is required for external projects";
        if (r.StartDate != null && r.DueDate != null && r.DueDate < r.StartDate)
            return "Due date can't be before the start date";
        if (r.FinishedDate != null && r.StartDate != null && r.FinishedDate < r.StartDate)
            return "Finished date can't be before the start date";

        if (!string.IsNullOrWhiteSpace(r.BpCode))
        {
            var bp = _db.Set<BPEntity>().FirstOrDefault(b => b.Code == r.BpCode);
            if (bp == null) return $"Business partner '{r.BpCode}' does not exist";
            r.BpName = bp.CardName;   // canonical
        }

        if (!string.IsNullOrWhiteSpace(r.FinancialProject))
        {
            var code = r.FinancialProject.Trim().ToUpper();
            var fp = _db.Set<FinancialProject>().FirstOrDefault(f => f.ProjectCode.ToUpper() == code);
            if (fp == null) return $"Financial project '{r.FinancialProject}' does not exist";
            if (fp.InActive == true) return $"Financial project '{fp.ProjectCode}' is inactive";
            r.FinancialProject = fp.ProjectCode;
            var clash = _db.Set<Project>().FirstOrDefault(p =>
                p.FinancialProject != null && p.FinancialProject.ToUpper() == code
                && p.Status != "Cancelled" && (existing == null || p.Id != existing.Id));
            if (clash != null) return $"Financial project '{fp.ProjectCode}' is already used by {clash.ProjectNo}";
        }

        // Closing needs every stage finished. (Stages are validated below; use
        // what the request will save.)
        var stages = r.Stages ?? existing?.Stages.Select(ToDto).ToList() ?? new();
        var hasStages = stages.Count > 0;
        var lineErr = ValidateStages(r, stages);
        if (lineErr != null) return lineErr;
        if (r.Status == "Closed" && stages.Any(s => s.FinishedDate == null && s.PercentComplete < 100))
            return $"The project can't be closed while {stages.Count(s => s.FinishedDate == null && s.PercentComplete < 100)} stage(s) are unfinished";
        if (existing != null && existing.Status == "Closed" && r.Status == "Closed")
            return "A closed project can't be edited; reopen it (change Status) first";
        _ = hasStages;
        return null;
    }

    // Normalises each stage in place (finished ⇒ 100%, 100% ⇒ finished today).
    private static string? ValidateStages(ProjectRequest r, List<ProjectStageDto> stages)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var n = 0;
        foreach (var s in stages)
        {
            n++;
            var at = $"Stage {n}";
            if (string.IsNullOrWhiteSpace(s.Stage) || !StageNames.Contains(s.Stage))
                return $"{at}: stage must be one of: {string.Join(", ", StageNames)}";
            if (s.PercentComplete < 0 || s.PercentComplete > 100) return $"{at}: percent complete must be between 0 and 100";
            if (s.PlannedCost < 0) return $"{at}: planned cost can't be negative";
            if (s.EstimatedHours < 0) return $"{at}: estimated hours can't be negative";
            if (s.StartDate != null && s.DueDate != null && s.DueDate < s.StartDate) return $"{at}: due date is before its start date";
            if (s.StartDate != null && s.FinishedDate != null && s.FinishedDate < s.StartDate) return $"{at}: finished date is before its start date";
            if (r.StartDate != null && s.StartDate != null && s.StartDate.Value.Date < r.StartDate.Value.Date)
                return $"{at}: starts before the project starts ({r.StartDate:yyyy-MM-dd})";
            if (r.DueDate != null && s.DueDate != null && s.DueDate.Value.Date > r.DueDate.Value.Date)
                return $"{at}: is due after the project is due ({r.DueDate:yyyy-MM-dd})";
            if (!string.IsNullOrWhiteSpace(s.UniqueId) && !ids.Add(s.UniqueId.Trim()))
                return $"{at}: unique ID '{s.UniqueId}' is used more than once";

            if (s.FinishedDate != null) s.PercentComplete = 100;
            else if (s.PercentComplete >= 100)
            {
                var today = DateTime.UtcNow.Date;
                s.FinishedDate = s.StartDate != null && s.StartDate.Value.Date > today ? s.StartDate.Value.Date : today;
            }
        }
        return null;
    }

    // Replace the project's stage rows and derive its progress.
    private void ApplyStages(Project p, List<ProjectStageDto> stages)
    {
        _db.Set<ProjectStage>().RemoveRange(p.Stages.ToList());
        p.Stages.Clear();
        var i = 0;
        foreach (var s in stages)
        {
            p.Stages.Add(new ProjectStage
            {
                LineNo = ++i, Stage = s.Stage, Task = s.Task?.Trim(), Description = s.Description,
                UniqueId = string.IsNullOrWhiteSpace(s.UniqueId) ? null : s.UniqueId.Trim(),
                Assignee = s.Assignee, StartDate = s.StartDate, DueDate = s.DueDate, FinishedDate = s.FinishedDate,
                PlannedCost = s.PlannedCost, EstimatedHours = s.EstimatedHours, PercentComplete = s.PercentComplete,
            });
        }
        Rollup(p);
    }

    private static void Rollup(Project p)
    {
        var rows = p.Stages;
        p.OpenActivities = rows.Count(s => s.FinishedDate == null);
        var weight = rows.Sum(s => s.EstimatedHours > 0 ? s.EstimatedHours : 1m);
        p.PercentComplete = rows.Count == 0 || weight == 0
            ? 0
            : Math.Round(rows.Sum(s => (s.EstimatedHours > 0 ? s.EstimatedHours : 1m) * s.PercentComplete) / weight, 2);
    }

    // Status side effects: closing stamps the finished date, reopening clears it.
    private static void StampFinish(Project p, string? previous)
    {
        if (p.Status == "Closed" && p.FinishedDate == null) p.FinishedDate = DateTime.UtcNow.Date;
        if (p.Status != "Closed" && previous == "Closed") p.FinishedDate = null;
    }

    // ── GET /projects ─────────────────────────────────────────
    [HttpGet]
    public IActionResult Gets([FromQuery] string? status = null)
    {
        var query = _repository.GetAll();
        if (!string.IsNullOrWhiteSpace(status) && status != "All")
            query = query.Where(p => p.Status == status);
        var rows = _mapper.ProjectTo<ProjectListResponse>(query.OrderByDescending(p => p.Id)).ToList();
        return Ok(rows);
    }

    // ── GET /projects/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var entity = Load(id);
        if (entity == null) return NotFound($"Project {id} not found");
        return Ok(ToDetail(entity));
    }

    // ── POST /projects ────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] ProjectRequest request)
    {
        var error = Validate(request, null);
        if (error != null) return BadRequest(error);

        var entity = new Project { ProjectNo = NextNumber(), CreatedAt = DateTime.UtcNow, InActive = false };
        ApplyScalars(request, entity);
        StampFinish(entity, null);
        entity.Details = BuildDetails(request);
        ApplyStages(entity, request.Stages ?? new());

        _db.Set<Project>().Add(entity);
        _db.SaveChanges();
        return Ok(new { message = "Project created", id = entity.Id, projectNo = entity.ProjectNo });
    }

    // ── PUT /projects/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ProjectRequest request)
    {
        var entity = Load(id);
        if (entity == null) return NotFound($"Project {id} not found");
        if (entity.Status == "Cancelled") return BadRequest("A cancelled project cannot be edited");
        var error = Validate(request, entity);
        if (error != null) return BadRequest(error);

        var previous = entity.Status;
        ApplyScalars(request, entity);
        StampFinish(entity, previous);
        entity.Details   = BuildDetails(request);
        if (request.Stages != null) ApplyStages(entity, request.Stages);
        entity.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();
        return NoContent();
    }

    // ── POST /projects/{id}/cancel ────────────────────────────
    [HttpPost("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(p => p.Id == id);
        if (entity == null) return NotFound($"Project {id} not found");
        if (entity.Status == "Cancelled") return BadRequest("Project is already cancelled");

        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Project cancelled", id });
    }

    // ── GET /projects/{id}/Timeline ───────────────────────────
    // Schedule health: time elapsed against work complete.
    //   Complete  every stage finished
    //   Overdue   past the due date, or a stage is past its due date and open
    //   Behind    progress is 10+ points below the share of time used
    //   On track  otherwise
    [HttpGet("{id:int}/Timeline")]
    public IActionResult Timeline(int id)
    {
        var p = Load(id);
        if (p == null) return NotFound($"Project {id} not found");
        var today = DateTime.UtcNow.Date;

        var stages = p.Stages.OrderBy(s => s.StartDate ?? DateTime.MaxValue).ThenBy(s => s.LineNo).Select(s =>
        {
            string state; int late = 0;
            if (s.FinishedDate != null)
            {
                state = "Done";
                if (s.DueDate != null && s.FinishedDate.Value.Date > s.DueDate.Value.Date)
                    late = (s.FinishedDate.Value.Date - s.DueDate.Value.Date).Days;
            }
            else if (s.DueDate != null && s.DueDate.Value.Date < today)
            {
                state = "Overdue"; late = (today - s.DueDate.Value.Date).Days;
            }
            else if (s.PercentComplete > 0 || (s.StartDate != null && s.StartDate.Value.Date <= today)) state = "In progress";
            else state = "Not started";
            return new TimelineStage
            {
                LineNo = s.LineNo, Stage = s.Stage, Task = s.Task, Assignee = s.Assignee,
                StartDate = s.StartDate, DueDate = s.DueDate, FinishedDate = s.FinishedDate,
                PercentComplete = s.PercentComplete, State = state, DaysLate = late,
            };
        }).ToList();

        int? planned = p.StartDate != null && p.DueDate != null ? Math.Max(1, (p.DueDate.Value.Date - p.StartDate.Value.Date).Days) : null;
        int? elapsed = p.StartDate != null ? Math.Max(0, (today - p.StartDate.Value.Date).Days) : null;
        decimal? timePct = planned != null && elapsed != null
            ? Math.Round(Math.Min(100m, elapsed.Value * 100m / planned.Value), 2)
            : null;
        var overdue = stages.Count(s => s.State == "Overdue");

        string schedule;
        if (p.Status == "Cancelled") schedule = "Cancelled";
        else if (p.Status == "Closed") schedule = "Closed";
        else if (p.Stages.Count > 0 && p.OpenActivities == 0) schedule = "Complete";
        else if (p.StartDate != null && p.StartDate.Value.Date > today) schedule = "Not started";
        else if (overdue > 0 || (p.DueDate != null && p.DueDate.Value.Date < today)) schedule = "Overdue";
        else if (timePct != null && p.PercentComplete + 10m < timePct) schedule = "Behind";
        else schedule = "On track";

        return Ok(new TimelineResponse
        {
            ProjectId = p.Id, ProjectNo = p.ProjectNo, Status = p.Status, AsOf = today,
            StartDate = p.StartDate, DueDate = p.DueDate, PlannedDays = planned, ElapsedDays = elapsed, TimeElapsedPct = timePct,
            PercentComplete = p.PercentComplete, OpenActivities = p.OpenActivities, OverdueStages = overdue,
            Schedule = schedule, PlannedCost = p.Stages.Sum(s => s.PlannedCost), Stages = stages,
        });
    }

    // ── GET /projects/{id}/Budget ─────────────────────────────
    // Budget vs actual by account, from the linked financial project.
    [HttpGet("{id:int}/Budget")]
    public IActionResult Budget(int id, [FromQuery] int? year = null)
    {
        var p = _repository.GetSingle(x => x.Id == id);
        if (p == null) return NotFound($"Project {id} not found");
        if (string.IsNullOrWhiteSpace(p.FinancialProject))
            return BadRequest("This project has no financial project linked; set one on the project to see its budget and actuals");
        var code = p.FinancialProject.ToUpper();
        var fp = _db.Set<FinancialProject>().FirstOrDefault(f => f.ProjectCode.ToUpper() == code);
        if (fp == null) return BadRequest($"Financial project '{p.FinancialProject}' no longer exists");
        return Ok(ProjectSummaryBuilder.Build(_db, fp, year));
    }

    // ── DELETE /projects/{id} ─────────────────────────────────
    // Work that has started stays on record: cancel it instead.
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = Load(id);
        if (entity == null) return NotFound($"Project {id} not found");
        if (entity.Status != "Cancelled" && entity.Stages.Any(s => s.PercentComplete > 0 || s.FinishedDate != null))
            return BadRequest("Work has been recorded on this project; cancel it instead of deleting it");
        if (entity.Status == "Closed") return BadRequest("A closed project can't be deleted");

        _db.Set<Project>().Remove(entity);
        _db.SaveChanges();
        return NoContent();
    }
}

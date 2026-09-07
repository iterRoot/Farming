using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ImplementationCentre;

// ═══════════════════════════════════════════════════════════════
// IMPLEMENTATION CENTRE  (route: /ImplementationTask)
// GET    /ImplementationTask            → all tasks (ordered)
// GET    /ImplementationTask/Summary    → progress counters
// GET    /ImplementationTask/{id}       → one task
// POST   /ImplementationTask            → create
// PUT    /ImplementationTask/{id}       → update
// DELETE /ImplementationTask/{id}       → delete
// POST   /ImplementationTask/Seed       → seed the default checklist (once)
// ═══════════════════════════════════════════════════════════════
public class ImplementationTaskController : MyController
{
    private readonly IImplementationTaskRepository _repo;

    public ImplementationTaskController(IImplementationTaskRepository repo) => _repo = repo;

    private static readonly string[] Statuses   = { "Open", "In Process", "Completed", "Skipped" };
    private static readonly string[] Priorities = { "Low", "Medium", "High" };

    private static ImplementationTaskDto ToDto(ImplementationTask x) => new()
    {
        Id = x.Id, Phase = x.Phase, Title = x.Title, Description = x.Description,
        Responsible = x.Responsible, Status = x.Status, Priority = x.Priority,
        PlannedDate = x.PlannedDate, CompletedDate = x.CompletedDate,
        LinkPath = x.LinkPath, Notes = x.Notes, SortOrder = x.SortOrder,
        InActive = x.InActive ?? false,
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(ToDto).ToList();
        return Ok(list);
    }

    [AllowAnonymous]
    [HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        var total = all.Count;
        var completed = all.Count(x => x.Status == "Completed");
        return Ok(new ImplementationSummary
        {
            Total = total,
            Completed = completed,
            InProcess = all.Count(x => x.Status == "In Process"),
            Open = all.Count(x => x.Status == "Open"),
            Skipped = all.Count(x => x.Status == "Skipped"),
            PercentComplete = total == 0 ? 0 : (int)Math.Round(completed * 100.0 / total),
        });
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Task {id} not found.");
        return Ok(ToDto(x));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] ImplementationTaskSaveRequest req)
    {
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        var e = new ImplementationTask { CreatedAt = DateTime.UtcNow };
        Apply(e, req);
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Task created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ImplementationTaskSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Task {id} not found.");
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Task updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Task {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    // Populate the standard SAP-style checklist the first time.
    [AllowAnonymous]
    [HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return Ok(new { seeded = 0, message = "Checklist already has tasks." });

        var seed = DefaultChecklist();
        var order = 0;
        foreach (var t in seed)
        {
            t.SortOrder = order++;
            t.CreatedAt = DateTime.UtcNow;
            _repo.Add(t);
        }
        _repo.Commit();
        return Ok(new { seeded = seed.Count, message = "Default checklist created." });
    }

    private static void Apply(ImplementationTask e, ImplementationTaskSaveRequest r)
    {
        e.Phase = r.Phase.Trim();
        e.Title = r.Title.Trim();
        e.Description = r.Description;
        e.Responsible = r.Responsible;
        e.Status = r.Status;
        e.Priority = r.Priority;
        e.PlannedDate = r.PlannedDate;
        // Stamp completion date automatically when moved to Completed.
        e.CompletedDate = r.Status == "Completed"
            ? (r.CompletedDate ?? DateOnly.FromDateTime(DateTime.UtcNow))
            : null;
        e.LinkPath = r.LinkPath;
        e.Notes = r.Notes;
        e.SortOrder = r.SortOrder;
        e.InActive = r.InActive;
    }

    private static string? Validate(ImplementationTaskSaveRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Title)) return "Task title is required.";
        if (string.IsNullOrWhiteSpace(r.Phase)) return "Phase is required.";
        if (!Statuses.Contains(r.Status))       return "Invalid status.";
        if (!Priorities.Contains(r.Priority))   return "Invalid priority.";
        return null;
    }

    private static List<ImplementationTask> DefaultChecklist() => new()
    {
        new() { Phase = "Getting Started", Title = "Define company details",     Description = "Enter legal name, address and tax registration.", LinkPath = "/SystemInitialization/CompanyDetails/Create" },
        new() { Phase = "Getting Started", Title = "Configure general settings",  Description = "Set company-wide preferences.",                  LinkPath = "/SystemInitialization/GeneralSettings/Create" },
        new() { Phase = "Getting Started", Title = "Set up posting periods",      Description = "Define fiscal year and posting periods.",        LinkPath = "/SystemInitialization/PostingPeriods/Create" },
        new() { Phase = "Financials",      Title = "Define chart of accounts",    Description = "Import or build the G/L account structure." },
        new() { Phase = "Financials",      Title = "Set G/L account determination", Description = "Map default accounts for automatic postings." },
        new() { Phase = "Financials",      Title = "Configure currencies & rates", Description = "Define currencies and daily exchange rates.",    LinkPath = "/Administration/ExchangeRateAndIndex/List" },
        new() { Phase = "Master Data",     Title = "Create branches",             Description = "Register operating branches.",                    LinkPath = "/Branches/List" },
        new() { Phase = "Master Data",     Title = "Import item master data",     Description = "Load items, prices and inventory settings." },
        new() { Phase = "Master Data",     Title = "Import business partners",     Description = "Load customers and vendors." },
        new() { Phase = "Security",        Title = "Create users",                Description = "Add user accounts.",                              LinkPath = "/Administration/SystemInitialization/Authorisations/List" },
        new() { Phase = "Security",        Title = "Assign authorisations",       Description = "Grant permissions per user.",                     LinkPath = "/Administration/SystemInitialization/Authorisations/List" },
        new() { Phase = "Go Live",         Title = "Enter opening balances",      Description = "Post opening balances for G/L, stock and BP." },
        new() { Phase = "Go Live",         Title = "Verify document settings",    Description = "Review per-document defaults.",                   LinkPath = "/Administration/SystemInitialization/DocumentSettings/List" },
        new() { Phase = "Go Live",         Title = "Final review & sign-off",     Description = "Confirm configuration with stakeholders." },
    };
}

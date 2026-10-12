using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using BudgetDoc        = FarmingApi.Modules.Financials.Budget.Budget;
using BudgetLineDoc    = FarmingApi.Modules.Financials.Budget.BudgetLine;
using CoaEntity        = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using PmProject        = FarmingApi.Modules.ProjectManagement.Project.Project;

namespace FarmingApi.Modules.Financials.FinancialProjects;

// ═══════════════════════════════════════════════════════════════
// FINANCIAL PROJECTS  (route: /FinancialProject)
// GET    /FinancialProject        → all projects
// GET    /FinancialProject/{id}   → one
// POST   /FinancialProject        → create one
// POST   /FinancialProject/Bulk   → create many (grid "OK")
// PUT    /FinancialProject/{id}   → update
// DELETE /FinancialProject/{id}   → refused while anything uses the project
// GET    /FinancialProject/{id}/Summary?year=  → budget vs actual by account
// ═══════════════════════════════════════════════════════════════
public class FinancialProjectController : MyController
{
    private readonly IFinancialProjectRepository _repo;
    private readonly MyDbContext _db;

    public FinancialProjectController(IFinancialProjectRepository repo, MyDbContext db) { _repo = repo; _db = db; }

    private static FinancialProjectDto ToDto(FinancialProject x) => new()
    {
        Id = x.Id, ProjectCode = x.ProjectCode, ProjectName = x.ProjectName,
        ValidFrom = x.ValidFrom, ValidTo = x.ValidTo, Active = !(x.InActive ?? false),
    };

    [HttpGet]
    public IActionResult GetAll()
        => Ok(_repo.GetAll().OrderBy(x => x.ProjectCode).Select(ToDto).ToList());

    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Project {id} not found.");
        return Ok(ToDto(x));
    }

    [HttpPost]
    public IActionResult Create([FromBody] FinancialProjectSaveRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var e = new FinancialProject { CreatedAt = DateTime.UtcNow };
        Apply(e, req);
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Project created." });
    }

    [HttpPost("Bulk")]
    public IActionResult CreateBulk([FromBody] List<FinancialProjectSaveRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Add at least one project before saving.");

        var dup = requests.GroupBy(r => (r.ProjectCode ?? "").Trim().ToLowerInvariant())
                          .FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return BadRequest($"Project code \"{dup.First().ProjectCode}\" appears more than once.");

        var existing = _repo.GetAll().AsEnumerable().Select(x => x.ProjectCode.ToLowerInvariant()).ToHashSet();
        var created = 0;
        foreach (var req in requests)
        {
            var err = Validate(req, null);
            if (err != null) return BadRequest(err);
            if (existing.Contains(req.ProjectCode.Trim().ToLowerInvariant()))
                return BadRequest($"Project code \"{req.ProjectCode}\" already exists.");

            var e = new FinancialProject { CreatedAt = DateTime.UtcNow };
            Apply(e, req);
            _repo.Add(e);
            existing.Add(req.ProjectCode.Trim().ToLowerInvariant());
            created++;
        }
        _repo.Commit();
        return Ok(new { created, message = $"{created} project(s) created." });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] FinancialProjectSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Project {id} not found.");
        var err = Validate(req, id);
        if (err != null) return BadRequest(err);
        if (!string.Equals(e.ProjectCode, req.ProjectCode.Trim(), StringComparison.Ordinal))
        {
            var use = UsageOf(e.ProjectCode);
            if (use != null) return BadRequest($"The code can't change: {use}.");
        }

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Project updated." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Project {id} not found.");
        var usage = UsageOf(e.ProjectCode);
        if (usage != null) return BadRequest($"'{e.ProjectCode}' can't be deleted: {usage}. Mark it inactive instead.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    // ── What a project is used by ───────────────────────────────
    private string? UsageOf(string code)
    {
        var c = code.ToUpper();
        var uses = new List<string>();
        var jl = _db.Set<JournalEntryLine>().Count(l => l.Project != null && l.Project.ToUpper() == c);
        var jh = _db.Set<JournalEntryDoc>().Count(j => j.ProjectCode != null && j.ProjectCode.ToUpper() == c);
        var bl = _db.Set<BudgetLineDoc>().Count(l => l.Project != null && l.Project.ToUpper() == c);
        var pm = _db.Set<PmProject>().Count(p => p.FinancialProject != null && p.FinancialProject.ToUpper() == c);
        if (jl + jh > 0) uses.Add($"{jl + jh} journal entr{(jl + jh == 1 ? "y line/header uses" : "y lines/headers use")} it");
        if (bl > 0) uses.Add($"{bl} budget line(s) use it");
        if (pm > 0) uses.Add($"{pm} project(s) in Project Management use it");
        return uses.Count == 0 ? null : string.Join("; ", uses);
    }

    // ── GET /FinancialProject/{id}/Summary ──────────────────────
    // Budget: lines tagged with the project on Approved or Closed budgets.
    // Actual: journal lines tagged with it (or on a header tagged with it and
    // no line tag), on entries that aren't void. "Actual" is the account's
    // natural-side amount; variance is favourable when positive.
    [HttpGet("{id:int}/Summary")]
    public IActionResult Summary(int id, [FromQuery] int? year = null)
    {
        var project = _repo.GetSingle(x => x.Id == id);
        if (project == null) return NotFound($"Project {id} not found.");
        return Ok(ProjectSummaryBuilder.Build(_db, project, year));
    }

    private static void Apply(FinancialProject e, FinancialProjectSaveRequest r)
    {
        e.ProjectCode = r.ProjectCode.Trim();
        e.ProjectName = r.ProjectName;
        e.ValidFrom = r.ValidFrom;
        e.ValidTo = r.ValidTo;
        e.InActive = !r.Active;
    }

    private string? Validate(FinancialProjectSaveRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.ProjectCode)) return "Project code is required.";
        if (r.ValidFrom.HasValue && r.ValidTo.HasValue && r.ValidTo < r.ValidFrom)
            return "Valid To cannot be earlier than Valid From.";

        var code = r.ProjectCode.Trim().ToLowerInvariant();
        var clash = _repo.GetAll().AsEnumerable()
            .Any(x => x.ProjectCode.ToLowerInvariant() == code && x.Id != ignoreId);
        if (clash) return $"Project code \"{r.ProjectCode}\" already exists.";
        return null;
    }
}

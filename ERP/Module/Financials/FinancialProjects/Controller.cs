using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FinancialProjects;

// ═══════════════════════════════════════════════════════════════
// FINANCIAL PROJECTS  (route: /FinancialProject)
// GET    /FinancialProject        → all projects
// GET    /FinancialProject/{id}   → one
// POST   /FinancialProject        → create one
// POST   /FinancialProject/Bulk   → create many (grid "OK")
// PUT    /FinancialProject/{id}   → update
// DELETE /FinancialProject/{id}   → delete
// ═══════════════════════════════════════════════════════════════
public class FinancialProjectController : MyController
{
    private readonly IFinancialProjectRepository _repo;

    public FinancialProjectController(IFinancialProjectRepository repo) => _repo = repo;

    private static FinancialProjectDto ToDto(FinancialProject x) => new()
    {
        Id = x.Id, ProjectCode = x.ProjectCode, ProjectName = x.ProjectName,
        ValidFrom = x.ValidFrom, ValidTo = x.ValidTo, Active = !(x.InActive ?? false),
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
        => Ok(_repo.GetAll().OrderBy(x => x.ProjectCode).Select(ToDto).ToList());

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Project {id} not found.");
        return Ok(ToDto(x));
    }

    [AllowAnonymous]
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

    [AllowAnonymous]
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

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] FinancialProjectSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Project {id} not found.");
        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Project updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Project {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
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

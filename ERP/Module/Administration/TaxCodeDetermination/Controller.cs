using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.TaxCodeDetermination;

// ═══════════════════════════════════════════════════════════════
// TAX CODE DETERMINATION  (route: /TaxDeterminationRule)
// GET    /TaxDeterminationRule        → all rules (by priority)
// GET    /TaxDeterminationRule/{id}   → one
// POST   /TaxDeterminationRule        → create one
// POST   /TaxDeterminationRule/Bulk   → create many (grid "OK")
// PUT    /TaxDeterminationRule/{id}   → update
// DELETE /TaxDeterminationRule/{id}   → delete
// ═══════════════════════════════════════════════════════════════
public class TaxDeterminationRuleController : MyController
{
    private readonly ITaxDeterminationRuleRepository _repo;

    public TaxDeterminationRuleController(ITaxDeterminationRuleRepository repo) => _repo = repo;

    private static TaxDeterminationRuleDto ToDto(TaxDeterminationRule x) => new()
    {
        Id = x.Id, DocumentType = x.DocumentType, BusinessArea = x.BusinessArea,
        Condition1 = x.Condition1, Value1 = x.Value1,
        Condition2 = x.Condition2, Value2 = x.Value2,
        Condition3 = x.Condition3, Value3 = x.Value3,
        Description = x.Description, LineTaxCode = x.LineTaxCode,
        Priority = x.Priority, InActive = x.InActive ?? false,
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
        => Ok(_repo.GetAll().OrderBy(x => x.Priority).ThenBy(x => x.Id).Select(ToDto).ToList());

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Rule {id} not found.");
        return Ok(ToDto(x));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] TaxDeterminationRuleSaveRequest req)
    {
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        var e = new TaxDeterminationRule { CreatedAt = DateTime.UtcNow };
        Apply(e, req);
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Rule created." });
    }

    [AllowAnonymous]
    [HttpPost("Bulk")]
    public IActionResult CreateBulk([FromBody] List<TaxDeterminationRuleSaveRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Add at least one rule before saving.");

        var created = 0;
        foreach (var req in requests)
        {
            var err = Validate(req);
            if (err != null) return BadRequest($"Row {created + 1}: {err}");

            var e = new TaxDeterminationRule { CreatedAt = DateTime.UtcNow };
            Apply(e, req);
            _repo.Add(e);
            created++;
        }
        _repo.Commit();
        return Ok(new { created, message = $"{created} rule(s) created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] TaxDeterminationRuleSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Rule {id} not found.");
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Rule updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Rule {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private static void Apply(TaxDeterminationRule e, TaxDeterminationRuleSaveRequest r)
    {
        e.DocumentType = string.IsNullOrWhiteSpace(r.DocumentType) ? "All" : r.DocumentType.Trim();
        e.BusinessArea = r.BusinessArea;
        e.Condition1 = r.Condition1; e.Value1 = r.Value1;
        e.Condition2 = r.Condition2; e.Value2 = r.Value2;
        e.Condition3 = r.Condition3; e.Value3 = r.Value3;
        e.Description = r.Description;
        e.LineTaxCode = r.LineTaxCode.Trim();
        e.Priority = r.Priority;
        e.InActive = r.InActive;
    }

    private static string? Validate(TaxDeterminationRuleSaveRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.LineTaxCode)) return "Line Tax Code is required.";
        return null;
    }
}

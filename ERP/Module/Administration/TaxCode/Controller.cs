using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.TaxCode;

// ═══════════════════════════════════════════════════════════════
// TAX CODES  (route: /TaxCode)
// GET    /TaxCode          → all codes
// GET    /TaxCode/{id}     → one
// POST   /TaxCode          → create one
// POST   /TaxCode/Bulk     → create many (grid "OK")
// PUT    /TaxCode/{id}     → update
// DELETE /TaxCode/{id}     → delete
// ═══════════════════════════════════════════════════════════════
public class TaxCodeController : MyController
{
    private readonly ITaxCodeRepository _repo;

    public TaxCodeController(ITaxCodeRepository repo) => _repo = repo;

    private static readonly string[] Categories = { "Output Tax", "Input Tax" };

    private static TaxCodeDto ToDto(TaxCodeEntry x) => new()
    {
        Id = x.Id, Code = x.Code, Inactive = x.InActive ?? false, Name = x.Name,
        Category = x.Category, AcquisitionReverse = x.AcquisitionReverse,
        EffectiveFrom = x.EffectiveFrom, Rate = x.Rate,
        NonDeductiblePercent = x.NonDeductiblePercent, TaxAccount = x.TaxAccount,
        AcquisitionTaxAccount = x.AcquisitionTaxAccount, DeferredTaxAccount = x.DeferredTaxAccount,
        NonDeductibleAccount = x.NonDeductibleAccount, CodeDescription = x.CodeDescription,
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
        => Ok(_repo.GetAll().OrderBy(x => x.Code).Select(ToDto).ToList());

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Tax code {id} not found.");
        return Ok(ToDto(x));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] TaxCodeSaveRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var e = new TaxCodeEntry { CreatedAt = DateTime.UtcNow };
        Apply(e, req);
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Tax code created." });
    }

    [AllowAnonymous]
    [HttpPost("Bulk")]
    public IActionResult CreateBulk([FromBody] List<TaxCodeSaveRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Add at least one tax code before saving.");

        // Reject duplicate codes within the batch.
        var dup = requests.GroupBy(r => (r.Code ?? "").Trim().ToLowerInvariant())
                          .FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return BadRequest($"Tax code \"{dup.First().Code}\" appears more than once.");

        var existing = _repo.GetAll().Select(x => x.Code.ToLowerInvariant()).ToHashSet();
        var created = 0;
        foreach (var req in requests)
        {
            var err = Validate(req, null);
            if (err != null) return BadRequest(err);
            if (existing.Contains(req.Code.Trim().ToLowerInvariant()))
                return BadRequest($"Tax code \"{req.Code}\" already exists.");

            var e = new TaxCodeEntry { CreatedAt = DateTime.UtcNow };
            Apply(e, req);
            _repo.Add(e);
            existing.Add(req.Code.Trim().ToLowerInvariant());
            created++;
        }
        _repo.Commit();
        return Ok(new { created, message = $"{created} tax code(s) created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] TaxCodeSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Tax code {id} not found.");
        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Tax code updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Tax code {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private static void Apply(TaxCodeEntry e, TaxCodeSaveRequest r)
    {
        e.Code = r.Code.Trim();
        e.InActive = r.Inactive;
        e.Name = r.Name;
        e.Category = r.Category;
        e.AcquisitionReverse = r.AcquisitionReverse;
        e.EffectiveFrom = r.EffectiveFrom;
        e.Rate = r.Rate;
        e.NonDeductiblePercent = r.NonDeductiblePercent;
        e.TaxAccount = r.TaxAccount;
        e.AcquisitionTaxAccount = r.AcquisitionTaxAccount;
        e.DeferredTaxAccount = r.DeferredTaxAccount;
        e.NonDeductibleAccount = r.NonDeductibleAccount;
        e.CodeDescription = r.CodeDescription;
    }

    // `ignoreId` skips the row being updated during the uniqueness check.
    private string? Validate(TaxCodeSaveRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.Code))   return "Tax code is required.";
        if (!Categories.Contains(r.Category))    return "Category must be Output Tax or Input Tax.";
        if (r.Rate < 0 || r.Rate > 100)          return "Rate % must be between 0 and 100.";
        if (r.NonDeductiblePercent < 0 || r.NonDeductiblePercent > 100)
                                                 return "Non-deductible % must be between 0 and 100.";
        var code = r.Code.Trim().ToLowerInvariant();
        var clash = _repo.GetAll().AsEnumerable()
            .Any(x => x.Code.ToLowerInvariant() == code && x.Id != ignoreId);
        if (clash) return $"Tax code \"{r.Code}\" already exists.";
        return null;
    }
}

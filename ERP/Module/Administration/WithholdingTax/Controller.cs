using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.WithholdingTax;

// ═══════════════════════════════════════════════════════════════
// WITHHOLDING TAX CODES  (route: /WithholdingTaxCode)
// GET    /WithholdingTaxCode        → all codes
// GET    /WithholdingTaxCode/{id}   → one
// POST   /WithholdingTaxCode        → create one
// POST   /WithholdingTaxCode/Bulk   → create many (grid "OK")
// PUT    /WithholdingTaxCode/{id}   → update
// DELETE /WithholdingTaxCode/{id}   → delete
// ═══════════════════════════════════════════════════════════════
public class WithholdingTaxCodeController : MyController
{
    private readonly IWithholdingTaxCodeRepository _repo;

    public WithholdingTaxCodeController(IWithholdingTaxCodeRepository repo) => _repo = repo;

    private static readonly string[] Categories = { "Payment", "Invoice" };
    private static readonly string[] BaseTypes  = { "Net", "Gross" };
    private static readonly string[] Roundings  = { "Commercial Values", "Round Up", "Round Down", "Truncate" };

    private static WithholdingTaxCodeDto ToDto(WithholdingTaxCode x) => new()
    {
        Id = x.Id, Code = x.Code, Inactive = x.InActive ?? false, Name = x.Name,
        Category = x.Category, EffectiveFrom = x.EffectiveFrom, Rate = x.Rate,
        BaseType = x.BaseType, RoundingType = x.RoundingType,
        BaseAmountPercent = x.BaseAmountPercent, OfficialCode = x.OfficialCode,
        Account = x.Account,
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
        if (x == null) return NotFound($"Withholding tax code {id} not found.");
        return Ok(ToDto(x));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] WithholdingTaxCodeSaveRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var e = new WithholdingTaxCode { CreatedAt = DateTime.UtcNow };
        Apply(e, req);
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Withholding tax code created." });
    }

    [AllowAnonymous]
    [HttpPost("Bulk")]
    public IActionResult CreateBulk([FromBody] List<WithholdingTaxCodeSaveRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Add at least one withholding tax code before saving.");

        var dup = requests.GroupBy(r => (r.Code ?? "").Trim().ToLowerInvariant())
                          .FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return BadRequest($"WTax code \"{dup.First().Code}\" appears more than once.");

        var existing = _repo.GetAll().AsEnumerable().Select(x => x.Code.ToLowerInvariant()).ToHashSet();
        var created = 0;
        foreach (var req in requests)
        {
            var err = Validate(req, null);
            if (err != null) return BadRequest(err);
            if (existing.Contains(req.Code.Trim().ToLowerInvariant()))
                return BadRequest($"WTax code \"{req.Code}\" already exists.");

            var e = new WithholdingTaxCode { CreatedAt = DateTime.UtcNow };
            Apply(e, req);
            _repo.Add(e);
            existing.Add(req.Code.Trim().ToLowerInvariant());
            created++;
        }
        _repo.Commit();
        return Ok(new { created, message = $"{created} withholding tax code(s) created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] WithholdingTaxCodeSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Withholding tax code {id} not found.");
        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Withholding tax code updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Withholding tax code {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private static void Apply(WithholdingTaxCode e, WithholdingTaxCodeSaveRequest r)
    {
        e.Code = r.Code.Trim();
        e.InActive = r.Inactive;
        e.Name = r.Name;
        e.Category = r.Category;
        e.EffectiveFrom = r.EffectiveFrom;
        e.Rate = r.Rate;
        e.BaseType = r.BaseType;
        e.RoundingType = r.RoundingType;
        e.BaseAmountPercent = r.BaseAmountPercent;
        e.OfficialCode = r.OfficialCode;
        e.Account = r.Account;
    }

    private string? Validate(WithholdingTaxCodeSaveRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.Code))  return "WTax code is required.";
        if (!Categories.Contains(r.Category))   return "Category must be Payment or Invoice.";
        if (!BaseTypes.Contains(r.BaseType))    return "Base type must be Net or Gross.";
        if (!Roundings.Contains(r.RoundingType)) return "Invalid rounding type.";
        if (r.Rate < 0 || r.Rate > 100)         return "Rate must be between 0 and 100.";
        if (r.BaseAmountPercent < 0 || r.BaseAmountPercent > 100)
                                                return "% Base Amount must be between 0 and 100.";
        var code = r.Code.Trim().ToLowerInvariant();
        var clash = _repo.GetAll().AsEnumerable()
            .Any(x => x.Code.ToLowerInvariant() == code && x.Id != ignoreId);
        if (clash) return $"WTax code \"{r.Code}\" already exists.";
        return null;
    }
}

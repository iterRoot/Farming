using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SetUp.Currencies;

// ═══════════════════════════════════════════════════════════════
// CURRENCIES — SETUP  (route: /Currency, alias: /Currencies)
// GET    /Currency          → all currencies
// GET    /Currency/{id}     → one
// POST   /Currency          → create one
// POST   /Currency/Bulk     → create many (grid "Update")
// PUT    /Currency/{id}     → update
// DELETE /Currency/{key}    → delete by id or code
// ═══════════════════════════════════════════════════════════════
[Route("Currency")]
[Route("Currencies")]
public class CurrenciesController : MyController
{
    private readonly ICurrenciesRepository _repo;
    private readonly IMapper _mapper;

    public CurrenciesController(ICurrenciesRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
        => Ok(_mapper.Map<List<CurrencyResponse>>(_repo.GetAll().OrderBy(x => x.Code).ToList()));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound($"Currency {id} not found.");
        return Ok(_mapper.Map<CurrencyResponse>(entity));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] CurrencyRequest request)
    {
        var err = Validate(request, null);
        if (err != null) return BadRequest(err);

        var entity = _mapper.Map<Currencies>(request);
        entity.CreatedAt = DateTime.UtcNow;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(new { id = entity.Id, message = "Currency created." });
    }

    // Grid "Update" button posts every filled row here in one shot.
    [AllowAnonymous]
    [HttpPost("Bulk")]
    public IActionResult CreateBulk([FromBody] List<CurrencyRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Add at least one currency before saving.");

        var dup = requests.GroupBy(r => (r.Code ?? "").Trim().ToUpperInvariant())
                           .FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return BadRequest($"Currency code \"{dup.First().Code}\" appears more than once.");

        var existing = _repo.GetAll().Select(x => x.Code.ToUpper()).ToHashSet();
        var created = 0;
        foreach (var request in requests)
        {
            var err = Validate(request, null);
            if (err != null) return BadRequest(err);

            var code = request.Code.Trim().ToUpperInvariant();
            if (existing.Contains(code))
                return BadRequest($"Currency code \"{request.Code}\" already exists.");

            var entity = _mapper.Map<Currencies>(request);
            entity.CreatedAt = DateTime.UtcNow;
            _repo.Add(entity);
            existing.Add(code);
            created++;
        }
        _repo.Commit();
        return Ok(new { created, message = $"{created} currency(ies) created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CurrencyRequest request)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound($"Currency {id} not found.");

        var err = Validate(request, id);
        if (err != null) return BadRequest(err);

        _mapper.Map(request, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { id = entity.Id, message = "Currency updated." });
    }

    // Accepts either the numeric Id or the currency Code — the list page
    // deletes by code, so both keys need to resolve to the same row.
    [AllowAnonymous]
    [HttpDelete("{key}")]
    public IActionResult Delete(string key)
    {
        var trimmed = key.Trim();
        var entity = int.TryParse(trimmed, out var id)
            ? _repo.GetSingle(x => x.Id == id)
            : _repo.GetSingle(x => x.Code.ToUpper() == trimmed.ToUpper());

        if (entity == null) return NotFound($"Currency \"{key}\" not found.");

        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // `ignoreId` skips the row being updated during the uniqueness check.
    private string? Validate(CurrencyRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.Code)) return "Currency code is required.";
        if (string.IsNullOrWhiteSpace(r.CurrencyName)) return "Currency name is required.";

        var code = r.Code.Trim().ToUpperInvariant();
        var clash = _repo.GetAll().AsEnumerable()
            .Any(x => x.Code.ToUpperInvariant() == code && x.Id != ignoreId);
        if (clash) return $"Currency code \"{r.Code}\" already exists.";
        return null;
    }
}

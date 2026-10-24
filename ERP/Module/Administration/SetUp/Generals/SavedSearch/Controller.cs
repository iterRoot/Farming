using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SetUp.Generals.SavedSearch;

// ═══════════════════════════════════════════════════════════════════
// SAVED SEARCH / FORMATTED SEARCH  (route: /SavedSearch)
// GET    /SavedSearch                → list
// GET    /SavedSearch/Targets        → whitelist: entities, fields, operators
// GET    /SavedSearch/{id}
// POST   /SavedSearch                → create (validates Field/Operator
//                                       against the Registry, not just
//                                       that they're non-empty strings)
// PUT    /SavedSearch/{id}
// DELETE /SavedSearch/{id}
// POST   /SavedSearch/{id}/Execute   → run a saved search, real rows back
// POST   /SavedSearch/Preview        → run an unsaved filter set (live
//                                       preview while building one)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class SavedSearchController : ControllerBase
{
    private readonly ISavedSearchRepository _repository;
    private readonly IMapper                _mapper;
    private readonly MyDbContext             _db;

    public SavedSearchController(ISavedSearchRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Filters).OrderBy(x => x.Name).ToList();
        return Ok(_mapper.Map<List<SavedSearchResponse>>(list));
    }

    [HttpGet("Targets")]
    public IActionResult Targets()
    {
        var result = SearchableEntityRegistry.Targets.Values.Select(t => new SearchTargetOption
        {
            Key = t.Key,
            Label = t.Label,
            Fields = t.Fields.Select(f => new SearchFieldOption
            {
                Name = f.Name,
                Label = f.Label,
                Type = f.Type.ToString(),
                Operators = SearchableEntityRegistry.OperatorsByType[f.Type],
            }).ToList(),
        }).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Filters).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SavedSearchResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] SavedSearchRequest dto)
    {
        var error = Validate(dto, null);
        if (error != null) return BadRequest(error);

        var entity = new SavedSearch
        {
            Name = dto.Name.Trim(), TargetEntity = dto.TargetEntity, Description = dto.Description,
            IsActive = dto.IsActive, CreatedAt = DateTime.UtcNow, InActive = false,
        };
        AddFilters(entity, dto.Filters);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<SavedSearchResponse>(ReloadWithFilters(entity.Id)));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] SavedSearchRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Filters).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var error = Validate(dto, id);
        if (error != null) return BadRequest(error);

        entity.Filters.Clear();
        entity.Name = dto.Name.Trim(); entity.TargetEntity = dto.TargetEntity;
        entity.Description = dto.Description; entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        AddFilters(entity, dto.Filters);

        _repository.Commit();
        return Ok(_mapper.Map<SavedSearchResponse>(ReloadWithFilters(entity.Id)));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    [HttpPost("{id:int}/Execute")]
    public IActionResult Execute(int id)
    {
        var entity = _repository.GetAll().Include(x => x.Filters).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        var (error, rows) = Run(entity.TargetEntity, entity.Filters
            .Select(f => new SavedSearchFilterRequest { Field = f.Field, Operator = f.Operator, Value = f.Value }).ToList());
        return error != null ? BadRequest(error) : Ok(rows);
    }

    [HttpPost("Preview")]
    public IActionResult Preview([FromBody] SearchPreviewRequest dto)
    {
        var (error, rows) = Run(dto.TargetEntity, dto.Filters);
        return error != null ? BadRequest(error) : Ok(rows);
    }

    // ── The only place filters ever touch real data — pure in-memory,
    //    reflection-free comparisons against Registry-supplied delegates. ──
    private (string? error, List<SearchResultRow> rows) Run(string targetKey, List<SavedSearchFilterRequest> filters)
    {
        if (!SearchableEntityRegistry.Targets.TryGetValue(targetKey, out var target))
            return ($"Unknown search target '{targetKey}'.", new());

        var fieldsByName = target.Fields.ToDictionary(f => f.Name);
        foreach (var f in filters)
        {
            if (!fieldsByName.TryGetValue(f.Field, out var fieldDef))
                return ($"'{f.Field}' is not searchable on {target.Label}.", new());
            if (!SearchableEntityRegistry.OperatorsByType[fieldDef.Type].Contains(f.Operator))
                return ($"'{f.Operator}' is not valid for {fieldDef.Label} ({fieldDef.Type}).", new());
        }

        var rows = target.Load(_db);
        foreach (var f in filters)
        {
            var fieldDef = fieldsByName[f.Field];
            rows = rows.Where(o => Matches(fieldDef.Accessor(o), fieldDef.Type, f.Operator, f.Value)).ToList();
        }

        var result = rows.Select(o =>
        {
            var (id, display) = target.Summarize(o);
            return new SearchResultRow { Id = id, Display = display };
        }).Take(200).ToList(); // a formatted-search picker shows a short list, not a full table dump

        return (null, result);
    }

    private static bool Matches(object? rawValue, FieldType type, string op, string filterValue)
    {
        switch (type)
        {
            case FieldType.Text:
                var text = (rawValue as string ?? "").Trim();
                var target = filterValue.Trim();
                return op switch
                {
                    "Equals"     => string.Equals(text, target, StringComparison.OrdinalIgnoreCase),
                    "NotEquals"  => !string.Equals(text, target, StringComparison.OrdinalIgnoreCase),
                    "Contains"   => text.Contains(target, StringComparison.OrdinalIgnoreCase),
                    "StartsWith" => text.StartsWith(target, StringComparison.OrdinalIgnoreCase),
                    _ => false,
                };
            case FieldType.Number:
                if (rawValue == null || !decimal.TryParse(filterValue, out var num)) return false;
                var actual = Convert.ToDecimal(rawValue);
                return op switch
                {
                    "Equals"         => actual == num,
                    "NotEquals"      => actual != num,
                    "GreaterThan"    => actual > num,
                    "LessThan"       => actual < num,
                    "GreaterOrEqual" => actual >= num,
                    "LessOrEqual"    => actual <= num,
                    _ => false,
                };
            case FieldType.Boolean:
                if (rawValue is not bool b || !bool.TryParse(filterValue, out var wantBool)) return false;
                return b == wantBool;
            default:
                return false;
        }
    }

    private void AddFilters(SavedSearch entity, List<SavedSearchFilterRequest> filters)
    {
        var lineNum = 1;
        foreach (var f in filters)
            entity.Filters.Add(new SavedSearchFilter { LineNum = lineNum++, Field = f.Field, Operator = f.Operator, Value = f.Value });
    }

    private string? Validate(SavedSearchRequest dto, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return "Name is required.";
        var nameTrimmed = dto.Name.Trim();
        var nameTaken = _repository.GetAll()
            .Any(x => x.Name == nameTrimmed && (excludeId == null || x.Id != excludeId));
        if (nameTaken) return $"A saved search named '{nameTrimmed}' already exists.";
        if (!SearchableEntityRegistry.Targets.TryGetValue(dto.TargetEntity ?? "", out var target))
            return $"Target must be one of: {string.Join(", ", SearchableEntityRegistry.Targets.Keys)}.";
        var fieldsByName = target.Fields.ToDictionary(f => f.Name);
        foreach (var f in dto.Filters)
        {
            if (!fieldsByName.TryGetValue(f.Field, out var fieldDef))
                return $"'{f.Field}' is not searchable on {target.Label}.";
            if (!SearchableEntityRegistry.OperatorsByType[fieldDef.Type].Contains(f.Operator))
                return $"'{f.Operator}' is not valid for {fieldDef.Label} ({fieldDef.Type}).";
            if (string.IsNullOrWhiteSpace(f.Value)) return $"A value is required for {fieldDef.Label}.";
        }
        return null;
    }

    private SavedSearch ReloadWithFilters(int id) =>
        _repository.GetAll().Include(x => x.Filters).First(x => x.Id == id);
}

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.DunningTerm;

// ═══════════════════════════════════════════════════════════════════
// DUNNING TERM  (route: /DunningTerm)
// Master data only — the escalation ladder the Dunning wizard reads.
// GET    /DunningTerm
// GET    /DunningTerm/{id}
// POST   /DunningTerm
// PUT    /DunningTerm/{id}
// DELETE /DunningTerm/{id}   → refused if any BP or DunningLetter still
//                               references this term's code
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class DunningTermController : ControllerBase
{
    private readonly IDunningTermRepository _repository;
    private readonly IMapper                _mapper;
    private readonly MyDbContext             _db;

    public DunningTermController(IDunningTermRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Levels).OrderBy(x => x.Code).ToList();
        return Ok(_mapper.Map<List<DunningTermResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Levels).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<DunningTermResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] DunningTermRequest dto)
    {
        var error = Validate(dto, null);
        if (error != null) return BadRequest(error);

        var entity = new DunningTerm
        {
            Code = dto.Code.Trim(), Name = dto.Name.Trim(), IsActive = dto.IsActive,
            Remarks = dto.Remarks, CreatedAt = DateTime.UtcNow, InActive = false,
        };
        AddLevels(entity, dto.Levels);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<DunningTermResponse>(ReloadWithLevels(entity.Id)));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] DunningTermRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Levels).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var error = Validate(dto, id);
        if (error != null) return BadRequest(error);

        var oldCode = entity.Code;
        entity.Levels.Clear();
        entity.Code = dto.Code.Trim(); entity.Name = dto.Name.Trim();
        entity.IsActive = dto.IsActive; entity.Remarks = dto.Remarks;
        entity.UpdatedAt = DateTime.UtcNow;
        AddLevels(entity, dto.Levels);

        // Renaming the code would silently orphan every BP/letter that points at the old one.
        if (!string.Equals(oldCode, entity.Code, StringComparison.OrdinalIgnoreCase))
        {
            var inUse = _db.Set<BPEntity>().Any(b => b.DunningTerm == oldCode);
            if (inUse) return BadRequest($"Can't change the code while business partners are assigned to '{oldCode}'.");
        }

        _repository.Commit();
        return Ok(_mapper.Map<DunningTermResponse>(ReloadWithLevels(entity.Id)));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var inUseByBp = _db.Set<BPEntity>().Any(b => b.DunningTerm == entity.Code);
        if (inUseByBp) return BadRequest("Can't delete a dunning term while business partners are assigned to it.");

        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private void AddLevels(DunningTerm entity, List<DunningTermLevelRequest> levels)
    {
        foreach (var l in levels.OrderBy(l => l.Level))
            entity.Levels.Add(new DunningTermLevel { Level = l.Level, DaysOverdue = l.DaysOverdue, LetterText = l.LetterText });
    }

    private string? Validate(DunningTermRequest dto, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(dto.Code)) return "Code is required.";
        if (string.IsNullOrWhiteSpace(dto.Name)) return "Name is required.";
        var codeTrimmed = dto.Code.Trim();
        var codeTaken = _repository.GetAll().Any(x => x.Code == codeTrimmed && (excludeId == null || x.Id != excludeId));
        if (codeTaken) return $"A dunning term with code '{codeTrimmed}' already exists.";

        if (dto.Levels.Count == 0) return "At least one dunning level is required.";
        if (dto.Levels.Select(l => l.Level).Distinct().Count() != dto.Levels.Count)
            return "Level numbers must be unique.";
        if (dto.Levels.Any(l => l.Level <= 0)) return "Level numbers must be positive.";
        if (dto.Levels.Any(l => l.DaysOverdue < 0)) return "Days overdue can't be negative.";
        if (dto.Levels.Any(l => string.IsNullOrWhiteSpace(l.LetterText))) return "Every level needs letter text.";

        // Escalation only makes sense if later levels require more days overdue than earlier ones.
        var ordered = dto.Levels.OrderBy(l => l.Level).ToList();
        for (int i = 1; i < ordered.Count; i++)
            if (ordered[i].DaysOverdue <= ordered[i - 1].DaysOverdue)
                return $"Level {ordered[i].Level}'s days-overdue must be greater than level {ordered[i - 1].Level}'s.";

        return null;
    }

    private DunningTerm ReloadWithLevels(int id) =>
        _repository.GetAll().Include(x => x.Levels).First(x => x.Id == id);
}

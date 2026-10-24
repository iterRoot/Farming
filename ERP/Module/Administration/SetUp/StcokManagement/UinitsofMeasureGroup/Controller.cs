

// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Inventory.UomGroup;

[ApiController]
[Route("[controller]")]
public class UomGroupController : ControllerBase
{
    private readonly IUomGroupRepository _repository;
    private readonly IMapper             _mapper;
    private readonly MyDbContext         _db;

    public UomGroupController(IUomGroupRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines).OrderBy(x => x.Code).ToList();
        return Ok(_mapper.Map<List<UomGroupResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<UomGroupResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] UomGroupRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code))
            return BadRequest($"UoM Group code '{code}' already exists");

        var entity       = _mapper.Map<UomGroup>(dto);
        entity.Code      = code;
        entity.BaseUom   = dto.BaseUom.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        int ln = 1;
        foreach (var line in entity.Lines)
        {
            line.AltUom  = line.AltUom.Trim().ToUpper();
            line.LineNum = ln++;
        }

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<UomGroupResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UomGroupRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"UoM Group code '{code}' already exists");

        _db.RemoveRange(entity.Lines);
        _mapper.Map(dto, entity);
        entity.Code     = code;
        entity.BaseUom  = dto.BaseUom.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;

        int ln = 1;
        foreach (var line in entity.Lines)
        {
            line.AltUom  = line.AltUom.Trim().ToUpper();
            line.LineNum = ln++;
        }

        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<UomGroupResponse>(entity));
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

    // ── GET /UomGroup/{id}/Units ──────────────────────────────────
    // Every unit the group defines (base + alternates), each with how
    // many base units it equals — what a converter UI needs to list.
    [HttpGet("{id:int}/Units")]
    public IActionResult Units(int id)
    {
        var group = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (group == null) return NotFound();
        return Ok(UnitsOf(group));
    }

    // ── POST /UomGroup/Convert ────────────────────────────────────
    // Convert a quantity between two units of the same group, via the
    // base unit: result = qty * basePerOne(from) / basePerOne(to).
    [HttpPost("Convert")]
    public IActionResult Convert([FromBody] UomConvertRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FromUom) || string.IsNullOrWhiteSpace(dto.ToUom))
            return BadRequest("Both From and To units are required.");

        var group = _repository.GetAll().Include(x => x.Lines)
            .FirstOrDefault(x => dto.UomGroupId.HasValue
                ? x.Id == dto.UomGroupId.Value
                : x.Code == (dto.GroupCode ?? "").Trim().ToUpper());
        if (group == null) return BadRequest("UoM Group not found.");

        var units = UnitsOf(group).ToDictionary(u => u.Uom, u => u.BaseUnitsPerOne, StringComparer.OrdinalIgnoreCase);

        var from = dto.FromUom.Trim().ToUpper();
        var to   = dto.ToUom.Trim().ToUpper();
        if (!units.TryGetValue(from, out var fromFactor))
            return BadRequest($"'{dto.FromUom}' is not a unit in group '{group.Code}'.");
        if (!units.TryGetValue(to, out var toFactor))
            return BadRequest($"'{dto.ToUom}' is not a unit in group '{group.Code}'.");
        if (toFactor <= 0) return BadRequest($"'{dto.ToUom}' has an invalid conversion factor.");

        var baseQty = dto.Quantity * fromFactor;
        var result  = baseQty / toFactor;

        return Ok(new UomConvertResponse
        {
            GroupCode = group.Code,
            BaseUom   = group.BaseUom,
            FromUom   = from,
            ToUom     = to,
            Quantity  = dto.Quantity,
            Result    = Math.Round(result, 6),
            BaseQty   = Math.Round(baseQty, 6),
            Rate      = Math.Round(fromFactor / toFactor, 6),
        });
    }

    // base units per 1 of each unit: base = 1; an alt line says
    // AltQty [AltUom] = BaseQty [BaseUom], so 1 [AltUom] = BaseQty/AltQty base.
    private static List<UomUnitInfo> UnitsOf(UomGroup group)
    {
        var list = new List<UomUnitInfo>
        {
            new() { Uom = group.BaseUom, IsBase = true, BaseUnitsPerOne = 1m },
        };
        foreach (var l in group.Lines.Where(l => l.AltQty > 0))
        {
            if (string.Equals(l.AltUom, group.BaseUom, StringComparison.OrdinalIgnoreCase)) continue;
            list.Add(new UomUnitInfo
            {
                Uom = l.AltUom,
                IsBase = false,
                BaseUnitsPerOne = Math.Round(l.BaseQty / l.AltQty, 6),
            });
        }
        return list;
    }
}
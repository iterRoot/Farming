using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Sales.Territories;

[ApiController]
[Route("[controller]")]
public class TerritoryController : ControllerBase
{
    private readonly ITerritoryRepository _repo;
    private readonly IMapper              _mapper;

    public TerritoryController(ITerritoryRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /Territory ─────────────────────────────────────────
    // Returns flat list ordered by level then name
    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .Include(x => x.Children)
            .OrderBy(x => x.Level)
            .ThenBy(x => x.Name)
            .ToList();

        var flat = list.Select(t => BuildFlat(t, list)).ToList();
        return Ok(flat);
    }

    // ── GET /Territory/Tree ────────────────────────────────────
    // Returns nested tree (roots only, children embedded)
    [HttpGet("Tree")]
    public IActionResult GetTree()
    {
        var all = _repo.GetAll()
            .Include(x => x.Children)
            .ToList();

        var roots = all.Where(x => x.ParentId == null)
            .OrderBy(x => x.Name)
            .ToList();

        return Ok(_mapper.Map<List<TerritoryResponse>>(roots));
    }

    // ── GET /Territory/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Children)
            .Include(x => x.Parent)
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<TerritoryResponse>(e));
    }

    // ── GET /Territory/{id}/Children ──────────────────────────
    [HttpGet("{id:int}/Children")]
    public IActionResult GetChildren(int id)
    {
        var all  = _repo.GetAll().Include(x => x.Children).ToList();
        var kids = all.Where(x => x.ParentId == id).OrderBy(x => x.Name).ToList();
        return Ok(kids.Select(t => BuildFlat(t, all)));
    }

    // ── POST /Territory ────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] TerritoryRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Territory code '{code}' already exists");

        var entity     = _mapper.Map<Territory>(dto);
        entity.Code    = code;
        entity.CountryCode = dto.CountryCode?.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        // Denormalise parent info
        if (dto.ParentId.HasValue)
        {
            var parent = _repo.GetSingle(x => x.Id == dto.ParentId.Value);
            if (parent == null) return BadRequest($"Parent territory {dto.ParentId} does not exist.");
            {
                entity.ParentCode  = parent.Code;
                entity.ParentName  = parent.Name;
                entity.Level       = parent.Level + 1;
                entity.CountryCode ??= parent.CountryCode;
                entity.Region      ??= parent.Region;
            }
        }

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<TerritoryResponse>(entity));
    }

    // ── PUT /Territory/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] TerritoryRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Territory code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code        = code;
        entity.CountryCode = dto.CountryCode?.Trim().ToUpper();
        entity.UpdatedAt   = DateTime.UtcNow;

        if (dto.ParentId.HasValue)
        {
            var err = CheckParent(id, dto.ParentId.Value);
            if (err != null) return BadRequest(err);
            var parent = _repo.GetSingle(x => x.Id == dto.ParentId.Value);
            {
                entity.ParentCode = parent!.Code;
                entity.ParentName = parent.Name;
                entity.Level      = parent.Level + 1;
            }
        }
        else
        {
            entity.ParentCode = null;
            entity.ParentName = null;
            entity.Level      = 1;
        }

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<TerritoryResponse>(entity));
    }

    // ── POST /Territory/{id}/Move/{newParentId} ───────────────
    // Re-parent a territory node
    [HttpPost("{id:int}/Move/{newParentId:int}")]
    public IActionResult Move(int id, int newParentId)
    {
        var entity    = _repo.GetSingle(x => x.Id == id);
        var newParent = _repo.GetSingle(x => x.Id == newParentId);
        if (entity == null || newParent == null) return NotFound();
        var moveErr = CheckParent(id, newParentId);
        if (moveErr != null) return BadRequest(moveErr);

        entity.ParentId   = newParentId;
        entity.ParentCode = newParent.Code;
        entity.ParentName = newParent.Name;
        entity.Level      = newParent.Level + 1;
        entity.UpdatedAt  = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.Name}' moved under '{newParent.Name}'" });
    }

    // ── DELETE /Territory/{id} ─────────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (_repo.GetAll().Any(x => x.ParentId == id && x.Id != id))
            return BadRequest("Cannot delete a territory that has children. Reassign or delete children first.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // A territory can't sit under itself or under one of its own descendants.
    private string? CheckParent(int id, int parentId)
    {
        if (parentId == id) return "A territory can't be its own parent.";
        var all = _repo.GetAll().Select(x => new { x.Id, x.ParentId }).ToList();
        if (all.All(x => x.Id != parentId)) return $"Parent territory {parentId} does not exist.";
        var seen = new HashSet<int>();
        int? cur = parentId;
        while (cur != null && seen.Add(cur.Value))
        {
            if (cur == id) return "A territory can't be moved under one of its own sub-territories.";
            cur = all.FirstOrDefault(x => x.Id == cur)?.ParentId;
        }
        return null;
    }

    // ─── Helper: build full path + flat response ─────────────
    private TerritoryFlatResponse BuildFlat(Territory t, List<Territory> all)
    {
        var parts = new List<string> { t.Name };
        var cur   = t;
        var seen  = new HashSet<int> { t.Id };
        while (cur.ParentId != null)
        {
            cur = all.FirstOrDefault(x => x.Id == cur.ParentId)!;
            if (cur == null || !seen.Add(cur.Id)) break;   // stop on a parent loop in bad data
            parts.Insert(0, cur.Name);
        }
        var flat     = _mapper.Map<TerritoryFlatResponse>(t);
        flat.FullPath = string.Join(" › ", parts);
        return flat;
    }
}
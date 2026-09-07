using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.BusinessPartners.BPProperties;

[ApiController]
[Route("[controller]")]
public class BPPropertyController : ControllerBase
{
    private readonly IBPPropertyRepository _repo;
    private readonly IMapper               _mapper;

    public BPPropertyController(IBPPropertyRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /BPProperty ───────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll([FromQuery] string? appliesTo = null)
    {
        var query = _repo.GetAll()
            .Where(x => appliesTo == null
                     || x.AppliesTo == appliesTo
                     || x.AppliesTo == "Both")
            .OrderBy(x => x.PropertyNo == 0 ? int.MaxValue : x.PropertyNo)
            .ThenBy(x => x.Code)
            .ToList();
        return Ok(_mapper.Map<List<BPPropertyResponse>>(query));
    }

    // ── GET /BPProperty/{id} ──────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<BPPropertyResponse>(e));
    }

    // ── POST /BPProperty ──────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] BPPropertyRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"BP property code '{code}' already exists");

        // Auto-assign next property number if not provided
        if (dto.PropertyNo <= 0)
        {
            var max = _repo.GetAll().Any()
                ? _repo.GetAll().Max(x => x.PropertyNo) : 0;
            dto.PropertyNo = max + 1;
        }

        var entity       = _mapper.Map<BPProperty>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<BPPropertyResponse>(entity));
    }

    // ── PUT /BPProperty/{id} ──────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BPPropertyRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"BP property code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<BPPropertyResponse>(entity));
    }

    // ── DELETE /BPProperty/{id} ───────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }
}
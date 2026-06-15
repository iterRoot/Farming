using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Administration.UserDefaults;

[ApiController]
[Route("[controller]")]
public class UserDefaultController : ControllerBase
{
    private readonly IUserDefaultRepository _repo;
    private readonly IMapper                _mapper;

    public UserDefaultController(IUserDefaultRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /UserDefault ───────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll().OrderBy(x => x.UserCode).ToList();
        return Ok(_mapper.Map<List<UserDefaultResponse>>(list));
    }

    // ── GET /UserDefault/{id} ─────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<UserDefaultResponse>(e));
    }

    // ── GET /UserDefault/ByCode/{code} ────────────────────────
    [AllowAnonymous][HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var e = _repo.GetSingle(x => x.UserCode == code.Trim().ToUpper());
        if (e == null) return NotFound(new { message = $"No defaults found for user '{code}'" });
        return Ok(_mapper.Map<UserDefaultResponse>(e));
    }

    // ── POST /UserDefault ─────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] UserDefaultRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.UserCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.UserCode == code))
            return BadRequest($"Defaults for user '{code}' already exist. Use PUT to update.");

        var entity       = _mapper.Map<UserDefault>(dto);
        entity.UserCode  = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<UserDefaultResponse>(entity));
    }

    // ── PUT /UserDefault/{id} ─────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UserDefaultRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.UserCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.UserCode == code && x.Id != id))
            return BadRequest($"User code '{code}' already assigned to another user.");

        _mapper.Map(dto, entity);
        entity.UserCode  = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<UserDefaultResponse>(entity));
    }

    // ── PUT /UserDefault/Upsert ───────────────────────────────
    // Create or update by UserCode — useful for single-user self-update
    [AllowAnonymous][HttpPut("Upsert")]
    public IActionResult Upsert([FromBody] UserDefaultRequest dto)
    {
        var code     = dto.UserCode.Trim().ToUpper();
        var existing = _repo.GetSingle(x => x.UserCode == code);

        if (existing == null)
        {
            var newEntity       = _mapper.Map<UserDefault>(dto);
            newEntity.UserCode  = code;
            newEntity.CreatedAt = DateTime.UtcNow;
            newEntity.InActive  = false;
            _repo.Add(newEntity);
            _repo.Commit();
            return Ok(_mapper.Map<UserDefaultResponse>(newEntity));
        }

        _mapper.Map(dto, existing);
        existing.UserCode  = code;
        existing.UpdatedAt = DateTime.UtcNow;
        _repo.Update(existing);
        _repo.Commit();
        return Ok(_mapper.Map<UserDefaultResponse>(existing));
    }

    // ── POST /UserDefault/{id}/CopyTo/{targetCode} ────────────
    // Clone one user's defaults to another user
    [AllowAnonymous][HttpPost("{id:int}/CopyTo/{targetCode}")]
    public IActionResult CopyTo(int id, string targetCode)
    {
        var source = _repo.GetSingle(x => x.Id == id);
        if (source == null) return NotFound();

        var code = targetCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.UserCode == code))
            return BadRequest($"Defaults for '{code}' already exist. Delete them first or use PUT.");

        var copy           = _mapper.Map<UserDefaultRequest>(source);
        copy.UserCode      = code;
        copy.FullName      = $"{source.FullName} (Copy)";

        var entity         = _mapper.Map<UserDefault>(copy);
        entity.UserCode    = code;
        entity.CreatedAt   = DateTime.UtcNow;
        entity.InActive    = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<UserDefaultResponse>(entity));
    }

    // ── DELETE /UserDefault/{id} ──────────────────────────────
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
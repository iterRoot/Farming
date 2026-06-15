
// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Inventory.Locations;

[ApiController]
[Route("[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationRepository _repository;
    private readonly IMapper             _mapper;

    public LocationController(ILocationRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll([FromQuery] string? warehouse = null)
    {
        var query = _repository.GetAll()
            .Where(x => warehouse == null || x.WarehouseCode == warehouse)
            .OrderBy(x => x.WarehouseCode)
            .ThenBy(x => x.Aisle)
            .ThenBy(x => x.Row)
            .ThenBy(x => x.Shelf)
            .ThenBy(x => x.Bin)
            .ToList();
        return Ok(_mapper.Map<List<LocationResponse>>(query));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<LocationResponse>(e));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] LocationRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (_repository.GetAll().Any(x => x.Code == dto.Code.Trim()))
            return BadRequest($"Location code '{dto.Code}' already exists");

        var entity            = _mapper.Map<Location>(dto);
        entity.Code           = dto.Code.Trim().ToUpper();
        entity.WarehouseCode  = dto.WarehouseCode?.Trim().ToUpper();
        entity.CreatedAt      = DateTime.UtcNow;
        entity.InActive       = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<LocationResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] LocationRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (_repository.GetAll().Any(x => x.Code == dto.Code.Trim() && x.Id != id))
            return BadRequest($"Location code '{dto.Code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code          = dto.Code.Trim().ToUpper();
        entity.WarehouseCode = dto.WarehouseCode?.Trim().ToUpper();
        entity.UpdatedAt     = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<LocationResponse>(entity));
    }

    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }
}
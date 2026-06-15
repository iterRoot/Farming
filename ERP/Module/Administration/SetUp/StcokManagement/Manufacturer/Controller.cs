

// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Inventory.Manufacturers;

[ApiController]
[Route("[controller]")]
public class ManufacturerController : ControllerBase
{
    private readonly IManufacturerRepository _repository;
    private readonly IMapper                 _mapper;

    public ManufacturerController(IManufacturerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().OrderBy(x => x.Name).ToList();
        return Ok(_mapper.Map<List<ManufacturerResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ManufacturerResponse>(e));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ManufacturerRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (_repository.GetAll().Any(x => x.Code == dto.Code.Trim()))
            return BadRequest($"Manufacturer code '{dto.Code}' already exists");

        var entity       = _mapper.Map<Manufacturer>(dto);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ManufacturerResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ManufacturerRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (_repository.GetAll().Any(x => x.Code == dto.Code.Trim() && x.Id != id))
            return BadRequest($"Manufacturer code '{dto.Code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ManufacturerResponse>(entity));
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
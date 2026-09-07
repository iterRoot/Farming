

// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Inventory.PackageTypes;

[ApiController]
[Route("[controller]")]
public class PackageTypeController : ControllerBase
{
    private readonly IPackageTypeRepository _repository;
    private readonly IMapper                _mapper;

    public PackageTypeController(IPackageTypeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().OrderBy(x => x.Name).ToList();
        return Ok(_mapper.Map<List<PackageTypeResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<PackageTypeResponse>(e));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] PackageTypeRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code))
            return BadRequest($"Package type code '{code}' already exists");

        var entity       = _mapper.Map<PackageType>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<PackageTypeResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PackageTypeRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Package type code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<PackageTypeResponse>(entity));
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
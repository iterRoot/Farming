

// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Inventory.ItemProperties;

[ApiController]
[Route("[controller]")]
public class ItemPropertyController : ControllerBase
{
    private readonly IItemPropertyRepository _repository;
    private readonly IMapper                 _mapper;

    public ItemPropertyController(IItemPropertyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll()
            .OrderBy(x => x.PropertyNo == 0 ? int.MaxValue : x.PropertyNo)
            .ThenBy(x => x.Code)
            .ToList();
        return Ok(_mapper.Map<List<ItemPropertyResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ItemPropertyResponse>(e));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ItemPropertyRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code))
            return BadRequest($"Property code '{code}' already exists");

        // Auto-assign next property number if not provided
        if (dto.PropertyNo <= 0)
        {
            var max = _repository.GetAll().Any()
                ? _repository.GetAll().Max(x => x.PropertyNo) : 0;
            dto.PropertyNo = max + 1;
        }

        var entity       = _mapper.Map<ItemProperty>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ItemPropertyResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ItemPropertyRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Property code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ItemPropertyResponse>(entity));
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
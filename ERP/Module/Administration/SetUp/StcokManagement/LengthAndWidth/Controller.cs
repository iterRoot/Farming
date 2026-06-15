
// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Inventory.LengthWidth;

[ApiController]
[Route("[controller]")]
public class LengthWidthUomController : ControllerBase
{
    private readonly ILengthWidthUomRepository _repository;
    private readonly IMapper                   _mapper;

    public LengthWidthUomController(ILengthWidthUomRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().OrderBy(x => x.ConversionToMeter).ToList();
        return Ok(_mapper.Map<List<LengthWidthUomResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<LengthWidthUomResponse>(e));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] LengthWidthUomRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (_repository.GetAll().Any(x => x.Code == dto.Code.Trim()))
            return BadRequest($"Unit code '{dto.Code}' already exists");

        var entity       = _mapper.Map<LengthWidthUom>(dto);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<LengthWidthUomResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] LengthWidthUomRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (_repository.GetAll().Any(x => x.Code == dto.Code.Trim() && x.Id != id))
            return BadRequest($"Unit code '{dto.Code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<LengthWidthUomResponse>(entity));
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
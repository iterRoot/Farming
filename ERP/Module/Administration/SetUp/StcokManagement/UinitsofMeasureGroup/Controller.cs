

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

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines).OrderBy(x => x.Code).ToList();
        return Ok(_mapper.Map<List<UomGroupResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<UomGroupResponse>(e));
    }

    [AllowAnonymous][HttpPost]
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

    [AllowAnonymous][HttpPut("{id:int}")]
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
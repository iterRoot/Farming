using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Sale.Warehouse;

// ═══════════════════════════════════════════════════════════════
// WAREHOUSE  (route: /Warehouse)
// GET    /Warehouse        → all warehouses
// GET    /Warehouse/{id}   → one
// POST   /Warehouse        → create
// PUT    /Warehouse/{id}   → update
// DELETE /Warehouse/{id}   → delete
// ═══════════════════════════════════════════════════════════════
public class WarehouseController : MyController
{
    private readonly IMapper _mapper;
    private readonly IWarehouseRepository _repository;

    public WarehouseController(IWarehouseRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var list = _repository.GetAll()
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Code);
        var results = _mapper.ProjectTo<WarehouseListResponse>(list).ToList();
        return Ok(results);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(e => e.Id == id);
        if (item == null) return NotFound($"Warehouse {id} not found");
        return Ok(_mapper.Map<WarehouseListResponse>(item));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] WarehouseListRequest request)
    {
        var err = Validate(request, null);
        if (err != null) return BadRequest(err);

        var entity = _mapper.Map<Warehouse>(request);
        entity.Code = request.Code.Trim();
        entity.Name = request.Name.Trim();
        entity.InActive = !request.IsActive;
        entity.CreatedAt = DateTime.UtcNow;

        _repository.Add(entity);
        _repository.Commit();

        if (entity.IsDefault) { ClearOtherDefaults(entity.Id); _repository.Commit(); }

        return Ok(new { id = entity.Id, code = entity.Code, message = "Warehouse created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] WarehouseUpdateRequest request)
    {
        var item = _repository.GetSingle(e => e.Id == id);
        if (item == null) return NotFound($"Warehouse {id} not found");

        var err = Validate(request, id);
        if (err != null) return BadRequest(err);

        _mapper.Map(request, item);
        item.Code = request.Code.Trim();
        item.Name = request.Name.Trim();
        item.InActive = !request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        _repository.Update(item);
        _repository.Commit();

        if (item.IsDefault) { ClearOtherDefaults(item.Id); _repository.Commit(); }

        return Ok(new { id = item.Id, message = "Warehouse updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(e => e.Id == id);
        if (item == null) return NotFound($"Warehouse {id} not found");
        _repository.Remove(item);
        _repository.Commit();
        return NoContent();
    }

    // Only one warehouse may be the default.
    private void ClearOtherDefaults(int keepId)
    {
        foreach (var w in _repository.GetAll().Where(x => x.IsDefault && x.Id != keepId))
        {
            w.IsDefault = false;
            _repository.Update(w);
        }
    }

    private string? Validate(WarehouseListRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.Code)) return "Warehouse Code is required.";
        if (string.IsNullOrWhiteSpace(r.Name)) return "Warehouse Name is required.";
        var code = r.Code.Trim().ToLowerInvariant();
        var clash = _repository.GetAll().AsEnumerable()
            .Any(x => x.Code.ToLowerInvariant() == code && x.Id != ignoreId);
        if (clash) return $"Warehouse Code \"{r.Code}\" already exists.";
        return null;
    }
}

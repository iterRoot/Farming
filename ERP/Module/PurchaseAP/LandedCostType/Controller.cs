using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.LandedCostType;

public class LandedCostTypeController : MyController
{
    private readonly ILandedCostTypeRepository _repository;
    public LandedCostTypeController(ILandedCostTypeRepository repository) => _repository = repository;

    private static LandedCostTypeDto ToDto(LandedCostType e) => new()
    {
        Id = e.Id, Name = e.Name, Remarks = e.Remarks, Active = e.Active,
    };

    [HttpGet]
    public IActionResult Gets() => Ok(_repository.GetAll().OrderBy(x => x.Name).ToList().Select(ToDto));

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        return item == null ? NotFound($"Item not found: {id}") : Ok(ToDto(item));
    }

    [HttpPost]
    public IActionResult Insert([FromBody] LandedCostTypeDto dto)
    {
        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        if (_repository.Existed(x => x.Name == name)) return BadRequest($"'{name}' already exists.");

        var item = new LandedCostType
        {
            Name = name, Remarks = dto!.Remarks?.Trim(), Active = dto.Active, CreatedAt = DateTime.UtcNow,
        };
        _repository.Add(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] LandedCostTypeDto dto)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");

        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        if (name != item.Name && _repository.Existed(x => x.Name == name)) return BadRequest($"'{name}' already exists.");

        item.Name = name; item.Remarks = dto!.Remarks?.Trim(); item.Active = dto.Active;
        item.UpdatedAt = DateTime.UtcNow;
        _repository.Update(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");
        _repository.Remove(item);
        _repository.Commit();
        return NoContent();
    }
}

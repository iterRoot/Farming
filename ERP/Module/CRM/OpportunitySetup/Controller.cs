using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.CRM.OpportunitySetup;

// ── GET/POST/PUT/DELETE /OpportunityStage ──────────────────────────────
public class OpportunityStageController : MyController
{
    private readonly IOpportunityStageSetupRepository _repository;
    public OpportunityStageController(IOpportunityStageSetupRepository repository) => _repository = repository;

    private static OpportunityStageDto ToDto(OpportunityStageSetup e) => new()
    {
        Id = e.Id, Name = e.Name, SortOrder = e.SortOrder, Probability = e.Probability, Active = e.Active,
    };

    [HttpGet]
    public IActionResult Gets() =>
        Ok(_repository.GetAll().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList().Select(ToDto));

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        return item == null ? NotFound($"Item not found: {id}") : Ok(ToDto(item));
    }

    [HttpPost]
    public IActionResult Insert([FromBody] OpportunityStageDto dto)
    {
        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        if (_repository.Existed(x => x.Name == name)) return BadRequest($"'{name}' already exists.");

        var item = new OpportunityStageSetup
        {
            Name = name, SortOrder = dto!.SortOrder, Probability = dto.Probability, Active = dto.Active,
            CreatedAt = DateTime.UtcNow,
        };
        _repository.Add(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] OpportunityStageDto dto)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");

        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        if (name != item.Name && _repository.Existed(x => x.Name == name)) return BadRequest($"'{name}' already exists.");

        item.Name = name; item.SortOrder = dto!.SortOrder; item.Probability = dto.Probability; item.Active = dto.Active;
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

// ── GET/POST/PUT/DELETE /OpportunityReason ─────────────────────────────
public class OpportunityReasonController : MyController
{
    private readonly IOpportunityReasonSetupRepository _repository;
    public OpportunityReasonController(IOpportunityReasonSetupRepository repository) => _repository = repository;

    private static OpportunityReasonDto ToDto(OpportunityReasonSetup e) => new()
    {
        Id = e.Id, Name = e.Name, Type = e.Type, Active = e.Active,
    };

    [HttpGet]
    public IActionResult Gets() =>
        Ok(_repository.GetAll().OrderBy(x => x.Type).ThenBy(x => x.Name).ToList().Select(ToDto));

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        return item == null ? NotFound($"Item not found: {id}") : Ok(ToDto(item));
    }

    [HttpPost]
    public IActionResult Insert([FromBody] OpportunityReasonDto dto)
    {
        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        var type = dto!.Type == "Won" ? "Won" : "Lost";

        var item = new OpportunityReasonSetup { Name = name, Type = type, Active = dto.Active, CreatedAt = DateTime.UtcNow };
        _repository.Add(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] OpportunityReasonDto dto)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");

        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");

        item.Name = name; item.Type = dto!.Type == "Won" ? "Won" : "Lost"; item.Active = dto.Active;
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

// ── GET/POST/PUT/DELETE /Competitor ─────────────────────────────────────
public class CompetitorController : MyController
{
    private readonly ICompetitorMasterRepository _repository;
    public CompetitorController(ICompetitorMasterRepository repository) => _repository = repository;

    private static CompetitorDto ToDto(CompetitorMaster e) => new()
    {
        Id = e.Id, Name = e.Name, Strengths = e.Strengths, Weaknesses = e.Weaknesses, Active = e.Active,
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
    public IActionResult Insert([FromBody] CompetitorDto dto)
    {
        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        if (_repository.Existed(x => x.Name == name)) return BadRequest($"'{name}' already exists.");

        var item = new CompetitorMaster
        {
            Name = name, Strengths = dto!.Strengths?.Trim(), Weaknesses = dto.Weaknesses?.Trim(),
            Active = dto.Active, CreatedAt = DateTime.UtcNow,
        };
        _repository.Add(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CompetitorDto dto)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");

        var name = (dto?.Name ?? "").Trim();
        if (name.Length == 0) return BadRequest("Name is required.");
        if (name != item.Name && _repository.Existed(x => x.Name == name)) return BadRequest($"'{name}' already exists.");

        item.Name = name; item.Strengths = dto!.Strengths?.Trim(); item.Weaknesses = dto.Weaknesses?.Trim(); item.Active = dto.Active;
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



// ═══════════════════════════════════════════════════════════════
// Controller.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Inventory.StockCycles;

[ApiController]
[Route("[controller]")]
public class StockCycleController : ControllerBase
{
    private readonly IStockCycleRepository _repository;
    private readonly IMapper               _mapper;

    public StockCycleController(IStockCycleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll()
            .OrderByDescending(x => x.StartDate)
            .ToList();
        return Ok(_mapper.Map<List<StockCycleResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<StockCycleResponse>(e));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] StockCycleRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code))
            return BadRequest($"Stock cycle code '{code}' already exists");

        var entity       = _mapper.Map<StockCycle>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<StockCycleResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StockCycleRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Completed")
            return BadRequest("Cannot edit a Completed stock cycle");

        var code = dto.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Stock cycle code '{code}' already exists");

        var existingCompleted = entity.CompletedDate;
        _mapper.Map(dto, entity);
        entity.Code          = code;
        entity.CompletedDate = existingCompleted;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<StockCycleResponse>(entity));
    }

    // ── POST /StockCycle/{id}/Start ───────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Start")]
    public IActionResult Start(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Open") return BadRequest("Only Open cycles can be started");

        entity.Status    = "InProgress";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Stock cycle started", code = entity.Code });
    }

    // ── POST /StockCycle/{id}/Complete ────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Complete")]
    public IActionResult Complete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Completed") return BadRequest("Already completed");

        entity.Status        = "Completed";
        entity.CompletedDate = DateTime.UtcNow;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Stock cycle completed", code = entity.Code });
    }

    // ── POST /StockCycle/{id}/Cancel ──────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Completed") return BadRequest("Cannot cancel a Completed cycle");

        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Stock cycle cancelled", code = entity.Code });
    }

    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Completed")
            return BadRequest("Cannot delete a Completed stock cycle");
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }
}
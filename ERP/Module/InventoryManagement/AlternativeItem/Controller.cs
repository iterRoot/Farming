using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;

namespace FarmingApi.Modules.InventoryManagement.AlternativeItem;

// ═══════════════════════════════════════════════════════════════════
// ALTERNATIVE ITEM  (route: /AlternativeItem)
// GET /AlternativeItem?item=CODE  — all substitutes (optionally for one item)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class AlternativeItemController : ControllerBase
{
    private readonly IAlternativeItemRepository _repository;
    private readonly IMapper     _mapper;
    private readonly MyDbContext _db;

    public AlternativeItemController(IAlternativeItemRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? item = null)
    {
        var q = _repository.GetAll();
        if (!string.IsNullOrWhiteSpace(item))
            q = q.Where(x => x.ItemCode == item);
        var list = q.OrderBy(x => x.ItemCode).ThenBy(x => x.Ranking).ToList();

        var names = _db.Set<ItemEntity>().ToDictionary(i => i.ItemCode, i => i.ItemName);
        var res = _mapper.Map<List<AlternativeItemResponse>>(list);
        foreach (var r in res) r.ItemName = names.GetValueOrDefault(r.ItemCode);
        return Ok(res);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<AlternativeItemResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] AlternativeItemRequest dto)
    {
        var (error, altName) = Validate(dto, null);
        if (error != null) return BadRequest(error);

        var entity = new AlternativeItem
        {
            ItemCode    = dto.ItemCode,
            AltItemCode = dto.AltItemCode,
            AltItemName = altName,
            Factor      = dto.Factor <= 0 ? 1m : dto.Factor,
            Ranking     = dto.Ranking,
            Criteria    = dto.Criteria,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<AlternativeItemResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] AlternativeItemRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var (error, altName) = Validate(dto, id);
        if (error != null) return BadRequest(error);

        entity.ItemCode    = dto.ItemCode;
        entity.AltItemCode = dto.AltItemCode;
        entity.AltItemName = altName;
        entity.Factor      = dto.Factor <= 0 ? 1m : dto.Factor;
        entity.Ranking     = dto.Ranking;
        entity.Criteria    = dto.Criteria;
        entity.UpdatedAt   = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<AlternativeItemResponse>(entity));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private (string? error, string? altName) Validate(AlternativeItemRequest dto, int? id)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemCode))    return ("Item is required.", null);
        if (string.IsNullOrWhiteSpace(dto.AltItemCode)) return ("Alternative item is required.", null);
        if (dto.ItemCode == dto.AltItemCode)            return ("An item can't be its own alternative.", null);

        var item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == dto.ItemCode);
        if (item == null) return ($"Item '{dto.ItemCode}' does not exist.", null);
        var alt = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == dto.AltItemCode);
        if (alt == null) return ($"Alternative item '{dto.AltItemCode}' does not exist.", null);

        if (_repository.GetAll().Any(x => x.ItemCode == dto.ItemCode && x.AltItemCode == dto.AltItemCode && (id == null || x.Id != id)))
            return ($"'{dto.AltItemCode}' is already an alternative for '{dto.ItemCode}'.", null);

        return (null, alt.ItemName);
    }
}

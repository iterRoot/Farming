using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using WarehouseEntity = FarmingApi.Modules.Sale.Warehouse.Warehouse;

namespace FarmingApi.Modules.InventoryManagement.BinLocation;

// ═══════════════════════════════════════════════════════════════════
// BIN LOCATION  (route: /BinLocation)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class BinLocationController : ControllerBase
{
    private readonly IBinLocationRepository _repository;
    private readonly IMapper     _mapper;
    private readonly MyDbContext _db;

    public BinLocationController(IBinLocationRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? warehouse = null)
    {
        var q = _repository.GetAll();
        if (!string.IsNullOrWhiteSpace(warehouse))
            q = q.Where(x => x.WarehouseCode == warehouse);
        var list = q.OrderBy(x => x.WarehouseCode).ThenBy(x => x.Code).ToList();

        var names = _db.Set<WarehouseEntity>().ToDictionary(w => w.Code, w => w.Name);
        var res = _mapper.Map<List<BinLocationResponse>>(list);
        foreach (var r in res) r.WarehouseName = names.GetValueOrDefault(r.WarehouseCode);
        return Ok(res);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        var res = _mapper.Map<BinLocationResponse>(e);
        res.WarehouseName = _db.Set<WarehouseEntity>()
            .Where(w => w.Code == e.WarehouseCode).Select(w => w.Name).FirstOrDefault();
        return Ok(res);
    }

    [HttpPost]
    public IActionResult Create([FromBody] BinLocationRequest dto)
    {
        var error = Validate(dto, null);
        if (error != null) return BadRequest(error);

        var entity = new BinLocation
        {
            Code           = dto.Code.Trim(),
            WarehouseCode  = dto.WarehouseCode,
            Description    = dto.Description,
            Sublevel1      = dto.Sublevel1,
            Sublevel2      = dto.Sublevel2,
            IsActive       = dto.IsActive,
            IsReceivingBin = dto.IsReceivingBin,
            MaxWeight      = dto.MaxWeight,
            CreatedAt      = DateTime.UtcNow,
            InActive       = false,
        };
        EnsureSingleReceivingBin(entity);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<BinLocationResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BinLocationRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var error = Validate(dto, id);
        if (error != null) return BadRequest(error);

        entity.Code           = dto.Code.Trim();
        entity.WarehouseCode  = dto.WarehouseCode;
        entity.Description    = dto.Description;
        entity.Sublevel1      = dto.Sublevel1;
        entity.Sublevel2      = dto.Sublevel2;
        entity.IsActive       = dto.IsActive;
        entity.IsReceivingBin = dto.IsReceivingBin;
        entity.MaxWeight      = dto.MaxWeight;
        entity.UpdatedAt      = DateTime.UtcNow;
        EnsureSingleReceivingBin(entity);

        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<BinLocationResponse>(entity));
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

    private string? Validate(BinLocationRequest dto, int? id)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))          return "Bin code is required.";
        if (string.IsNullOrWhiteSpace(dto.WarehouseCode)) return "Warehouse is required.";
        if (!_db.Set<WarehouseEntity>().Any(w => w.Code == dto.WarehouseCode))
            return $"Warehouse '{dto.WarehouseCode}' does not exist.";
        var code = dto.Code.Trim();
        if (_repository.GetAll().Any(x => x.Code == code && (id == null || x.Id != id)))
            return $"Bin code '{code}' already exists.";
        return null;
    }

    // At most one receiving bin per warehouse — clear the flag on the others.
    private void EnsureSingleReceivingBin(BinLocation entity)
    {
        if (!entity.IsReceivingBin) return;
        var others = _repository.GetAll()
            .Where(x => x.WarehouseCode == entity.WarehouseCode && x.IsReceivingBin && x.Id != entity.Id)
            .ToList();
        foreach (var o in others) { o.IsReceivingBin = false; _repository.Update(o); }
    }
}

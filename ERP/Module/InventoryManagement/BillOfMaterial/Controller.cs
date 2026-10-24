using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using WarehouseEntity = FarmingApi.Modules.Sale.Warehouse.Warehouse;
using ProdOrder = FarmingApi.Modules.InventoryManagement.ProductionOrder.ProductionOrder;

namespace FarmingApi.Modules.InventoryManagement.BillOfMaterial;

// ═══════════════════════════════════════════════════════════════════
// BILL OF MATERIAL  (route: /BillOfMaterial)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class BillOfMaterialController : ControllerBase
{
    private readonly IBillOfMaterialRepository _repository;
    private readonly IMapper     _mapper;
    private readonly MyDbContext _db;

    public BillOfMaterialController(IBillOfMaterialRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines).OrderBy(x => x.Code).ToList();
        return Ok(_mapper.Map<List<BillOfMaterialResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<BillOfMaterialResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] BillOfMaterialRequest dto)
    {
        var (error, parent) = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new BillOfMaterial
        {
            Code           = NextCode(),
            ParentItemCode = parent!.ItemCode,
            ParentItemName = parent.ItemName,
            Quantity       = dto.Quantity,
            Warehouse      = dto.Warehouse,
            IsActive       = dto.IsActive,
            Remarks        = dto.Remarks,
            CreatedAt      = DateTime.UtcNow,
            InActive       = false,
        };
        AddLines(entity, dto);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<BillOfMaterialResponse>(ReloadWithLines(entity.Id)));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BillOfMaterialRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var (error, parent) = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.Lines.Clear();
        entity.ParentItemCode = parent!.ItemCode;
        entity.ParentItemName = parent.ItemName;
        entity.Quantity       = dto.Quantity;
        entity.Warehouse      = dto.Warehouse;
        entity.IsActive       = dto.IsActive;
        entity.Remarks        = dto.Remarks;
        entity.UpdatedAt      = DateTime.UtcNow;
        AddLines(entity, dto);

        _repository.Commit();
        return Ok(_mapper.Map<BillOfMaterialResponse>(ReloadWithLines(entity.Id)));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var inUse = _db.Set<ProdOrder>().Any(p => p.BillOfMaterialId == id);
        if (inUse) return BadRequest("This BOM has Production Orders referencing it and cannot be deleted.");

        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private (string? error, ItemEntity? parent) Validate(BillOfMaterialRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ParentItemCode)) return ("Parent item is required.", null);
        if (dto.Quantity <= 0) return ("Quantity must be greater than zero.", null);
        if (dto.Lines == null || dto.Lines.Count == 0) return ("Add at least one component line.", null);
        if (string.IsNullOrWhiteSpace(dto.Warehouse)) return ("Warehouse is required.", null);
        if (!_db.Set<WarehouseEntity>().Any(w => w.Code == dto.Warehouse))
            return ($"Warehouse '{dto.Warehouse}' does not exist.", null);

        var parent = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == dto.ParentItemCode);
        if (parent == null) return ($"Item '{dto.ParentItemCode}' does not exist.", null);

        if (dto.Lines.Any(l => l.ItemCode == dto.ParentItemCode))
            return ("A BOM cannot use its own parent item as a component.", null);
        if (dto.Lines.Any(l => l.Quantity <= 0))
            return ("Every component quantity must be greater than zero.", null);

        var codes = dto.Lines.Select(l => l.ItemCode).ToList();
        var found = _db.Set<ItemEntity>().Where(i => codes.Contains(i.ItemCode)).Select(i => i.ItemCode).ToHashSet();
        var missing = codes.Distinct().Where(c => !found.Contains(c)).ToList();
        if (missing.Count > 0) return ($"Component item(s) not found: {string.Join(", ", missing)}.", null);

        return (null, parent);
    }

    private void AddLines(BillOfMaterial entity, BillOfMaterialRequest dto)
    {
        var items = _db.Set<ItemEntity>().ToDictionary(i => i.ItemCode, i => i.ItemName);
        var lineNum = 1;
        foreach (var l in dto.Lines)
        {
            entity.Lines.Add(new BillOfMaterialLine
            {
                LineNum  = lineNum++,
                ItemCode = l.ItemCode,
                ItemName = items.GetValueOrDefault(l.ItemCode, l.ItemCode),
                Quantity = l.Quantity,
            });
        }
    }

    private string NextCode()
    {
        var highest = _repository.GetAll().Select(b => b.Code).ToList()
            .Select(c => int.TryParse(c.Replace("BOM-", ""), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"BOM-{highest + 1:D4}";
    }

    private BillOfMaterial ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}

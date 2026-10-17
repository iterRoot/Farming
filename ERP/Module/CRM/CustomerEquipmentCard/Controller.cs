using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.CRM.CustomerEquipmentCard;

// ═══════════════════════════════════════════════════════════════════
// CUSTOMER EQUIPMENT CARD  (route: /CustomerEquipmentCard)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class CustomerEquipmentCardController : ControllerBase
{
    private readonly ICustomerEquipmentCardRepository _repository;
    private readonly IMapper     _mapper;
    private readonly MyDbContext _db;

    public CustomerEquipmentCardController(ICustomerEquipmentCardRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? customer = null)
    {
        var q = _repository.GetAll();
        if (!string.IsNullOrWhiteSpace(customer))
            q = q.Where(x => x.CustomerCode == customer);
        var list = q.OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<CustomerEquipmentCardResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<CustomerEquipmentCardResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CustomerEquipmentCardRequest dto)
    {
        var (error, item, bp) = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new CustomerEquipmentCard
        {
            CardCode       = NextCode(),
            ItemCode       = item!.ItemCode,
            ItemName       = item.ItemName,
            ManufacturerSN = dto.ManufacturerSN,
            InternalSN     = dto.InternalSN,
            CustomerCode   = bp!.Code,
            CustomerName   = bp.CardName,
            Status         = NormalizeStatus(dto.Status),
            DeliveryDate   = dto.DeliveryDate,
            WarrantyStart  = dto.WarrantyStart,
            WarrantyEnd    = dto.WarrantyEnd,
            Location       = dto.Location,
            Remarks        = dto.Remarks,
            CreatedAt      = DateTime.UtcNow,
            InActive       = false,
        };
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<CustomerEquipmentCardResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CustomerEquipmentCardRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var (error, item, bp) = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.ItemCode       = item!.ItemCode;
        entity.ItemName       = item.ItemName;
        entity.ManufacturerSN = dto.ManufacturerSN;
        entity.InternalSN     = dto.InternalSN;
        entity.CustomerCode   = bp!.Code;
        entity.CustomerName   = bp.CardName;
        entity.Status         = NormalizeStatus(dto.Status);
        entity.DeliveryDate   = dto.DeliveryDate;
        entity.WarrantyStart  = dto.WarrantyStart;
        entity.WarrantyEnd    = dto.WarrantyEnd;
        entity.Location       = dto.Location;
        entity.Remarks        = dto.Remarks;
        entity.UpdatedAt      = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<CustomerEquipmentCardResponse>(entity));
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

    private (string? error, ItemEntity? item, BPEntity? bp) Validate(CustomerEquipmentCardRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ItemCode))     return ("Item is required.", null, null);
        if (string.IsNullOrWhiteSpace(dto.CustomerCode)) return ("Customer is required.", null, null);
        var item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == dto.ItemCode);
        if (item == null) return ($"Item '{dto.ItemCode}' does not exist.", null, null);
        var bp = _db.Set<BPEntity>().FirstOrDefault(c => c.Code == dto.CustomerCode);
        if (bp == null) return ($"Customer '{dto.CustomerCode}' does not exist.", null, null);
        if (dto.WarrantyStart.HasValue && dto.WarrantyEnd.HasValue && dto.WarrantyEnd < dto.WarrantyStart)
            return ("Warranty end can't be before warranty start.", null, null);
        return (null, item, bp);
    }

    private static string NormalizeStatus(string? s) =>
        s is "Active" or "Inactive" or "Terminated" ? s : "Active";

    private string NextCode()
    {
        var highest = _repository.GetAll().Select(x => x.CardCode).ToList()
            .Select(c => int.TryParse(c.Replace("EQ-", ""), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"EQ-{highest + 1:D4}";
    }
}

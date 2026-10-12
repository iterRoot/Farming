using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;

namespace FarmingApi.Modules.CRM.ServiceContract;

// ═══════════════════════════════════════════════════════════════════
// SERVICE CONTRACT  (route: /ServiceContract)
// GET    /ServiceContract
// GET    /ServiceContract/{id}
// GET    /ServiceContract/ForCustomer/{customerCode} → coverage picker for Service Call
// POST   /ServiceContract
// PUT    /ServiceContract/{id}
// POST   /ServiceContract/{id}/Cancel
// DELETE /ServiceContract/{id}
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ServiceContractController : ControllerBase
{
    private readonly IServiceContractRepository _repository;
    private readonly IMapper                    _mapper;
    private readonly MyDbContext                _db;

    public ServiceContractController(IServiceContractRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? customerCode = null)
    {
        var list = _repository.GetAll()
            .Where(x => customerCode == null || x.CustomerCode == customerCode)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<ServiceContractResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ServiceContractResponse>(e));
    }

    // Only contracts a new Service Call could actually be covered by:
    // this customer, not Cancelled, not past its end date.
    [HttpGet("ForCustomer/{customerCode}")]
    public IActionResult ForCustomer(string customerCode)
    {
        var today = DateTime.UtcNow.Date;
        var list = _repository.GetAll()
            .Where(x => x.CustomerCode == customerCode && x.Status == "Active" && x.EndDate.Date >= today)
            .ToList();
        return Ok(_mapper.Map<List<ServiceContractResponse>>(list));
    }

    [HttpPost]
    public IActionResult Create([FromBody] ServiceContractRequest dto)
    {
        var (error, customer, item) = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new ServiceContract
        {
            ContractNo        = NextNumber(),
            CustomerCode      = customer!.Code,
            CustomerName      = customer.CardName,
            ItemCode          = item?.ItemCode,
            ItemName          = item?.ItemName,
            SerialNumber      = dto.SerialNumber,
            ContractType      = dto.ContractType,
            Status            = "Active",
            StartDate         = dto.StartDate == default ? DateTime.UtcNow.Date : dto.StartDate,
            EndDate           = dto.EndDate,
            ResponseTimeHours = dto.ResponseTimeHours,
            Value             = dto.Value,
            CoverageTerms     = dto.CoverageTerms,
            Remarks           = dto.Remarks,
            CreatedAt         = DateTime.UtcNow,
            InActive          = false,
        };

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ServiceContractResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ServiceContractRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Cancelled") return BadRequest("A Cancelled contract cannot be edited.");

        var (error, customer, item) = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.CustomerCode      = customer!.Code;
        entity.CustomerName      = customer.CardName;
        entity.ItemCode          = item?.ItemCode;
        entity.ItemName          = item?.ItemName;
        entity.SerialNumber      = dto.SerialNumber;
        entity.ContractType      = dto.ContractType;
        entity.StartDate         = dto.StartDate == default ? entity.StartDate : dto.StartDate;
        entity.EndDate           = dto.EndDate;
        entity.ResponseTimeHours = dto.ResponseTimeHours;
        entity.Value             = dto.Value;
        entity.CoverageTerms     = dto.CoverageTerms;
        entity.Remarks           = dto.Remarks;
        entity.UpdatedAt         = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ServiceContractResponse>(entity));
    }

    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Cancelled") return BadRequest("Already cancelled.");
        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"{entity.ContractNo} cancelled." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var inUse = _db.Set<FarmingApi.Modules.CRM.ServiceCall.ServiceCall>()
            .Any(s => s.ServiceContractId == id);
        if (inUse) return BadRequest("This contract has Service Calls referencing it and cannot be deleted — cancel it instead.");

        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private (string? error, BPEntity? customer, ItemEntity? item) Validate(ServiceContractRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerCode)) return ("Customer is required.", null, null);
        if (!ServiceContractOptions.Types.Contains(dto.ContractType))
            return ($"Contract type must be one of: {string.Join(", ", ServiceContractOptions.Types)}.", null, null);
        if (dto.EndDate <= dto.StartDate) return ("End date must be after the start date.", null, null);
        if (dto.ResponseTimeHours <= 0) return ("Response time must be greater than zero hours.", null, null);
        if (dto.Value < 0) return ("Value cannot be negative.", null, null);

        var customer = _db.Set<BPEntity>().FirstOrDefault(b => b.Code == dto.CustomerCode);
        if (customer == null) return ($"Customer '{dto.CustomerCode}' does not exist.", null, null);

        ItemEntity? item = null;
        if (!string.IsNullOrWhiteSpace(dto.ItemCode))
        {
            item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == dto.ItemCode);
            if (item == null) return ($"Item '{dto.ItemCode}' does not exist.", null, null);
        }

        return (null, customer, item);
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SC-{year}-";
        var highest = _repository.GetAll().Where(c => c.ContractNo.StartsWith(prefix))
            .Select(c => c.ContractNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }
}

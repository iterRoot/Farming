using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;

namespace FarmingApi.Modules.CRM.ServiceCall;

// ═══════════════════════════════════════════════════════════════════
// SERVICE CALL  (route: /ServiceCall)
// GET    /ServiceCall
// GET    /ServiceCall/{id}
// POST   /ServiceCall            → Open
// PUT    /ServiceCall/{id}
// PUT    /ServiceCall/{id}/Status → Open/InProgress/OnHold/Closed
//                                    (Closed requires a Resolution)
// DELETE /ServiceCall/{id}       → Open only
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ServiceCallController : ControllerBase
{
    private readonly IServiceCallRepository _repository;
    private readonly IMapper                _mapper;
    private readonly MyDbContext            _db;

    public ServiceCallController(IServiceCallRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? status = null, [FromQuery] string? customerCode = null)
    {
        var list = _repository.GetAll()
            .Where(x => status == null || x.Status == status)
            .Where(x => customerCode == null || x.CustomerCode == customerCode)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<ServiceCallResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ServiceCallResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] ServiceCallRequest dto)
    {
        var (error, customer, item) = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new ServiceCall
        {
            ServiceCallNo      = NextNumber(),
            CustomerCode       = customer!.Code,
            CustomerName       = customer.CardName,
            ContactPerson      = dto.ContactPerson,
            ItemCode           = item?.ItemCode,
            ItemName           = item?.ItemName,
            SerialNumber       = dto.SerialNumber,
            Subject            = dto.Subject.Trim(),
            ProblemDescription = dto.ProblemDescription,
            Status             = "Open",
            Priority           = dto.Priority,
            AssignedTo         = dto.AssignedTo,
            OpenDate           = DateTime.UtcNow,
            DueDate            = dto.DueDate,
            ServiceContractId  = dto.ServiceContractId,
            Remarks            = dto.Remarks,
            CreatedAt          = DateTime.UtcNow,
            InActive           = false,
        };

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ServiceCallResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ServiceCallRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Closed") return BadRequest("A Closed service call cannot be edited — reopen it first.");

        var (error, customer, item) = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.CustomerCode       = customer!.Code;
        entity.CustomerName       = customer.CardName;
        entity.ContactPerson      = dto.ContactPerson;
        entity.ItemCode           = item?.ItemCode;
        entity.ItemName           = item?.ItemName;
        entity.SerialNumber       = dto.SerialNumber;
        entity.Subject            = dto.Subject.Trim();
        entity.ProblemDescription = dto.ProblemDescription;
        entity.Priority           = dto.Priority;
        entity.AssignedTo         = dto.AssignedTo;
        entity.DueDate            = dto.DueDate;
        entity.ServiceContractId  = dto.ServiceContractId;
        entity.Remarks            = dto.Remarks;
        entity.UpdatedAt          = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ServiceCallResponse>(entity));
    }

    [HttpPut("{id:int}/Status")]
    public IActionResult SetStatus(int id, [FromBody] ServiceCallStatusRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (!ServiceCallOptions.Statuses.Contains(dto.Status))
            return BadRequest($"Status must be one of: {string.Join(", ", ServiceCallOptions.Statuses)}.");
        if (entity.Status == "Closed" && dto.Status != "Open")
            return BadRequest("A Closed service call can only be reopened (set back to Open), not moved to another status directly.");
        if (dto.Status == "Closed" && string.IsNullOrWhiteSpace(dto.Resolution) && string.IsNullOrWhiteSpace(entity.Resolution))
            return BadRequest("A resolution is required to close a service call.");

        entity.Status = dto.Status;
        if (!string.IsNullOrWhiteSpace(dto.Resolution)) entity.Resolution = dto.Resolution;
        entity.ClosedDate = dto.Status == "Closed" ? DateTime.UtcNow : null;
        entity.UpdatedAt  = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ServiceCallResponse>(entity));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Open") return BadRequest("Only an Open service call can be deleted.");
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private (string? error, BPEntity? customer, ItemEntity? item) Validate(ServiceCallRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CustomerCode)) return ("Customer is required.", null, null);
        if (string.IsNullOrWhiteSpace(dto.Subject)) return ("Subject is required.", null, null);
        if (!ServiceCallOptions.Priorities.Contains(dto.Priority))
            return ($"Priority must be one of: {string.Join(", ", ServiceCallOptions.Priorities)}.", null, null);

        var customer = _db.Set<BPEntity>().FirstOrDefault(b => b.Code == dto.CustomerCode);
        if (customer == null) return ($"Customer '{dto.CustomerCode}' does not exist.", null, null);

        ItemEntity? item = null;
        if (!string.IsNullOrWhiteSpace(dto.ItemCode))
        {
            item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == dto.ItemCode);
            if (item == null) return ($"Item '{dto.ItemCode}' does not exist.", null, null);
        }

        if (dto.ServiceContractId is int contractId)
        {
            var contract = _db.Set<FarmingApi.Modules.CRM.ServiceContract.ServiceContract>()
                .FirstOrDefault(c => c.Id == contractId);
            if (contract == null) return ($"Service contract {contractId} does not exist.", null, null);
            if (contract.CustomerCode != dto.CustomerCode)
                return ($"Contract {contract.ContractNo} belongs to {contract.CustomerCode}, not {dto.CustomerCode}.", null, null);
            if (contract.Status == "Cancelled")
                return ($"Contract {contract.ContractNo} is Cancelled and provides no coverage.", null, null);
            if (contract.EndDate.Date < DateTime.UtcNow.Date)
                return ($"Contract {contract.ContractNo} expired on {contract.EndDate:yyyy-MM-dd}.", null, null);
        }

        return (null, customer, item);
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"SVC-{year}-";
        var highest = _repository.GetAll().Where(s => s.ServiceCallNo.StartsWith(prefix))
            .Select(s => s.ServiceCallNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }
}

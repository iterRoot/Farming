using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Licence;

// ═══════════════════════════════════════════════════════════════════
// LICENSE PACKAGES  (route: /LicensePackage)
// GET    /LicensePackage
// GET    /LicensePackage/{id}
// POST   /LicensePackage
// PUT    /LicensePackage/{id}
// DELETE /LicensePackage/{id}
// ═══════════════════════════════════════════════════════════════════
public class LicensePackageController : MyController
{
    private static readonly string[] Cycles = { "Monthly", "Yearly", "One-Time" };

    private readonly IMapper                     _mapper;
    private readonly ILicensePackageRepository   _repository;

    public LicensePackageController(ILicensePackageRepository repository, IMapper mapper)
    {
        _mapper     = mapper;
        _repository = repository;
    }

    private string? Validate(LicensePackageRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) return "Package name is required";
        if (r.Price < 0)                       return "Price cannot be negative";
        if (r.MaxUsers < 1)                    return "Max users must be at least 1";
        if (r.TrialDays < 0)                   return "Trial days cannot be negative";
        if (!string.IsNullOrWhiteSpace(r.BillingCycle) && !Cycles.Contains(r.BillingCycle))
            return $"Billing cycle must be one of: {string.Join(", ", Cycles)}";
        return null;
    }

    [HttpGet]
    public IActionResult Gets()
    {
        var query  = _repository.GetAll().OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Price);
        var result = _mapper.ProjectTo<LicensePackageResponse>(query).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var entity = _repository.GetSingle(p => p.Id == id);
        if (entity == null) return NotFound($"License package {id} not found");
        return Ok(_mapper.Map<LicensePackageResponse>(entity));
    }

    [HttpPost]
    public IActionResult Create([FromBody] LicensePackageRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var error = Validate(request);
        if (error != null) return BadRequest(error);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            if (_repository.GetSingle(p => p.Code == code) != null)
                return BadRequest($"Package code '{code}' already exists");
        }

        var entity = _mapper.Map<LicensePackage>(request);
        entity.Code      = request.Code?.Trim();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "License package created", id = entity.Id });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] LicensePackageRequest request)
    {
        var entity = _repository.GetSingle(p => p.Id == id);
        if (entity == null) return NotFound($"License package {id} not found");
        var error = Validate(request);
        if (error != null) return BadRequest(error);

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim();
            if (_repository.GetAll().Any(p => p.Code == code && p.Id != id))
                return BadRequest($"Package code '{code}' already exists");
        }

        _mapper.Map(request, entity);
        entity.Code      = request.Code?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(p => p.Id == id);
        if (entity == null) return NotFound($"License package {id} not found");

        entity.DeletedAt = DateTime.UtcNow;
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }
}

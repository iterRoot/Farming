using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Sales.SaleEmployeeBuyer;

[ApiController]
[Route("[controller]")]
public class SaleEmployeeBuyerController : ControllerBase
{
    private readonly ISaleEmployeeBuyerRepository _repo;
    private readonly IMapper                      _mapper;

    public SaleEmployeeBuyerController(
        ISaleEmployeeBuyerRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /SaleEmployeeBuyer ─────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll([FromQuery] string? type = null)
    {
        var query = _repo.GetAll()
            .Where(x => type == null
                     || x.EmployeeType == type
                     || x.EmployeeType == "Both")
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToList();
        return Ok(_mapper.Map<List<SaleEmployeeBuyerResponse>>(query));
    }

    // ── GET /SaleEmployeeBuyer/{id} ────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(e));
    }

    // ── GET /SaleEmployeeBuyer/ByCode/{code} ──────────────────
    [AllowAnonymous][HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var e = _repo.GetSingle(x => x.Code == code.Trim().ToUpper());
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(e));
    }

    // ── GET /SaleEmployeeBuyer/SalesEmployees ─────────────────
    [AllowAnonymous][HttpGet("SalesEmployees")]
    public IActionResult GetSalesEmployees()
    {
        var list = _repo.GetAll()
            .Where(x => x.IsActive && (x.EmployeeType == "SalesEmployee" || x.EmployeeType == "Both"))
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .ToList();
        return Ok(_mapper.Map<List<SaleEmployeeBuyerResponse>>(list));
    }

    // ── GET /SaleEmployeeBuyer/Buyers ─────────────────────────
    [AllowAnonymous][HttpGet("Buyers")]
    public IActionResult GetBuyers()
    {
        var list = _repo.GetAll()
            .Where(x => x.IsActive && (x.EmployeeType == "Buyer" || x.EmployeeType == "Both"))
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .ToList();
        return Ok(_mapper.Map<List<SaleEmployeeBuyerResponse>>(list));
    }

    // ── POST /SaleEmployeeBuyer ────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] SaleEmployeeBuyerRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Employee code '{code}' already exists");

        var entity       = _mapper.Map<SaleEmployeeBuyer>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(entity));
    }

    // ── PUT /SaleEmployeeBuyer/{id} ────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] SaleEmployeeBuyerRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Employee code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(entity));
    }

    // ── DELETE /SaleEmployeeBuyer/{id} ────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }
}
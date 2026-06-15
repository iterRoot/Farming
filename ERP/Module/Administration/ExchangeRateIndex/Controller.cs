using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Financials.ExchangeRate;

// ═══════════════════════════════════════════════════════════════
// EXCHANGE RATE CONTROLLER
// GET /ExchangeRate
// GET /ExchangeRate/{id}
// GET /ExchangeRate/Latest          ← latest rate per currency
// POST /ExchangeRate
// PUT  /ExchangeRate/{id}
// DELETE /ExchangeRate/{id}
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ExchangeRateController : ControllerBase
{
    private readonly IExchangeRateRepository _repo;
    private readonly IMapper                 _mapper;

    public ExchangeRateController(IExchangeRateRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .OrderBy(x => x.Currency)
            .ThenByDescending(x => x.EffectiveDate)
            .ToList();
        return Ok(_mapper.Map<List<ExchangeRateResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ExchangeRateResponse>(e));
    }

    // Returns the most recent rate for every currency
    [AllowAnonymous][HttpGet("Latest")]
    public IActionResult GetLatest()
    {
        var latest = _repo.GetAll()
            .Where(x => x.IsActive)
            .GroupBy(x => x.Currency)
            .Select(g => g.OrderByDescending(x => x.EffectiveDate).First())
            .ToList();
        return Ok(_mapper.Map<List<ExchangeRateResponse>>(latest));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ExchangeRateRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity       = _mapper.Map<ExchangeRate>(dto);
        entity.Currency  = dto.Currency.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ExchangeRateResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ExchangeRateRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        _mapper.Map(dto, entity);
        entity.Currency  = dto.Currency.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ExchangeRateResponse>(entity));
    }

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


// ═══════════════════════════════════════════════════════════════
// PRICE INDEX CONTROLLER
// GET /PriceIndex
// GET /PriceIndex/{id}
// GET /PriceIndex/Latest
// POST /PriceIndex
// PUT  /PriceIndex/{id}
// DELETE /PriceIndex/{id}
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class PriceIndexController : ControllerBase
{
    private readonly IPriceIndexRepository _repo;
    private readonly IMapper               _mapper;

    public PriceIndexController(IPriceIndexRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .OrderBy(x => x.IndexCode)
            .ThenByDescending(x => x.EffectiveDate)
            .ToList();
        return Ok(_mapper.Map<List<PriceIndexResponse>>(list));
    }

    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<PriceIndexResponse>(e));
    }

    // Latest index value per IndexCode
    [AllowAnonymous][HttpGet("Latest")]
    public IActionResult GetLatest()
    {
        var latest = _repo.GetAll()
            .Where(x => x.IsActive)
            .GroupBy(x => x.IndexCode)
            .Select(g => g.OrderByDescending(x => x.EffectiveDate).First())
            .ToList();
        return Ok(_mapper.Map<List<PriceIndexResponse>>(latest));
    }

    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] PriceIndexRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity       = _mapper.Map<PriceIndex>(dto);
        entity.IndexCode = dto.IndexCode.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<PriceIndexResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PriceIndexRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        _mapper.Map(dto, entity);
        entity.IndexCode = dto.IndexCode.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<PriceIndexResponse>(entity));
    }

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
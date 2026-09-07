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

    // UPSERT — one rate per currency per day. Posting a date that already
    // has a rate for that currency updates it instead of duplicating.
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ExchangeRateRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Currency.Trim().ToUpper();
        var day  = NormaliseDay(dto.EffectiveDate);

        var existing = _repo.GetSingle(x => x.Currency == code && x.EffectiveDate == day);
        if (existing != null)
        {
            _mapper.Map(dto, existing);
            existing.Currency      = code;
            existing.EffectiveDate = day;
            existing.UpdatedAt     = DateTime.UtcNow;
            _repo.Update(existing);
            _repo.Commit();
            return Ok(_mapper.Map<ExchangeRateResponse>(existing));
        }

        var entity           = _mapper.Map<ExchangeRate>(dto);
        entity.Currency      = code;
        entity.EffectiveDate = day;
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ExchangeRateResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ExchangeRateRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Currency.Trim().ToUpper();
        var day  = NormaliseDay(dto.EffectiveDate);

        // Don't let an edit collide with another row for the same currency/day
        var clash = _repo.GetSingle(x => x.Currency == code && x.EffectiveDate == day && x.Id != id);
        if (clash != null)
            return BadRequest($"A {code} rate already exists for {day:yyyy-MM-dd}.");

        _mapper.Map(dto, entity);
        entity.Currency      = code;
        entity.EffectiveDate = day;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ExchangeRateResponse>(entity));
    }

    // Strip the time and pin to UTC so a day matches exactly, whatever
    // timezone the client posted from.
    private static DateTime NormaliseDay(DateTime d) =>
        DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

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

    // UPSERT — one value per index code per day.
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] PriceIndexRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.IndexCode.Trim().ToUpper();
        var day  = NormaliseDay(dto.EffectiveDate);

        var existing = _repo.GetSingle(x => x.IndexCode == code && x.EffectiveDate == day);
        if (existing != null)
        {
            _mapper.Map(dto, existing);
            existing.IndexCode     = code;
            existing.EffectiveDate = day;
            existing.UpdatedAt     = DateTime.UtcNow;
            _repo.Update(existing);
            _repo.Commit();
            return Ok(_mapper.Map<PriceIndexResponse>(existing));
        }

        var entity           = _mapper.Map<PriceIndex>(dto);
        entity.IndexCode     = code;
        entity.EffectiveDate = day;
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<PriceIndexResponse>(entity));
    }

    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PriceIndexRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.IndexCode.Trim().ToUpper();
        var day  = NormaliseDay(dto.EffectiveDate);

        var clash = _repo.GetSingle(x => x.IndexCode == code && x.EffectiveDate == day && x.Id != id);
        if (clash != null)
            return BadRequest($"A {code} value already exists for {day:yyyy-MM-dd}.");

        _mapper.Map(dto, entity);
        entity.IndexCode     = code;
        entity.EffectiveDate = day;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<PriceIndexResponse>(entity));
    }

    private static DateTime NormaliseDay(DateTime d) =>
        DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

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
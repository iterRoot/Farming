using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ExchangeRate;

// ═══════════════════════════════════════════════════════════════
// EXCHANGE RATE CONTROLLER
// GET /ExchangeRate
// GET /ExchangeRate/{id}
// GET /ExchangeRate/Latest          ← latest rate per currency
// GET /ExchangeRate/On?currency=&date=   ← rate in force on a date (latest on or before it)
// GET /ExchangeRate/Settings        ← daily auto-update settings + last run
// PUT /ExchangeRate/Settings
// POST /ExchangeRate/Fetch          ← fetch today's rates now
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
    private readonly MyDbContext             _db;
    private readonly ExchangeRateFetcher     _fetcher;

    public ExchangeRateController(IExchangeRateRepository repo, IMapper mapper, MyDbContext db, ExchangeRateFetcher fetcher)
    {
        _repo    = repo;
        _mapper  = mapper;
        _db      = db;
        _fetcher = fetcher;
    }

    // ── Auto-update settings ─────────────────────────────────
    private static ExchangeRateSettingsDto ToDto(ExchangeRateSettings? s) => new()
    {
        AutoFetchEnabled = s?.AutoFetchEnabled ?? false,
        BaseCurrency     = s?.BaseCurrency ?? "KHR",
        Currencies       = s?.Currencies ?? "USD",
        RunAtHourUtc     = s?.RunAtHourUtc ?? 1,
        LastAttemptAt    = s?.LastAttemptAt,
        LastSuccessAt    = s?.LastSuccessAt,
        LastStatus       = s?.LastStatus,
        LastMessage      = s?.LastMessage,
    };

    [HttpGet("Settings")]
    public IActionResult GetSettings() =>
        Ok(ToDto(_db.Set<ExchangeRateSettings>().AsNoTracking().OrderBy(x => x.Id).FirstOrDefault()));

    [HttpPut("Settings")]
    public IActionResult PutSettings([FromBody] ExchangeRateSettingsRequest dto)
    {
        var s = _db.Set<ExchangeRateSettings>().OrderBy(x => x.Id).FirstOrDefault();
        if (s == null)
        {
            s = new ExchangeRateSettings { CreatedAt = DateTime.UtcNow, InActive = false };
            _db.Set<ExchangeRateSettings>().Add(s);
        }
        var candidate = new ExchangeRateSettings
        {
            AutoFetchEnabled = dto.AutoFetchEnabled,
            BaseCurrency     = (dto.BaseCurrency ?? "").Trim().ToUpperInvariant(),
            Currencies       = string.Join(",", ExchangeRateFetcher.ParseCurrencies(dto.Currencies, (dto.BaseCurrency ?? "").Trim())),
            RunAtHourUtc     = dto.RunAtHourUtc,
        };
        var err = ExchangeRateFetcher.ValidateSettings(candidate);
        if (err != null) return BadRequest(err);

        s.AutoFetchEnabled = candidate.AutoFetchEnabled;
        s.BaseCurrency     = candidate.BaseCurrency;
        s.Currencies       = candidate.Currencies;
        s.RunAtHourUtc     = candidate.RunAtHourUtc;
        s.UpdatedAt        = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(ToDto(s));
    }

    // Fetch today's rates now (works whether or not the daily job is on).
    [HttpPost("Fetch")]
    public async Task<IActionResult> Fetch(CancellationToken ct)
    {
        var s = await _db.Set<ExchangeRateSettings>().AsNoTracking().OrderBy(x => x.Id).FirstOrDefaultAsync(ct);
        var err = ExchangeRateFetcher.ValidateSettings(s ?? new ExchangeRateSettings { AutoFetchEnabled = true });
        if (err != null) return BadRequest(err);
        return Ok(await _fetcher.FetchAsync(_db, ct));
    }

    // The rate in force on a date: the latest one on or before it.
    [HttpGet("On")]
    public IActionResult On([FromQuery] string currency, [FromQuery] DateTime? date = null)
    {
        if (string.IsNullOrWhiteSpace(currency)) return BadRequest("currency is required.");
        var code = currency.Trim().ToUpperInvariant();
        var day  = NormaliseDay(date ?? DateTime.UtcNow);
        var row  = _repo.GetAll().Where(x => x.Currency == code && x.IsActive && x.EffectiveDate <= day)
                        .OrderByDescending(x => x.EffectiveDate).FirstOrDefault();
        if (row == null) return NotFound($"No {code} rate on or before {day:yyyy-MM-dd}.");
        return Ok(_mapper.Map<ExchangeRateResponse>(row));
    }

    // Shared by typed-in create/update.
    private static string? CheckRate(ExchangeRateRequest dto)
    {
        var cur = (dto.Currency ?? "").Trim();
        var bas = (dto.BaseCurrency ?? "").Trim();
        if (cur.Length != 3 || !cur.All(char.IsLetter)) return "Currency must be a 3-letter code such as USD.";
        if (bas.Length != 3 || !bas.All(char.IsLetter)) return "Base currency must be a 3-letter code such as KHR.";
        if (string.Equals(cur, bas, StringComparison.OrdinalIgnoreCase)) return "A currency can't have a rate against itself.";
        if (dto.Rate <= 0) return "Rate must be greater than zero.";
        return null;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .OrderBy(x => x.Currency)
            .ThenByDescending(x => x.EffectiveDate)
            .ToList();
        return Ok(_mapper.Map<List<ExchangeRateResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ExchangeRateResponse>(e));
    }

    // Returns the most recent rate for every currency
    [HttpGet("Latest")]
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
    [HttpPost]
    public IActionResult Create([FromBody] ExchangeRateRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var rateErr = CheckRate(dto);
        if (rateErr != null) return BadRequest(rateErr);

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

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ExchangeRateRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        var rateErr = CheckRate(dto);
        if (rateErr != null) return BadRequest(rateErr);

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

    [HttpDelete("{id:int}")]
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

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .OrderBy(x => x.IndexCode)
            .ThenByDescending(x => x.EffectiveDate)
            .ToList();
        return Ok(_mapper.Map<List<PriceIndexResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<PriceIndexResponse>(e));
    }

    // Latest index value per IndexCode
    [HttpGet("Latest")]
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
    [HttpPost]
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

    [HttpPut("{id:int}")]
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

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }
}
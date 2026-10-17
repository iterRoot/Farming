using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;

namespace FarmingApi.Modules.InventoryManagement.Forecast;

// ═══════════════════════════════════════════════════════════════════
// FORECAST  (route: /Forecast)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ForecastController : ControllerBase
{
    private readonly IForecastRepository _repository;
    private readonly IMapper     _mapper;
    private readonly MyDbContext _db;

    public ForecastController(IForecastRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines).OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<ForecastResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ForecastResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] ForecastRequest dto)
    {
        var (error, items) = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new Forecast
        {
            Code       = NextCode(),
            Name       = dto.Name.Trim(),
            PeriodType = NormalizePeriod(dto.PeriodType),
            StartDate  = dto.StartDate,
            EndDate    = dto.EndDate,
            IsActive   = dto.IsActive,
            Remarks    = dto.Remarks,
            CreatedAt  = DateTime.UtcNow,
            InActive   = false,
        };
        AddLines(entity, dto, items!);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ForecastResponse>(ReloadWithLines(entity.Id)));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ForecastRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var (error, items) = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.Lines.Clear();
        entity.Name       = dto.Name.Trim();
        entity.PeriodType = NormalizePeriod(dto.PeriodType);
        entity.StartDate  = dto.StartDate;
        entity.EndDate    = dto.EndDate;
        entity.IsActive   = dto.IsActive;
        entity.Remarks    = dto.Remarks;
        entity.UpdatedAt  = DateTime.UtcNow;
        AddLines(entity, dto, items!);

        _repository.Commit();
        return Ok(_mapper.Map<ForecastResponse>(ReloadWithLines(entity.Id)));
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

    private (string? error, Dictionary<string, string>? items) Validate(ForecastRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) return ("Forecast name is required.", null);
        if (dto.EndDate < dto.StartDate)         return ("End date can't be before start date.", null);
        if (dto.Lines == null || dto.Lines.Count == 0) return ("Add at least one forecast line.", null);
        if (dto.Lines.Any(l => l.Quantity <= 0)) return ("Every line quantity must be greater than zero.", null);

        var codes = dto.Lines.Select(l => l.ItemCode).Distinct().ToList();
        var items = _db.Set<ItemEntity>().Where(i => codes.Contains(i.ItemCode))
            .ToDictionary(i => i.ItemCode, i => i.ItemName);
        var missing = codes.Where(c => string.IsNullOrWhiteSpace(c) || !items.ContainsKey(c)).ToList();
        if (missing.Count > 0) return ($"Item(s) not found: {string.Join(", ", missing)}.", null);
        return (null, items);
    }

    private static void AddLines(Forecast entity, ForecastRequest dto, Dictionary<string, string> items)
    {
        var lineNum = 1;
        foreach (var l in dto.Lines)
        {
            entity.Lines.Add(new ForecastLine
            {
                LineNum      = lineNum++,
                ItemCode     = l.ItemCode,
                ItemName     = items.GetValueOrDefault(l.ItemCode, l.ItemCode),
                ForecastDate = l.ForecastDate,
                Quantity     = l.Quantity,
            });
        }
    }

    private static string NormalizePeriod(string? p) =>
        p is "Daily" or "Weekly" or "Monthly" ? p : "Monthly";

    private string NextCode()
    {
        var highest = _repository.GetAll().Select(x => x.Code).ToList()
            .Select(c => int.TryParse(c.Replace("FC-", ""), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"FC-{highest + 1:D4}";
    }

    private Forecast ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Administration.DashboardParameters;

[ApiController]
[Route("[controller]")]
public class DashboardParameterController : ControllerBase
{
    private readonly IDashboardParameterRepository _repo;
    private readonly IMapper                       _mapper;

    public DashboardParameterController(
        IDashboardParameterRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /DashboardParameter ────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll([FromQuery] string? category = null)
    {
        var query = _repo.GetAll()
            .Where(x => category == null || x.Category == category)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<DashboardParameterResponse>>(query));
    }

    // ── GET /DashboardParameter/{id} ──────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<DashboardParameterResponse>(e));
    }

    // ── GET /DashboardParameter/ByCode/{code} ─────────────────
    [AllowAnonymous][HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var e = _repo.GetSingle(x => x.Code == code.Trim().ToUpper());
        if (e == null) return NotFound(new { message = $"Parameter '{code}' not found" });
        return Ok(_mapper.Map<DashboardParameterResponse>(e));
    }

    // ── GET /DashboardParameter/Categories ────────────────────
    [AllowAnonymous][HttpGet("Categories")]
    public IActionResult GetCategories()
    {
        var cats = _repo.GetAll()
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        return Ok(cats);
    }

    // ── POST /DashboardParameter ───────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] DashboardParameterRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Parameter '{code}' already exists");

        var entity       = _mapper.Map<DashboardParameter>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<DashboardParameterResponse>(entity));
    }

    // ── PUT /DashboardParameter/{id} ──────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] DashboardParameterRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Parameter code '{code}' already exists");

        // Preserve system flag
        var isSystem = entity.IsSystem;
        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.IsSystem  = isSystem;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<DashboardParameterResponse>(entity));
    }

    // ── PUT /DashboardParameter/BulkSave ──────────────────────
    // Update only the Value field for many params at once (settings page)
    [AllowAnonymous][HttpPut("BulkSave")]
    public IActionResult BulkSave([FromBody] List<ParameterValueRequest> items)
    {
        var updated = new List<string>();
        foreach (var item in items)
        {
            var code   = item.Code.Trim().ToUpper();
            var entity = _repo.GetSingle(x => x.Code == code);
            if (entity == null) continue;
            entity.Value     = item.Value;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entity);
            updated.Add(code);
        }
        _repo.Commit();
        return Ok(new { updated = updated.Count, codes = updated });
    }

    // ── POST /DashboardParameter/{id}/Reset ───────────────────
    // Restore a parameter to its DefaultValue
    [AllowAnonymous][HttpPost("{id:int}/Reset")]
    public IActionResult Reset(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var old         = entity.Value;
        entity.Value    = entity.DefaultValue;
        entity.UpdatedAt= DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new ResetResult
        {
            Code     = entity.Code,
            OldValue = old,
            NewValue = entity.DefaultValue,
            Message  = $"'{entity.Name}' reset to default value",
        });
    }

    // ── POST /DashboardParameter/ResetAll ─────────────────────
    [AllowAnonymous][HttpPost("ResetAll")]
    public IActionResult ResetAll()
    {
        var all = _repo.GetAll().ToList();
        foreach (var e in all)
        {
            e.Value     = e.DefaultValue;
            e.UpdatedAt = DateTime.UtcNow;
            _repo.Update(e);
        }
        _repo.Commit();
        return Ok(new { message = $"{all.Count} parameters reset to defaults" });
    }

    // ── POST /DashboardParameter/Seed ─────────────────────────
    // Seed factory default parameters if none exist
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Parameters already seeded. Use ResetAll to restore defaults.");

        var seeds = GetSeedData();
        foreach (var p in seeds)
        {
            p.CreatedAt = DateTime.UtcNow;
            p.InActive  = false;
            _repo.Add(p);
        }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} default parameters created" });
    }

    // ── DELETE /DashboardParameter/{id} ───────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsSystem)
            return BadRequest("System parameters cannot be deleted");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ─── Seed Data ────────────────────────────────────────────
    private static List<DashboardParameter> GetSeedData() => new()
    {
        // Performance
        new() { Code="REFRESH_INTERVAL",    Name="Auto Refresh Interval",       Category="Performance", DataType="Integer", Value="60",    DefaultValue="60",    MinValue="10",   MaxValue="3600", Unit="seconds",   IsSystem=true,  Description="How often the dashboard auto-refreshes (seconds)" },
        new() { Code="MAX_ROWS_PER_WIDGET", Name="Max Rows Per Widget",          Category="Performance", DataType="Integer", Value="100",   DefaultValue="100",   MinValue="10",   MaxValue="1000", Unit="rows",      IsSystem=true,  Description="Maximum rows fetched per dashboard widget" },
        new() { Code="CACHE_DURATION",      Name="Widget Cache Duration",        Category="Performance", DataType="Integer", Value="300",   DefaultValue="300",   MinValue="0",    MaxValue="3600", Unit="seconds",   IsSystem=false, Description="How long widget data is cached (0 = no cache)" },

        // Display
        new() { Code="ITEMS_PER_PAGE",      Name="Items Per Page",               Category="Display",     DataType="Select",  Value="25",    DefaultValue="25",    Options="[\"10\",\"25\",\"50\",\"100\"]",      IsSystem=false, Description="Default pagination size in list views" },
        new() { Code="DATE_DISPLAY_FORMAT", Name="Date Display Format",          Category="Display",     DataType="Select",  Value="DD/MM/YYYY", DefaultValue="DD/MM/YYYY", Options="[\"DD/MM/YYYY\",\"MM/DD/YYYY\",\"YYYY-MM-DD\"]", IsSystem=false },
        new() { Code="CURRENCY_DISPLAY",    Name="Default Currency Display",     Category="Display",     DataType="Select",  Value="KHR",   DefaultValue="KHR",   Options="[\"KHR\",\"USD\",\"EUR\",\"THB\"]",   IsSystem=false },
        new() { Code="SHOW_CURRENCY_SYMBOL",Name="Show Currency Symbol",         Category="Display",     DataType="Boolean", Value="true",  DefaultValue="true",                                                    IsSystem=false },
        new() { Code="DECIMAL_PLACES",      Name="Decimal Places",               Category="Display",     DataType="Select",  Value="2",     DefaultValue="2",     Options="[\"0\",\"1\",\"2\",\"3\",\"4\"]",    IsSystem=false },
        new() { Code="SHOW_GRID_LINES",     Name="Show Grid Lines",              Category="Display",     DataType="Boolean", Value="true",  DefaultValue="true",                                                    IsSystem=false },

        // Alerts
        new() { Code="LOW_STOCK_THRESHOLD", Name="Low Stock Alert Threshold",    Category="Alerts",      DataType="Decimal", Value="10",    DefaultValue="10",    MinValue="0",    MaxValue="100",  Unit="%",         IsSystem=false, Description="Alert when stock falls below this percentage" },
        new() { Code="OVERDUE_DAYS",        Name="Overdue Invoice Days",         Category="Alerts",      DataType="Integer", Value="30",    DefaultValue="30",    MinValue="1",    MaxValue="365",  Unit="days",      IsSystem=false, Description="Days past due date to flag as overdue" },
        new() { Code="ALERT_EMAIL_ENABLED", Name="Send Alert Emails",            Category="Alerts",      DataType="Boolean", Value="false", DefaultValue="false",                                                   IsSystem=false },
        new() { Code="ALERT_EMAIL_ADDR",    Name="Alert Email Address",          Category="Alerts",      DataType="String",  Value="",      DefaultValue="",                                                        IsSystem=false },
        new() { Code="HIGH_VALUE_THRESHOLD",Name="High Value Transaction Alert", Category="Alerts",      DataType="Decimal", Value="10000",DefaultValue="10000",  Unit="USD",                                        IsSystem=false },

        // Appearance
        new() { Code="PRIMARY_COLOR",       Name="Dashboard Primary Color",      Category="Appearance",  DataType="Color",   Value="#3b82f6",DefaultValue="#3b82f6",                                               IsSystem=false },
        new() { Code="ACCENT_COLOR",        Name="Dashboard Accent Color",       Category="Appearance",  DataType="Color",   Value="#22c55e",DefaultValue="#22c55e",                                               IsSystem=false },
        new() { Code="CHART_THEME",         Name="Chart Colour Theme",           Category="Appearance",  DataType="Select",  Value="Default",DefaultValue="Default",Options="[\"Default\",\"Pastel\",\"Bold\",\"Monochrome\"]", IsSystem=false },
        new() { Code="FONT_SIZE",           Name="Base Font Size",               Category="Appearance",  DataType="Select",  Value="14",    DefaultValue="14",    Options="[\"12\",\"13\",\"14\",\"15\",\"16\"]", Unit="px",       IsSystem=false },
        new() { Code="COMPACT_MODE",        Name="Compact Mode",                 Category="Appearance",  DataType="Boolean", Value="false", DefaultValue="false",                                                   IsSystem=false },

        // Data
        new() { Code="FISCAL_YEAR_START",   Name="Fiscal Year Start Month",      Category="Data",        DataType="Select",  Value="1",     DefaultValue="1",     Options="[\"1\",\"2\",\"3\",\"4\",\"7\",\"10\",\"12\"]", IsSystem=false },
        new() { Code="DEFAULT_DATE_RANGE",  Name="Default Chart Date Range",     Category="Data",        DataType="Select",  Value="30",    DefaultValue="30",    Options="[\"7\",\"14\",\"30\",\"90\",\"180\",\"365\"]", Unit="days", IsSystem=false },
        new() { Code="SHOW_INACTIVE",       Name="Show Inactive Records",        Category="Data",        DataType="Boolean", Value="false", DefaultValue="false",                                                   IsSystem=false },
        new() { Code="BASE_CURRENCY",       Name="Base/Local Currency",          Category="Data",        DataType="String",  Value="KHR",   DefaultValue="KHR",                                                    IsSystem=true },
        new() { Code="TIMEZONE",            Name="Timezone",                     Category="Data",        DataType="String",  Value="Asia/Phnom_Penh",DefaultValue="Asia/Phnom_Penh",                               IsSystem=true },
    };
}
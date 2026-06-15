using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net;

namespace FarmingApi.Modules.Administration.SAPLinks;

[ApiController]
[Route("[controller]")]
public class SAPLinkController : ControllerBase
{
    private readonly ISAPLinkRepository _repo;
    private readonly IMapper            _mapper;
    private readonly MyDbContext        _db;

    public SAPLinkController(ISAPLinkRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /SAPLink ───────────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? category      = null,
        [FromQuery] string? linkType      = null,
        [FromQuery] string? linkedModule  = null,
        [FromQuery] bool?   isPinned      = null,
        [FromQuery] bool?   isOnDashboard = null)
    {
        var list = _repo.GetAll()
            .Where(x => category      == null || x.Category      == category)
            .Where(x => linkType      == null || x.LinkType      == linkType)
            .Where(x => linkedModule  == null || x.LinkedModule  == linkedModule)
            .Where(x => isPinned      == null || x.IsPinned      == isPinned)
            .Where(x => isOnDashboard == null || x.IsOnDashboard == isOnDashboard)
            .OrderBy(x => x.IsPinned ? 0 : 1)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.Title)
            .ToList();
        return Ok(_mapper.Map<List<SAPLinkResponse>>(list));
    }

    // ── GET /SAPLink/{id} ──────────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Usages.OrderByDescending(u => u.ClickedAt).Take(50))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SAPLinkResponse>(e));
    }

    // ── GET /SAPLink/ByCode/{code} ─────────────────────────────
    [AllowAnonymous][HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var e = _repo.GetSingle(x => x.Code == code.Trim().ToUpper());
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SAPLinkResponse>(e));
    }

    // ── GET /SAPLink/ByModule/{module} ─────────────────────────
    [AllowAnonymous][HttpGet("ByModule/{module}")]
    public IActionResult GetByModule(string module)
    {
        var list = _repo.GetAll()
            .Where(x => x.IsActive && (x.LinkedModule == module || x.LinkedModule == null && x.IsPinned))
            .OrderBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.Title)
            .ToList();
        return Ok(_mapper.Map<List<SAPLinkResponse>>(list));
    }

    // ── GET /SAPLink/QuickLaunch ───────────────────────────────
    // Returns pinned + dashboard links for quick-access panel
    [AllowAnonymous][HttpGet("QuickLaunch")]
    public IActionResult GetQuickLaunch()
    {
        var list = _repo.GetAll()
            .Where(x => x.IsActive && (x.IsPinned || x.IsOnDashboard))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ToList();
        return Ok(_mapper.Map<List<SAPLinkResponse>>(list));
    }

    // ── GET /SAPLink/Summary ───────────────────────────────────
    [AllowAnonymous][HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        return Ok(new SAPLinkSummary
        {
            Total       = all.Count,
            Active      = all.Count(x => x.IsActive),
            Pinned      = all.Count(x => x.IsPinned),
            OnDashboard = all.Count(x => x.IsOnDashboard),
            Unhealthy   = all.Count(x => !x.IsHealthy && x.IsActive),
            TotalClicks = all.Sum(x => (long)x.ClickCount),
            ByCategory  = all.GroupBy(x => x.Category).ToDictionary(g => g.Key, g => g.Count()),
            ByType      = all.GroupBy(x => x.LinkType).ToDictionary(g => g.Key, g => g.Count()),
            TopLinks    = _mapper.Map<List<SAPLinkResponse>>(
                all.Where(x => x.IsActive).OrderByDescending(x => x.ClickCount).Take(5).ToList()),
        });
    }

    // ── POST /SAPLink ──────────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] SAPLinkRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Link code '{code}' already exists");

        var entity    = _mapper.Map<SAPLink>(dto);
        entity.Code   = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<SAPLinkResponse>(entity));
    }

    // ── PUT /SAPLink/{id} ──────────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] SAPLinkRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Link code '{code}' already exists");
        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<SAPLinkResponse>(entity));
    }

    // ── POST /SAPLink/{id}/Click ───────────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Click")]
    public IActionResult RecordClick(int id, [FromBody] RecordClickRequest? dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Usages)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        entity.ClickCount   += 1;
        entity.LastClickedAt = DateTime.UtcNow;

        // Keep last 1000 usage records
        if (entity.Usages.Count >= 1000)
        {
            var oldest = entity.Usages.OrderBy(u => u.ClickedAt).First();
            _db.Remove(oldest);
        }

        entity.Usages.Add(new SAPLinkUsage
        {
            ClickedAt = DateTime.UtcNow,
            UserCode  = dto?.UserCode,
            UserName  = dto?.UserName,
            IpAddress = dto?.IpAddress,
            UserAgent = dto?.UserAgent,
            Referrer  = dto?.Referrer,
        });

        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { url = entity.Url, clickCount = entity.ClickCount });
    }

    // ── POST /SAPLink/{id}/TogglePin ───────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/TogglePin")]
    public IActionResult TogglePin(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsPinned  = !entity.IsPinned;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { isPinned = entity.IsPinned });
    }

    // ── POST /SAPLink/{id}/ToggleDashboard ─────────────────────
    [AllowAnonymous][HttpPost("{id:int}/ToggleDashboard")]
    public IActionResult ToggleDashboard(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsOnDashboard = !entity.IsOnDashboard;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { isOnDashboard = entity.IsOnDashboard });
    }

    // ── POST /SAPLink/{id}/HealthCheck ─────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/HealthCheck")]
    public async Task<IActionResult> HealthCheck(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        // Only check HTTP URLs
        if (!entity.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return Ok(new LinkHealthResult { Id = id, Code = entity.Code, Title = entity.Title, Url = entity.Url, IsReachable = true, Error = "Non-HTTP URL — skipping check" });

        var sw = Stopwatch.StartNew();
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.Add("User-Agent", "FarmingERP/HealthCheck");
            var resp = await client.GetAsync(entity.Url);
            sw.Stop();

            entity.LastStatusCode = ((int)resp.StatusCode).ToString();
            entity.LastCheckedAt  = DateTime.UtcNow;
            entity.IsHealthy      = resp.IsSuccessStatusCode;
            entity.UpdatedAt      = DateTime.UtcNow;
            _repo.Update(entity); _repo.Commit();

            return Ok(new LinkHealthResult
            {
                Id = id, Code = entity.Code, Title = entity.Title, Url = entity.Url,
                IsReachable = resp.IsSuccessStatusCode,
                StatusCode  = (int)resp.StatusCode,
                ResponseMs  = (int)sw.ElapsedMilliseconds,
            });
        }
        catch (Exception ex)
        {
            sw.Stop();
            entity.LastStatusCode = "timeout";
            entity.LastCheckedAt  = DateTime.UtcNow;
            entity.IsHealthy      = false;
            entity.UpdatedAt      = DateTime.UtcNow;
            _repo.Update(entity); _repo.Commit();

            return Ok(new LinkHealthResult
            {
                Id = id, Code = entity.Code, Title = entity.Title, Url = entity.Url,
                IsReachable = false, ResponseMs = (int)sw.ElapsedMilliseconds,
                Error = ex.Message,
            });
        }
    }

    // ── DELETE /SAPLink/{id} ───────────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /SAPLink/Seed ─────────────────────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any()) return BadRequest("Links already seeded.");
        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} SAP Links seeded" });
    }

    // ─── Seed factory data ────────────────────────────────────
    private static List<SAPLink> GetSeedData() => new()
    {
        // ── Government ──────────────────────────────────────
        new(){ Code="LNK-GDT-MAIN",    Title="GDT Tax Portal",                  Category="Government", LinkType="URL",  Url="https://www.tax.gov.kh",             IconName="🏛️",  Description="General Department of Taxation — main portal",              Tags="tax,gdt,government",       IsPinned=true,  IsOnDashboard=true,  SortOrder=1,  RequiresLogin=true,  LoginHint="Use your GDT business account" },
        new(){ Code="LNK-GDT-EINV",    Title="GDT e-Invoice System",            Category="Government", LinkType="URL",  Url="https://einvoice.tax.gov.kh",        IconName="🧾",  Description="Submit and manage electronic invoices to GDT",               Tags="tax,einvoice,gdt",         IsPinned=true,  IsOnDashboard=true,  SortOrder=2,  RequiresLogin=true  },
        new(){ Code="LNK-GDT-API",     Title="GDT e-Invoice API",               Category="Government", LinkType="API",  Url="https://api.tax.gov.kh/einvoice/v2", IconName="🔌",  Description="REST API for programmatic e-invoice submission",             Tags="tax,api,einvoice",         IsPinned=false, IsOnDashboard=false, SortOrder=3,  HttpMethod="POST", AuthType="ApiKey", ApiKeyHeader="X-GDT-Key", ContentType="application/json" },
        new(){ Code="LNK-MOC",         Title="Ministry of Commerce",            Category="Government", LinkType="URL",  Url="https://www.moc.gov.kh",             IconName="🏛️",  Description="Business registration and trade licensing",                  Tags="commerce,registration",    IsPinned=false, IsOnDashboard=false, SortOrder=4  },
        new(){ Code="LNK-CUSTOMS",     Title="Cambodia Customs (GDCE)",         Category="Government", LinkType="URL",  Url="https://www.customs.gov.kh",         IconName="🛃",  Description="General Department of Customs and Excise",                   Tags="customs,import,export",    IsPinned=false, IsOnDashboard=false, SortOrder=5  },
        new(){ Code="LNK-MOLVT",       Title="Ministry of Labour (MOLVT)",      Category="Government", LinkType="URL",  Url="https://www.molvt.gov.kh",           IconName="👷",  Description="Labour law, work permits, payroll compliance",               Tags="labour,hr,compliance",     IsPinned=false, IsOnDashboard=false, SortOrder=6,  LinkedModule="HR" },
        new(){ Code="LNK-NBC",         Title="National Bank of Cambodia",        Category="Government", LinkType="URL",  Url="https://www.nbc.gov.kh",             IconName="🏦",  Description="Central bank — exchange rates, regulations",                 Tags="bank,exchange,nbc",        IsPinned=false, IsOnDashboard=true,  SortOrder=7  },

        // ── Banking ─────────────────────────────────────────
        new(){ Code="LNK-ABA",         Title="ABA Business Banking",            Category="Banking",    LinkType="URL",  Url="https://www.ababank.com/business",   IconName="💳",  Description="ABA Bank — business internet banking portal",               Tags="aba,bank,payment",         IsPinned=true,  IsOnDashboard=true,  SortOrder=10, RequiresLogin=true },
        new(){ Code="LNK-ACLEDA",      Title="Acleda Business Banking",         Category="Banking",    LinkType="URL",  Url="https://www.acledabank.com.kh",      IconName="💳",  Description="Acleda Bank — corporate account management",                Tags="acleda,bank",              IsPinned=false, IsOnDashboard=false, SortOrder=11, RequiresLogin=true },
        new(){ Code="LNK-ABA-API",     Title="ABA Payment API",                 Category="Banking",    LinkType="API",  Url="https://api.ababank.com/v1/payments", IconName="🔌", Description="ABA Bank payment initiation API",                           Tags="aba,payment,api",          IsPinned=false, IsOnDashboard=false, SortOrder=12, HttpMethod="POST", AuthType="Bearer", ContentType="application/json" },
        new(){ Code="LNK-WING",        Title="Wing Money Portal",               Category="Banking",    LinkType="URL",  Url="https://www.wingmoney.com",          IconName="✈️",  Description="Wing mobile money — local payments",                         Tags="wing,payment,mobile",      IsPinned=false, IsOnDashboard=false, SortOrder=13 },

        // ── FX / Finance ────────────────────────────────────
        new(){ Code="LNK-NBC-FX",      Title="NBC Exchange Rates",              Category="Finance",    LinkType="URL",  Url="https://www.nbc.gov.kh/english/economic_research/exchange_rate.php", IconName="💱", Description="Official daily KHR/USD/EUR exchange rates", Tags="fx,exchange,rate,khr",  IsPinned=true,  IsOnDashboard=true,  SortOrder=20 },
        new(){ Code="LNK-XE",          Title="XE Currency Converter",           Category="Finance",    LinkType="URL",  Url="https://www.xe.com/currencyconverter", IconName="💹", Description="Live FX rates and currency conversion",                      Tags="fx,currency,xe",           IsPinned=false, IsOnDashboard=false, SortOrder=21 },

        // ── Integration / API ────────────────────────────────
        new(){ Code="LNK-INT-ERP",     Title="ERP API Documentation",           Category="Integration",LinkType="URL",  Url="http://localhost:5181/swagger",      IconName="📖",  Description="Swagger UI — browse all ERP REST endpoints",                Tags="api,swagger,docs",         IsPinned=true,  IsOnDashboard=false, SortOrder=30, BadgeText="DEV" },
        new(){ Code="LNK-INT-WEBHOOK", Title="Webhook Management",              Category="Integration",LinkType="API",  Url="http://localhost:5181/webhook",      IconName="🔔",  Description="Incoming webhook endpoint for event-driven integrations",    Tags="webhook,integration",      IsPinned=false, IsOnDashboard=false, SortOrder=31, HttpMethod="POST", ContentType="application/json" },

        // ── Reference ────────────────────────────────────────
        new(){ Code="LNK-ISO-9001",    Title="ISO 9001 Standard",               Category="Reference",  LinkType="URL",  Url="https://www.iso.org/iso-9001-quality-management.html", IconName="📋", Description="ISO 9001:2015 Quality Management System reference",       Tags="iso,quality,standard",     IsPinned=false, IsOnDashboard=false, SortOrder=40 },
        new(){ Code="LNK-INCOTERMS",   Title="Incoterms 2020",                  Category="Reference",  LinkType="URL",  Url="https://iccwbo.org/business-solutions/incoterms-rules/incoterms-2020", IconName="🚢", Description="International trade terms reference",                     Tags="incoterms,trade,shipping", IsPinned=false, IsOnDashboard=false, SortOrder=41 },
        new(){ Code="LNK-HS-CODE",     Title="HS Code Search (WCO)",            Category="Reference",  LinkType="URL",  Url="https://www.wcoomd.org/en/topics/nomenclature/overview/what-is-the-harmonized-system.aspx", IconName="🔍", Description="Harmonised System commodity codes",                      Tags="hs,code,customs,tariff",   IsPinned=false, IsOnDashboard=false, SortOrder=42 },
        new(){ Code="LNK-IFRS",        Title="IFRS Standards",                  Category="Reference",  LinkType="URL",  Url="https://www.ifrs.org/issued-standards/list-of-standards", IconName="📊", Description="International Financial Reporting Standards",                Tags="ifrs,accounting,finance",  IsPinned=false, IsOnDashboard=false, SortOrder=43, LinkedModule="Financials" },

        // ── Internal ─────────────────────────────────────────
        new(){ Code="LNK-HR-PORTAL",   Title="HR Self-Service Portal",          Category="Internal",   LinkType="Navigation", Url="/HR/Employee/List", IconName="👥", Description="Employee self-service — leave, payslips, profile",            Tags="hr,employee,selfservice",  IsPinned=true,  IsOnDashboard=true,  SortOrder=50, LinkedModule="HR" },
        new(){ Code="LNK-REPORTS",     Title="Report Manager",                  Category="Internal",   LinkType="Navigation", Url="/Administration/ReportLayout/List", IconName="📊", Description="Browse and run Crystal Reports",                         Tags="reports,print",            IsPinned=false, IsOnDashboard=false, SortOrder=51 },
        new(){ Code="LNK-CERTS",       Title="Certificate Registry",            Category="Internal",   LinkType="Navigation", Url="/Administration/ElectronicCertificate/List", IconName="🏅", Description="View and manage company certificates",                  Tags="certs,compliance",         IsPinned=false, IsOnDashboard=false, SortOrder=52 },

        // ── Social / Communication ───────────────────────────
        new(){ Code="LNK-TG-SUPPORT",  Title="IT Support Telegram",             Category="Social",     LinkType="URL",  Url="https://t.me/farmingcorp_itsupport", IconName="✈️", Description="Telegram group for ERP support requests",                   Tags="telegram,support,it",      IsPinned=false, IsOnDashboard=false, SortOrder=60, RequiresLogin=false },
        new(){ Code="LNK-LINKEDIN",    Title="Company LinkedIn",                Category="Social",     LinkType="URL",  Url="https://www.linkedin.com/company/farmingcorp", IconName="💼", Description="Official company LinkedIn page",                          Tags="linkedin,social",          IsPinned=false, IsOnDashboard=false, SortOrder=61 },
    };
}
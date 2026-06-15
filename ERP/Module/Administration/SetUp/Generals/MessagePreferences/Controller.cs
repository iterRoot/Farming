using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Administration.MessagePreferences;

[ApiController]
[Route("[controller]")]
public class MessagePreferenceController : ControllerBase
{
    private readonly IMessagePreferenceRepository _repo;
    private readonly IMapper                      _mapper;

    public MessagePreferenceController(
        IMessagePreferenceRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /MessagePreference ─────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? userCode  = null,
        [FromQuery] string? category  = null,
        [FromQuery] string? channel   = null,
        [FromQuery] string? module    = null)
    {
        var list = _repo.GetAll()
            .Where(x => userCode == null || x.UserCode == userCode)
            .Where(x => category == null || x.Category == category)
            .Where(x => channel  == null || x.Channel  == channel)
            .Where(x => module   == null || x.Module   == module)
            .OrderBy(x => x.UserCode)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.EventName)
            .ToList();
        return Ok(_mapper.Map<List<MessagePreferenceResponse>>(list));
    }

    // ── GET /MessagePreference/{id} ────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<MessagePreferenceResponse>(e));
    }

    // ── GET /MessagePreference/ByUser/{userCode} ───────────────
    [AllowAnonymous][HttpGet("ByUser/{userCode}")]
    public IActionResult GetByUser(string userCode)
    {
        var list = _repo.GetAll()
            .Where(x => x.UserCode == userCode.Trim().ToUpper() || x.UserCode == "All")
            .OrderBy(x => x.Category).ThenBy(x => x.EventName).ThenBy(x => x.Channel)
            .ToList();
        return Ok(_mapper.Map<List<MessagePreferenceResponse>>(list));
    }

    // ── GET /MessagePreference/Users ───────────────────────────
    [AllowAnonymous][HttpGet("Users")]
    public IActionResult GetUsers()
    {
        var users = _repo.GetAll()
            .Select(x => new { x.UserCode, x.UserName })
            .Distinct()
            .OrderBy(x => x.UserCode)
            .ToList();
        return Ok(users);
    }

    // ── GET /MessagePreference/EventCatalog ────────────────────
    [AllowAnonymous][HttpGet("EventCatalog")]
    public IActionResult GetEventCatalog()
        => Ok(GetAllEvents());

    // ── POST /MessagePreference ────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] MessagePreferenceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.UserCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x =>
            x.UserCode == code && x.EventKey == dto.EventKey && x.Channel == dto.Channel))
            return BadRequest($"Preference for {code} / {dto.EventKey} / {dto.Channel} already exists");

        var entity       = _mapper.Map<MessagePreference>(dto);
        entity.UserCode  = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<MessagePreferenceResponse>(entity));
    }

    // ── PUT /MessagePreference/{id} ────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] MessagePreferenceRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.UserCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x =>
            x.UserCode == code && x.EventKey == dto.EventKey &&
            x.Channel  == dto.Channel && x.Id != id))
            return BadRequest($"Another preference for {code}/{dto.EventKey}/{dto.Channel} already exists");

        _mapper.Map(dto, entity);
        entity.UserCode  = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<MessagePreferenceResponse>(entity));
    }

    // ── PUT /MessagePreference/{id}/Toggle ─────────────────────
    [AllowAnonymous][HttpPut("{id:int}/Toggle")]
    public IActionResult Toggle(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsEnabled = !entity.IsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { id, isEnabled = entity.IsEnabled });
    }

    // ── PUT /MessagePreference/BulkToggle ──────────────────────
    [AllowAnonymous][HttpPut("BulkToggle")]
    public IActionResult BulkToggle([FromBody] BulkToggleRequest dto)
    {
        var entities = _repo.GetAll().Where(x => dto.Ids.Contains(x.Id)).ToList();
        foreach (var e in entities)
        {
            e.IsEnabled = dto.IsEnabled;
            e.UpdatedAt = DateTime.UtcNow;
            _repo.Update(e);
        }
        _repo.Commit();
        return Ok(new { updated = entities.Count });
    }

    // ── POST /MessagePreference/CopyToUser ─────────────────────
    [AllowAnonymous][HttpPost("CopyToUser/{fromUserCode}")]
    public IActionResult CopyToUser(string fromUserCode, [FromBody] CopyToUserRequest dto)
    {
        var source      = _repo.GetAll().Where(x => x.UserCode == fromUserCode.Trim().ToUpper()).ToList();
        if (!source.Any()) return NotFound($"No preferences found for user '{fromUserCode}'");

        var targetCode  = dto.TargetUserCode.Trim().ToUpper();
        int copied = 0, skipped = 0;

        foreach (var s in source)
        {
            var exists = _repo.GetAll().Any(x =>
                x.UserCode == targetCode && x.EventKey == s.EventKey && x.Channel == s.Channel);

            if (exists && !dto.OverwriteExisting) { skipped++; continue; }
            if (exists && dto.OverwriteExisting)
            {
                var old = _repo.GetSingle(x =>
                    x.UserCode == targetCode && x.EventKey == s.EventKey && x.Channel == s.Channel)!;
                _repo.Remove(old);
            }

            _repo.Add(new MessagePreference
            {
                UserCode        = targetCode,
                UserName        = dto.TargetUserName,
                EventKey        = s.EventKey,
                EventName       = s.EventName,
                Category        = s.Category,
                Module          = s.Module,
                Priority        = s.Priority,
                Channel         = s.Channel,
                IsEnabled       = s.IsEnabled,
                Frequency       = s.Frequency,
                DigestTime      = s.DigestTime,
                DigestDay       = s.DigestDay,
                SubjectTemplate = s.SubjectTemplate,
                BodyTemplate    = s.BodyTemplate,
                SortOrder       = s.SortOrder,
                IsActive        = true,
                CreatedAt       = DateTime.UtcNow,
                InActive        = false,
            });
            copied++;
        }
        _repo.Commit();
        return Ok(new { copied, skipped, message = $"Copied {copied} preferences to '{targetCode}'" });
    }

    // ── DELETE /MessagePreference/{id} ─────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── DELETE /MessagePreference/ByUser/{userCode} ────────────
    [AllowAnonymous][HttpDelete("ByUser/{userCode}")]
    public IActionResult DeleteByUser(string userCode)
    {
        var list = _repo.GetAll().Where(x => x.UserCode == userCode.Trim().ToUpper()).ToList();
        foreach (var e in list) _repo.Remove(e);
        _repo.Commit();
        return Ok(new { deleted = list.Count });
    }

    // ── POST /MessagePreference/Seed ───────────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Preferences already seeded. Use /ByUser to add per-user prefs.");

        var seeds = BuildDefaultPrefs("All", null);
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} default message preferences created for 'All' users" });
    }

    // ── POST /MessagePreference/SeedForUser/{userCode} ─────────
    [AllowAnonymous][HttpPost("SeedForUser/{userCode}")]
    public IActionResult SeedForUser(string userCode, [FromQuery] string? userName = null)
    {
        var code = userCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.UserCode == code))
            return BadRequest($"Preferences for '{code}' already exist.");

        var seeds = BuildDefaultPrefs(code, userName);
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} preferences created for '{code}'" });
    }

    // ─── Helpers ──────────────────────────────────────────────
    private static List<MessagePreference> BuildDefaultPrefs(
        string userCode, string? userName)
    {
        var result = new List<MessagePreference>();
        foreach (var ev in GetAllEvents())
        foreach (var ch in ev.DefaultChannels)
        {
            result.Add(new MessagePreference
            {
                UserCode    = userCode,
                UserName    = userName,
                EventKey    = ev.EventKey,
                EventName   = ev.EventName,
                Category    = ev.Category,
                Module      = ev.Module,
                Priority    = ev.Priority,
                Channel     = ch,
                IsEnabled   = ev.EnabledByDefault,
                Frequency   = ev.Priority == "Critical" ? "Immediate" : "Immediate",
                SortOrder   = ev.SortOrder,
                IsActive    = true,
            });
        }
        return result;
    }

    private static List<EventDefinition> GetAllEvents() => new()
    {
        // Document — Sales
        new("ARInvoice.Posted",      "AR Invoice Posted",          "Document", "ARInvoice",      "High",     true,  10, new[]{ "Email","InApp" }),
        new("ARInvoice.Overdue",     "AR Invoice Overdue",         "Document", "ARInvoice",      "High",     true,  11, new[]{ "Email","InApp" }),
        new("SalesOrder.Created",    "Sales Order Created",        "Document", "SalesOrder",     "Medium",   true,  12, new[]{ "InApp" }),
        new("SalesOrder.Approved",   "Sales Order Approved",       "Document", "SalesOrder",     "High",     true,  13, new[]{ "Email","InApp" }),
        new("Quotation.Expiring",    "Quotation Expiring Soon",    "Document", "Quotation",      "Medium",   true,  14, new[]{ "Email","InApp" }),
        // Document — Purchasing
        new("PurchaseOrder.Created", "Purchase Order Created",     "Document", "PurchaseOrder",  "Medium",   false, 20, new[]{ "InApp" }),
        new("PurchaseOrder.Approved","Purchase Order Approved",    "Document", "PurchaseOrder",  "High",     true,  21, new[]{ "Email","InApp" }),
        new("GoodsReceipt.Posted",   "Goods Receipt Posted",       "Document", "GoodsReceipt",   "Medium",   false, 22, new[]{ "InApp" }),
        // Alerts — Inventory
        new("Stock.LowLevel",        "Stock Below Minimum Level",  "Alert",    "Inventory",      "High",     true,  30, new[]{ "Email","InApp" }),
        new("Stock.OutOfStock",      "Item Out of Stock",          "Alert",    "Inventory",      "Critical", true,  31, new[]{ "Email","SMS","InApp" }),
        new("Stock.ExpiryWarning",   "Batch Expiry Warning",       "Alert",    "Inventory",      "High",     true,  32, new[]{ "Email","InApp" }),
        // Alerts — Finance
        new("Payment.Received",      "Payment Received",           "Alert",    "Financials",     "Medium",   true,  40, new[]{ "Email","InApp" }),
        new("Payment.Overdue",       "Payment Overdue",            "Alert",    "Financials",     "High",     true,  41, new[]{ "Email","InApp" }),
        new("ExchangeRate.Updated",  "Exchange Rate Updated",      "Alert",    "Financials",     "Low",      false, 42, new[]{ "InApp" }),
        // HR
        new("Leave.Requested",       "Leave Request Submitted",    "HR",       "HR",             "Medium",   true,  50, new[]{ "Email","InApp" }),
        new("Leave.Approved",        "Leave Request Approved",     "HR",       "HR",             "Medium",   true,  51, new[]{ "Email","InApp" }),
        new("Payroll.Generated",     "Payroll Generated",          "HR",       "HR",             "High",     true,  52, new[]{ "Email","InApp" }),
        new("Attendance.Missing",    "Missing Attendance Record",  "HR",       "HR",             "Medium",   true,  53, new[]{ "InApp" }),
        // Security
        new("Login.NewDevice",       "Login from New Device",      "Security", "Administration", "Critical", true,  60, new[]{ "Email","SMS" }),
        new("Login.Failed",          "Failed Login Attempts",      "Security", "Administration", "High",     true,  61, new[]{ "Email" }),
        new("Password.Changed",      "Password Changed",           "Security", "Administration", "High",     true,  62, new[]{ "Email" }),
        new("User.Locked",           "User Account Locked",        "Security", "Administration", "Critical", true,  63, new[]{ "Email","InApp" }),
        // System
        new("System.BackupCompleted","Backup Completed",           "System",   "Administration", "Low",      false, 70, new[]{ "InApp" }),
        new("System.BackupFailed",   "Backup Failed",              "System",   "Administration", "Critical", true,  71, new[]{ "Email","InApp" }),
        new("System.ApiError",       "API Error Occurred",         "System",   "Administration", "High",     false, 72, new[]{ "Email" }),
        new("Report.Generated",      "Report Generation Complete", "System",   "Reports",        "Low",      false, 73, new[]{ "InApp" }),
    };

    private record EventDefinition(
        string   EventKey,
        string   EventName,
        string   Category,
        string   Module,
        string   Priority,
        bool     EnabledByDefault,
        int      SortOrder,
        string[] DefaultChannels);
}
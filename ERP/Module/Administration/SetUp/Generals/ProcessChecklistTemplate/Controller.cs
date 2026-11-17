using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Modules.Administration.SetUp.ProcessChecklistTemplate;

// ═══════════════════════════════════════════════════════════════════
// PROCESS CHECKLISTS  (route: /ChecklistTemplate)
// GET    /ChecklistTemplate?category=          → templates
// GET    /ChecklistTemplate/Summary            → counters for the cards
// GET    /ChecklistTemplate/{id}               → one template with steps
// POST   /ChecklistTemplate                    → create
// PUT    /ChecklistTemplate/{id}               → update (replaces steps)
// DELETE /ChecklistTemplate/{id}               → refused while a run is in progress
// POST   /ChecklistTemplate/{id}/Duplicate     → copy as a new inactive template
// POST   /ChecklistTemplate/Seed               → default templates (only when none exist)
// POST   /ChecklistTemplate/{id}/Start         → new run with the template's steps copied in
// GET    /ChecklistTemplate/Runs?status=       → runs
// GET    /ChecklistTemplate/Runs/{id}          → one run with its steps
// PUT    /ChecklistTemplate/Runs/{id}/Step/{n} → mark a step Done / Skipped / Failed / Pending
// POST   /ChecklistTemplate/Runs/{id}/SignOff  → complete a run once every step is Done or Skipped
// Who completed a step or signed off is always the signed-in user,
// never a name sent by the client.
// ═══════════════════════════════════════════════════════════════════
public class ChecklistTemplateController : MyController
{
    private readonly MyDbContext _db;
    public ChecklistTemplateController(MyDbContext db) => _db = db;

    private static readonly string[] Categories   = { "Finance", "HR", "Inventory", "Sales", "Purchasing", "Compliance", "IT", "Quality", "General" };
    private static readonly string[] TriggerTypes = { "Manual", "OnDocumentPost", "OnMonthEnd", "OnYearEnd", "Scheduled", "OnEvent" };
    private static readonly string[] StepTypes    = { "Action", "Approval", "Review", "Verification", "Upload", "Notification", "Report" };
    private static readonly string[] StepStatuses = { "Pending", "Done", "Skipped", "Failed" };

    // ── Templates ───────────────────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? category)
    {
        var q = _db.Set<ChecklistTemplate>().AsNoTracking().Include(t => t.Steps).AsQueryable();
        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(t => t.Category == category);
        var templates = q.OrderBy(t => t.SortOrder).ThenBy(t => t.Name).ToList();
        var runCounts = RunCounts();
        return Ok(templates.Select(t => ToDto(t, runCounts, withSteps: false)));
    }

    [HttpGet("Summary")]
    public IActionResult Summary()
    {
        var templates = _db.Set<ChecklistTemplate>().AsNoTracking().Select(t => new { t.Category, t.IsActive }).ToList();
        var runs = _db.Set<ChecklistRun>().AsNoTracking().Select(r => r.Status).ToList();
        return Ok(new ChecklistSummary
        {
            Total         = templates.Count,
            Active        = templates.Count(t => t.IsActive),
            ActiveRuns    = runs.Count(s => s == "InProgress"),
            CompletedRuns = runs.Count(s => s == "Completed"),
            ByCategory    = templates.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => g.Count()),
        });
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var t = _db.Set<ChecklistTemplate>().AsNoTracking().Include(x => x.Steps).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Checklist template {id} not found.");
        return Ok(ToDto(t, RunCounts(), withSteps: true));
    }

    [HttpPost]
    public IActionResult Create([FromBody] ChecklistTemplateSaveRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var t = new ChecklistTemplate { CreatedAt = DateTime.UtcNow, InActive = false };
        Apply(t, req);
        _db.Set<ChecklistTemplate>().Add(t);
        _db.SaveChanges();
        return Ok(new { id = t.Id, message = "Checklist template created." });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ChecklistTemplateSaveRequest req)
    {
        var t = _db.Set<ChecklistTemplate>().Include(x => x.Steps).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Checklist template {id} not found.");

        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        _db.Set<ChecklistTemplateStep>().RemoveRange(t.Steps);
        t.Steps.Clear();
        Apply(t, req);
        t.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(new { id = t.Id, message = "Checklist template updated." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var t = _db.Set<ChecklistTemplate>().FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Checklist template {id} not found.");
        if (_db.Set<ChecklistRun>().Any(r => r.ChecklistTemplateId == id && r.Status == "InProgress"))
            return BadRequest("This template has a run in progress. Finish or sign off the run first.");

        _db.Set<ChecklistTemplate>().Remove(t);   // steps cascade; finished runs are kept
        _db.SaveChanges();
        return NoContent();
    }

    [HttpPost("{id:int}/Duplicate")]
    public IActionResult Duplicate(int id)
    {
        var src = _db.Set<ChecklistTemplate>().AsNoTracking().Include(x => x.Steps).FirstOrDefault(x => x.Id == id);
        if (src == null) return NotFound($"Checklist template {id} not found.");

        var code = src.Code + "-COPY";
        for (var n = 2; _db.Set<ChecklistTemplate>().Any(x => x.Code == code); n++) code = $"{src.Code}-COPY{n}";

        var copy = new ChecklistTemplate
        {
            Code = code.Length > 50 ? code[..50] : code, Name = $"{src.Name} (copy)",
            Category = src.Category, Module = src.Module, SubCategory = src.SubCategory, Description = src.Description,
            TriggerType = src.TriggerType, TriggerEvent = src.TriggerEvent, LinkedDocType = src.LinkedDocType,
            AssignedRole = src.AssignedRole, AssignedTo = src.AssignedTo,
            RequireSignOff = src.RequireSignOff, SignOffRole = src.SignOffRole,
            Tags = src.Tags, IconName = src.IconName, Version = src.Version,
            IsActive = false,   // a copy starts inactive so it isn't run by mistake before it's edited
            SortOrder = src.SortOrder, Remarks = src.Remarks,
            CreatedAt = DateTime.UtcNow, InActive = false,
            Steps = src.Steps.OrderBy(s => s.StepNo).Select(CopyStep).ToList(),
        };
        _db.Set<ChecklistTemplate>().Add(copy);
        _db.SaveChanges();
        return Ok(new { id = copy.Id, code = copy.Code, message = "Template duplicated." });
    }

    [HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_db.Set<ChecklistTemplate>().Any())
            return BadRequest("Checklist templates already exist; nothing was seeded.");

        var now = DateTime.UtcNow;
        var defaults = DefaultTemplates();
        foreach (var t in defaults) { t.CreatedAt = now; t.InActive = false; }
        _db.Set<ChecklistTemplate>().AddRange(defaults);
        _db.SaveChanges();
        return Ok(new { created = defaults.Count, message = $"Seeded {defaults.Count} checklist templates." });
    }

    // ── Runs ────────────────────────────────────────────────────────
    [HttpPost("{id:int}/Start")]
    public IActionResult Start(int id, [FromBody] ChecklistStartRequest req)
    {
        var t = _db.Set<ChecklistTemplate>().AsNoTracking().Include(x => x.Steps).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Checklist template {id} not found.");
        if (!t.IsActive) return BadRequest("This template is inactive. Activate it before starting a run.");
        if (t.Steps.Count == 0) return BadRequest("This template has no steps.");
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Give the run a name.");

        var now = DateTime.UtcNow;
        var run = new ChecklistRun
        {
            RunNo = NextRunNo(now), ChecklistTemplateId = t.Id, TemplateCode = t.Code,
            Name = Trim(req.Name, 200)!, ContextRef = Trim(req.ContextRef, 100),
            StartedBy = Trim(req.StartedBy, 100) ?? CurrentUserName(),
            DueDate = req.DueDate?.Date, Notes = Trim(req.Notes, 1000),
            Status = "InProgress", StartedAt = now,
            RequireSignOff = t.RequireSignOff, SignOffRole = t.SignOffRole,
            CreatedAt = now, InActive = false,
            StepResults = t.Steps.OrderBy(s => s.StepNo).Select(s => new ChecklistRunStep
            {
                StepNo = s.StepNo, Title = s.Title, StepType = s.StepType, GroupName = s.GroupName,
                IsRequired = s.IsRequired, DependsOnStepNo = s.DependsOnStepNo, LinkedRoute = s.LinkedRoute,
                StepStatus = "Pending",
            }).ToList(),
        };
        _db.Set<ChecklistRun>().Add(run);
        _db.SaveChanges();
        return Ok(ToRunDto(run, withSteps: true));
    }

    [HttpGet("Runs")]
    public IActionResult Runs([FromQuery] string? status)
    {
        var q = _db.Set<ChecklistRun>().AsNoTracking().Include(r => r.StepResults).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(r => r.Status == status);
        return Ok(q.OrderByDescending(r => r.StartedAt).ToList().Select(r => ToRunDto(r, withSteps: false)));
    }

    [HttpGet("Runs/{id:int}")]
    public IActionResult GetRun(int id)
    {
        var run = _db.Set<ChecklistRun>().AsNoTracking().Include(r => r.StepResults).FirstOrDefault(r => r.Id == id);
        if (run == null) return NotFound($"Checklist run {id} not found.");
        return Ok(ToRunDto(run, withSteps: true));
    }

    [HttpPut("Runs/{id:int}/Step/{stepNo:int}")]
    public IActionResult UpdateStep(int id, int stepNo, [FromBody] ChecklistStepUpdateRequest req)
    {
        var run = _db.Set<ChecklistRun>().Include(r => r.StepResults).FirstOrDefault(r => r.Id == id);
        if (run == null) return NotFound($"Checklist run {id} not found.");
        if (run.Status != "InProgress") return BadRequest($"This run is {run.Status} and can't be changed.");

        var step = run.StepResults.FirstOrDefault(s => s.StepNo == stepNo);
        if (step == null) return NotFound($"Step {stepNo} is not part of this run.");

        var status = StepStatuses.FirstOrDefault(s => s.Equals(req.StepStatus?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (status == null) return BadRequest($"Step status must be one of: {string.Join(", ", StepStatuses)}.");
        if (status == "Skipped" && step.IsRequired) return BadRequest($"Step {stepNo} is required and can't be skipped.");

        if ((status is "Done" or "Skipped") && step.DependsOnStepNo is int dep)
        {
            var before = run.StepResults.FirstOrDefault(s => s.StepNo == dep);
            if (before != null && before.StepStatus is not ("Done" or "Skipped"))
                return BadRequest($"Finish step {dep} (\"{before.Title}\") first.");
        }

        step.StepStatus = status;
        if (status == "Pending") { step.CompletedBy = null; step.CompletedAt = null; }
        else { step.CompletedBy = CurrentUserName(); step.CompletedAt = DateTime.UtcNow; }
        if (req.Note != null) step.Note = Trim(req.Note, 1000);
        run.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(ToRunDto(run, withSteps: true));
    }

    [HttpPost("Runs/{id:int}/SignOff")]
    public IActionResult SignOff(int id)
    {
        var run = _db.Set<ChecklistRun>().Include(r => r.StepResults).FirstOrDefault(r => r.Id == id);
        if (run == null) return NotFound($"Checklist run {id} not found.");
        if (run.Status != "InProgress") return BadRequest($"This run is already {run.Status}.");

        var open = run.StepResults.Where(s => s.StepStatus is not ("Done" or "Skipped")).OrderBy(s => s.StepNo).ToList();
        if (open.Count > 0)
            return BadRequest($"{open.Count} step(s) are not finished yet, starting with step {open[0].StepNo} (\"{open[0].Title}\").");

        var now = DateTime.UtcNow;
        run.Status = "Completed";
        run.CompletedAt = now;
        run.SignedOffBy = CurrentUserName();
        run.SignedOffAt = now;
        run.UpdatedAt = now;
        _db.SaveChanges();
        return Ok(ToRunDto(run, withSteps: true));
    }

    // ── Helpers ─────────────────────────────────────────────────────
    private string? Validate(ChecklistTemplateSaveRequest r, int? id)
    {
        if (string.IsNullOrWhiteSpace(r.Code)) return "Code is required.";
        if (string.IsNullOrWhiteSpace(r.Name)) return "Name is required.";
        var code = r.Code.Trim();
        if (code.Length > 50) return "Code can be at most 50 characters.";
        if (_db.Set<ChecklistTemplate>().Any(t => t.Code == code && t.Id != (id ?? 0)))
            return $"A template with code {code} already exists.";
        if (r.Category != null && !Categories.Contains(r.Category)) return $"Category must be one of: {string.Join(", ", Categories)}.";
        if (r.TriggerType != null && !TriggerTypes.Contains(r.TriggerType)) return $"Trigger must be one of: {string.Join(", ", TriggerTypes)}.";
        if (r.Steps.Count == 0) return "Add at least one step.";

        for (var i = 0; i < r.Steps.Count; i++)
        {
            var s = r.Steps[i];
            var n = i + 1;
            if (string.IsNullOrWhiteSpace(s.Title)) return $"Step {n} needs a title.";
            if (s.StepType != null && !StepTypes.Contains(s.StepType)) return $"Step {n}: type must be one of: {string.Join(", ", StepTypes)}.";
            if (s.EstimatedMinutes is < 0) return $"Step {n}: estimated minutes can't be negative.";
            if (s.DependsOnStepNo is int d && (d < 1 || d >= n)) return $"Step {n} can only depend on an earlier step.";
        }
        return null;
    }

    private static void Apply(ChecklistTemplate t, ChecklistTemplateSaveRequest r)
    {
        t.Code = r.Code.Trim();
        t.Name = r.Name.Trim();
        t.Category = r.Category ?? "General";
        t.Module = Trim(r.Module, 30);
        t.SubCategory = Trim(r.SubCategory, 50);
        t.Description = Trim(r.Description, 1000);
        t.TriggerType = r.TriggerType ?? "Manual";
        t.TriggerEvent = Trim(r.TriggerEvent, 100);
        t.LinkedDocType = Trim(r.LinkedDocType, 50);
        t.AssignedRole = Trim(r.AssignedRole, 50);
        t.AssignedTo = Trim(r.AssignedTo, 50);
        t.RequireSignOff = r.RequireSignOff;
        t.SignOffRole = r.RequireSignOff ? Trim(r.SignOffRole, 50) : null;
        t.Tags = Trim(r.Tags, 300);
        t.IconName = Trim(r.IconName, 20);
        t.Version = Trim(r.Version, 20);
        t.IsActive = r.IsActive;
        t.SortOrder = r.SortOrder;
        t.Remarks = Trim(r.Remarks, 500);
        // Step numbers always follow list order, whatever the client sent.
        t.Steps = r.Steps.Select((s, i) => new ChecklistTemplateStep
        {
            StepNo = i + 1, Title = s.Title.Trim(), Description = Trim(s.Description, 1000),
            StepType = s.StepType ?? "Action", GroupName = Trim(s.GroupName, 80),
            AssignedRole = Trim(s.AssignedRole, 50), AssignedTo = Trim(s.AssignedTo, 50),
            EstimatedMinutes = s.EstimatedMinutes ?? 0, IsRequired = s.IsRequired,
            RequiresAttachment = s.RequiresAttachment, RequiresNote = s.RequiresNote,
            DependsOnStepNo = s.DependsOnStepNo, LinkedRoute = Trim(s.LinkedRoute, 200),
            Instructions = Trim(s.Instructions, 2000), AcceptanceCriteria = Trim(s.AcceptanceCriteria, 1000),
        }).ToList();
    }

    private static ChecklistTemplateStep CopyStep(ChecklistTemplateStep s) => new()
    {
        StepNo = s.StepNo, Title = s.Title, Description = s.Description, StepType = s.StepType,
        GroupName = s.GroupName, AssignedRole = s.AssignedRole, AssignedTo = s.AssignedTo,
        EstimatedMinutes = s.EstimatedMinutes, IsRequired = s.IsRequired,
        RequiresAttachment = s.RequiresAttachment, RequiresNote = s.RequiresNote,
        DependsOnStepNo = s.DependsOnStepNo, LinkedRoute = s.LinkedRoute,
        Instructions = s.Instructions, AcceptanceCriteria = s.AcceptanceCriteria,
    };

    private Dictionary<int, int> RunCounts() =>
        _db.Set<ChecklistRun>().AsNoTracking()
            .GroupBy(r => r.ChecklistTemplateId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(x => x.Key, x => x.Count);

    private static ChecklistTemplateDto ToDto(ChecklistTemplate t, Dictionary<int, int> runCounts, bool withSteps)
    {
        var minutes = t.Steps.Sum(s => s.EstimatedMinutes);
        return new ChecklistTemplateDto
        {
            Id = t.Id, Code = t.Code, Name = t.Name, Category = t.Category, Module = t.Module,
            SubCategory = t.SubCategory, Description = t.Description, TriggerType = t.TriggerType,
            TriggerEvent = t.TriggerEvent, LinkedDocType = t.LinkedDocType,
            AssignedRole = t.AssignedRole, AssignedTo = t.AssignedTo,
            RequireSignOff = t.RequireSignOff, SignOffRole = t.SignOffRole,
            Tags = t.Tags, IconName = t.IconName, Version = t.Version,
            IsActive = t.IsActive, SortOrder = t.SortOrder, Remarks = t.Remarks,
            StepCount = t.Steps.Count,
            EstimatedTime = minutes < 60 ? $"{minutes}m" : $"{minutes / 60}h {minutes % 60}m",
            TotalRunCount = runCounts.GetValueOrDefault(t.Id),
            Steps = withSteps
                ? t.Steps.OrderBy(s => s.StepNo).Select(s => new ChecklistStepDto
                {
                    Id = s.Id, StepNo = s.StepNo, Title = s.Title, Description = s.Description,
                    StepType = s.StepType, GroupName = s.GroupName, AssignedRole = s.AssignedRole,
                    AssignedTo = s.AssignedTo, EstimatedMinutes = s.EstimatedMinutes, IsRequired = s.IsRequired,
                    RequiresAttachment = s.RequiresAttachment, RequiresNote = s.RequiresNote,
                    DependsOnStepNo = s.DependsOnStepNo, LinkedRoute = s.LinkedRoute,
                    Instructions = s.Instructions, AcceptanceCriteria = s.AcceptanceCriteria,
                }).ToList()
                : null,
        };
    }

    private static ChecklistRunDto ToRunDto(ChecklistRun r, bool withSteps)
    {
        var total = r.StepResults.Count;
        var finished = r.StepResults.Count(s => s.StepStatus is "Done" or "Skipped");
        return new ChecklistRunDto
        {
            Id = r.Id, RunNo = r.RunNo, ChecklistTemplateId = r.ChecklistTemplateId, TemplateCode = r.TemplateCode,
            Name = r.Name, ContextRef = r.ContextRef, StartedBy = r.StartedBy, DueDate = r.DueDate, Notes = r.Notes,
            Status = r.Status, StartedAt = r.StartedAt, CompletedAt = r.CompletedAt,
            RequireSignOff = r.RequireSignOff, SignOffRole = r.SignOffRole,
            SignedOffBy = r.SignedOffBy, SignedOffAt = r.SignedOffAt,
            TotalSteps = total, CompletedSteps = finished,
            ProgressPct = total == 0 ? 0 : (int)Math.Floor(finished * 100.0 / total),
            StepResults = withSteps
                ? r.StepResults.OrderBy(s => s.StepNo).Select(s => new ChecklistRunStepDto
                {
                    Id = s.Id, StepNo = s.StepNo, Title = s.Title, StepType = s.StepType, GroupName = s.GroupName,
                    IsRequired = s.IsRequired, DependsOnStepNo = s.DependsOnStepNo, LinkedRoute = s.LinkedRoute,
                    StepStatus = s.StepStatus, CompletedBy = s.CompletedBy, CompletedAt = s.CompletedAt, Note = s.Note,
                }).ToList()
                : null,
        };
    }

    private string NextRunNo(DateTime date)
    {
        var prefix = $"CLR-{date.Year}-";
        var last = _db.Set<ChecklistRun>().Where(r => r.RunNo.StartsWith(prefix)).OrderByDescending(r => r.Id).FirstOrDefault();
        if (last == null) return $"{prefix}00001";
        var parts = last.RunNo.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n) ? $"{prefix}{n + 1:D5}" : $"{prefix}00001";
    }

    private string CurrentUserName()
    {
        var id = this.CurrentUserId();
        var u = id == null ? null : _db.Set<UserEntity>().AsNoTracking().FirstOrDefault(x => x.Id == id);
        return u == null ? "unknown" : $"{u.UserName} ({u.UserId})";
    }

    private static string? Trim(string? s, int max)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        var t = s.Trim();
        return t.Length > max ? t[..max] : t;
    }

    private static ChecklistTemplateStep Step(string title, string type, int minutes, string? route = null,
        bool required = true, int? dependsOn = null, string? group = null, string? instructions = null) => new()
    {
        Title = title, StepType = type, EstimatedMinutes = minutes, LinkedRoute = route,
        IsRequired = required, DependsOnStepNo = dependsOn, GroupName = group, Instructions = instructions,
    };

    private static List<ChecklistTemplate> Numbered(params ChecklistTemplate[] templates)
    {
        foreach (var t in templates)
            for (var i = 0; i < t.Steps.Count; i++) t.Steps[i].StepNo = i + 1;
        return templates.ToList();
    }

    private static List<ChecklistTemplate> DefaultTemplates() => Numbered(
        new ChecklistTemplate
        {
            Code = "FIN-MONTH-END", Name = "Month-End Close", Category = "Finance", Module = "Financials",
            TriggerType = "OnMonthEnd", AssignedRole = "Accountant", RequireSignOff = true, SignOffRole = "Finance Manager",
            Description = "Close the books for the month: bank, receivables, payables, stock and review.",
            Tags = "close,month-end", Version = "1.0", SortOrder = 10,
            Steps =
            {
                Step("Import and match this month's bank statements", "Action", 30, "/Banking/BankStatement/List", group: "Bank"),
                Step("Reconcile every bank account", "Verification", 30, "/Banking/BankReconciliation/List", dependsOn: 1, group: "Bank"),
                Step("Deposit all undeposited receipts", "Action", 15, "/Banking/Reports/UndepositedFunds", group: "Bank"),
                Step("Post exchange rate differences", "Action", 10, "/Financials/ExchangeRateDifferences/List", required: false, group: "Ledger"),
                Step("Review open A/R items", "Review", 20, "/Sale-AR/Reports/OpenItems", group: "Receivables"),
                Step("Check stock status for negative or odd balances", "Review", 20, "/Inventory/Reports/StockStatus", group: "Inventory"),
                Step("Run and review the trial balance", "Report", 15, "/Financials/TrialBalance", group: "Review"),
                Step("Review the profit and loss", "Review", 15, "/Financials/PNL", dependsOn: 7, group: "Review"),
            },
        },
        new ChecklistTemplate
        {
            Code = "FIN-YEAR-END", Name = "Year-End Close", Category = "Finance", Module = "Financials",
            TriggerType = "OnYearEnd", AssignedRole = "Accountant", RequireSignOff = true, SignOffRole = "Finance Manager",
            Description = "Finish the last month, close the P&L to retained earnings and lock the year.",
            Tags = "close,year-end", Version = "1.0", SortOrder = 20,
            Steps =
            {
                Step("Complete the December month-end close", "Verification", 60, group: "Prepare"),
                Step("Count stock and post the differences", "Action", 120, "/Inventory/StockCycle/List", group: "Prepare"),
                Step("Review the balance sheet", "Review", 30, "/Financials/BalanceSheet", dependsOn: 1, group: "Review"),
                Step("Run period-end closing to retained earnings", "Action", 15, "/Financials/PeriodEndClosing", dependsOn: 3, group: "Close"),
                Step("Lock the year's posting periods", "Approval", 10, "/SystemInitialization/PostingPeriods", dependsOn: 4, group: "Close"),
            },
        },
        new ChecklistTemplate
        {
            Code = "INV-STOCK-COUNT", Name = "Stock Count", Category = "Inventory", Module = "Inventory",
            TriggerType = "Scheduled", AssignedRole = "Warehouse Supervisor",
            Description = "Physical count of feed, medicine and supplies, then post the differences.",
            Tags = "stock,count", Version = "1.0", SortOrder = 30,
            Steps =
            {
                Step("Print the stock status for each warehouse", "Report", 10, "/Inventory/Reports/StockStatus"),
                Step("Count every bin and record the quantities", "Action", 90, "/Inventory/StockCycle/List", dependsOn: 1),
                Step("Recount items with large differences", "Verification", 30, dependsOn: 2, required: false),
                Step("Post the count differences", "Approval", 15, "/Inventory/StockCycle/List", dependsOn: 2),
            },
        },
        new ChecklistTemplate
        {
            Code = "SAL-NEW-CUSTOMER", Name = "New Customer Setup", Category = "Sales", Module = "Sales",
            TriggerType = "Manual", AssignedRole = "Sales Officer",
            Description = "Everything needed before the first sale to a new buyer.",
            Tags = "customer,onboarding", Version = "1.0", SortOrder = 40,
            Steps =
            {
                Step("Create the business partner", "Action", 10, "/BusinessPartnersMaster/Create"),
                Step("Collect the tax ID and set tax status", "Verification", 10, dependsOn: 1),
                Step("Agree payment terms and credit limit", "Approval", 15, dependsOn: 1),
                Step("Send a welcome message with bank details", "Notification", 5, required: false),
            },
        },
        new ChecklistTemplate
        {
            Code = "HR-ONBOARDING", Name = "New Employee Onboarding", Category = "HR", Module = "HR",
            TriggerType = "Manual", AssignedRole = "HR Officer",
            Description = "First-week setup for a new farm worker or office employee.",
            Tags = "hr,onboarding", Version = "1.0", SortOrder = 50,
            Steps =
            {
                Step("Create the employee record", "Action", 15, "/HumanResources/EmployeeMasterData/Create"),
                Step("Collect ID and contract documents", "Upload", 15, dependsOn: 1),
                Step("Add to payroll", "Action", 10, "/HumanResources/PayRoll/List", dependsOn: 1),
                Step("Safety and animal-handling briefing", "Action", 60),
            },
        });
}

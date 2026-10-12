using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Financials.FinancialReportTemplates;

// ═══════════════════════════════════════════════════════════════
// GET    /FinancialReportTemplates              → templates, by type then code
// GET    /FinancialReportTemplates/Types        → the report types on offer
// GET    /FinancialReportTemplates/{id}         → one template with its rows
// POST   /FinancialReportTemplates              → create
// PUT    /FinancialReportTemplates/{id}         → replace header and rows
// POST   /FinancialReportTemplates/{id}/Duplicate → copy under a new code
// POST   /FinancialReportTemplates/{id}/SetDefault → the type's default template
// DELETE /FinancialReportTemplates/{id}         → remove
//
// A template is only useful if it can actually be evaluated, so the
// structural checks live here: row codes must be unique, formulas may
// only cite rows of this template, and neither the formula graph nor
// the parent hierarchy may contain a loop. A loop would hang the
// report that consumed the template, long after the mistake was saved.
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class FinancialReportTemplatesController : ControllerBase
{
    private readonly IFinancialReportTemplateRepository _repo;
    private readonly IMapper _mapper;
    private readonly MyDbContext _db;

    public FinancialReportTemplatesController(
        IFinancialReportTemplateRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    public class RunRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    // ── POST /FinancialReportTemplates/{id}/Run ─────────────────
    // Fills the template from the ledger. Balance sheets and trial balances take balances up to
    // To; profit and loss and cash flow templates take activity between From and To.
    [HttpPost("{id:int}/Run")]
    public IActionResult Run(int id, [FromBody] RunRequest req)
    {
        var t = _repo.GetAll().Include(x => x.Rows).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Template {id} not found.");
        if (!t.IsActive) return BadRequest($"Template '{t.Code}' is inactive.");
        if (req.To == null) return BadRequest("Choose the report date (To).");
        var cumulative = t.ReportType is "BalanceSheet" or "TrialBalance";
        if (!cumulative && req.From == null) return BadRequest("Choose the period (From and To) for this kind of report.");
        if (req.From != null && req.To.Value.Date < req.From.Value.Date) return BadRequest("The end date is before the start date.");
        if (t.Rows.Count == 0) return BadRequest($"Template '{t.Code}' has no rows.");

        var engine = new FarmingApi.Modules.Financials.FinancialReports.ReportEngine(_db);
        var sums = cumulative ? engine.Sum(null, req.To) : engine.Sum(req.From, req.To);
        var net = sums.ToDictionary(k => k.Key, k => k.Value.Net);
        var (lines, error) = FarmingApi.Modules.Financials.FinancialReports.TemplateRunner.Run(t, engine, net);
        if (error != null) return BadRequest(error);
        return Ok(new
        {
            template = new { t.Id, t.Code, t.Name, t.ReportType, t.Currency },
            from = cumulative ? null : req.From?.Date, to = req.To.Value.Date, basis = cumulative ? "Balances up to the report date" : "Activity in the period",
            lines,
        });
    }

    private static readonly string[] ReportTypes =
    {
        "BalanceSheet", "ProfitAndLoss", "TrialBalance", "CashFlow",
    };

    private static readonly string[] RowTypes =
    {
        "Title", "Account", "Formula", "Text", "PageBreak",
    };

    /// <summary>
    /// Row codes inside a formula. Anything starting with a letter is a
    /// reference; bare numbers are literals and are skipped.
    /// </summary>
    private static readonly Regex FormulaToken =
        new(@"[A-Za-z][A-Za-z0-9_\-]*", RegexOptions.Compiled);

    /// <summary>Characters a formula may contain besides row codes.</summary>
    private static readonly Regex FormulaShape =
        new(@"^[A-Za-z0-9_\-\+\*/\(\)\.\s]+$", RegexOptions.Compiled);

    private static string? Normalise(string? raw, string[] allowed) =>
        string.IsNullOrWhiteSpace(raw)
            ? null
            : allowed.FirstOrDefault(a => string.Equals(a, raw.Trim(), StringComparison.OrdinalIgnoreCase));

    private static List<string> ReferencedRows(string formula) =>
        FormulaToken.Matches(formula)
            .Select(m => m.Value.ToUpper())
            .Distinct()
            .ToList();

    /// <summary>
    /// Every structural rule for a template. Returns one message per problem so
    /// the screen can show them all at once rather than one save at a time.
    /// </summary>
    private List<string> Validate(FinancialReportTemplateRequest dto, int? existingId)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Code))
            problems.Add("Template code is required.");
        if (string.IsNullOrWhiteSpace(dto.Name))
            problems.Add("Template name is required.");

        if (Normalise(dto.ReportType, ReportTypes) == null)
            problems.Add($"'{dto.ReportType}' is not a valid report type. " +
                         $"Use one of: {string.Join(", ", ReportTypes)}.");

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var code = dto.Code.Trim().ToUpper();
            var clash = _repo.GetAll().Any(t => t.Code.ToUpper() == code && t.Id != existingId);
            if (clash) problems.Add($"Template code '{dto.Code.Trim()}' is already in use.");
        }

        if (dto.Rows == null || dto.Rows.Count == 0)
        {
            problems.Add("Add at least one row before saving.");
            return problems;
        }

        // ── Row codes ────────────────────────────────────────────
        var codes = new HashSet<string>();
        foreach (var (row, i) in dto.Rows.Select((r, i) => (r, i + 1)))
        {
            if (string.IsNullOrWhiteSpace(row.RowCode))
            {
                problems.Add($"Row {i}: row code is required.");
                continue;
            }
            var code = row.RowCode.Trim().ToUpper();
            if (!FormulaToken.IsMatch(code) || FormulaToken.Match(code).Value != code)
                problems.Add($"Row {i}: '{row.RowCode}' must start with a letter and contain " +
                             "only letters, digits, underscore or dash, so formulas can cite it.");
            if (!codes.Add(code))
                problems.Add($"Row {i}: row code '{row.RowCode}' is used more than once.");
            if (string.IsNullOrWhiteSpace(row.Description))
                problems.Add($"Row {i}: description is required.");
            if (row.Sign != 1 && row.Sign != -1)
                problems.Add($"Row {i}: sign must be 1 or -1.");
        }

        // ── Per-type requirements ────────────────────────────────
        foreach (var (row, i) in dto.Rows.Select((r, i) => (r, i + 1)))
        {
            var type = Normalise(row.RowType, RowTypes);
            if (type == null)
            {
                problems.Add($"Row {i}: '{row.RowType}' is not a valid row type. " +
                             $"Use one of: {string.Join(", ", RowTypes)}.");
                continue;
            }

            switch (type)
            {
                case "Account":
                    if (string.IsNullOrWhiteSpace(row.AccountFrom))
                        problems.Add($"Row {i}: an account row needs an account, or a range start.");
                    else if (!string.IsNullOrWhiteSpace(row.AccountTo) &&
                             string.CompareOrdinal(row.AccountFrom.Trim(), row.AccountTo.Trim()) > 0)
                        problems.Add($"Row {i}: account range starts after it ends " +
                                     $"({row.AccountFrom} to {row.AccountTo}).");
                    break;

                case "Formula":
                    if (string.IsNullOrWhiteSpace(row.Formula))
                    {
                        problems.Add($"Row {i}: a formula row needs a formula.");
                        break;
                    }
                    if (!FormulaShape.IsMatch(row.Formula))
                    {
                        problems.Add($"Row {i}: the formula contains characters that are not " +
                                     "row codes, numbers or + - * / ( ).");
                        break;
                    }
                    var open = row.Formula.Count(c => c == '(');
                    var close = row.Formula.Count(c => c == ')');
                    if (open != close)
                        problems.Add($"Row {i}: the formula has unbalanced brackets.");

                    foreach (var reference in ReferencedRows(row.Formula))
                    {
                        if (!codes.Contains(reference))
                            problems.Add($"Row {i}: the formula cites '{reference}', " +
                                         "which is not a row of this template.");
                        else if (reference == row.RowCode?.Trim().ToUpper())
                            problems.Add($"Row {i}: the formula cites its own row.");
                    }
                    break;
            }

            if (!string.IsNullOrWhiteSpace(row.ParentRowCode))
            {
                var parent = row.ParentRowCode.Trim().ToUpper();
                if (parent == row.RowCode?.Trim().ToUpper())
                    problems.Add($"Row {i}: a row cannot be its own parent.");
                else if (!codes.Contains(parent))
                    problems.Add($"Row {i}: parent row '{row.ParentRowCode}' " +
                                 "is not a row of this template.");
            }
        }

        // ── Loops ────────────────────────────────────────────────
        // Both graphs are checked only once the individual rows are sound;
        // walking a graph built from unresolved codes would report noise.
        if (problems.Count == 0)
        {
            var formulaGraph = dto.Rows
                .Where(r => Normalise(r.RowType, RowTypes) == "Formula" && r.Formula != null)
                .ToDictionary(
                    r => r.RowCode.Trim().ToUpper(),
                    r => ReferencedRows(r.Formula!).Where(codes.Contains).ToList());

            var loop = FindCycle(formulaGraph);
            if (loop != null)
                problems.Add($"The formulas form a loop: {string.Join(" → ", loop)}.");

            var parentGraph = dto.Rows
                .Where(r => !string.IsNullOrWhiteSpace(r.ParentRowCode))
                .ToDictionary(
                    r => r.RowCode.Trim().ToUpper(),
                    r => new List<string> { r.ParentRowCode!.Trim().ToUpper() });

            var parentLoop = FindCycle(parentGraph);
            if (parentLoop != null)
                problems.Add($"The row hierarchy forms a loop: {string.Join(" → ", parentLoop)}.");
        }

        return problems;
    }

    /// <summary>
    /// Depth-first search for a cycle. Returns the looping codes in order, or
    /// null when the graph is acyclic. Nodes not present as keys are leaves.
    /// </summary>
    private static List<string>? FindCycle(IDictionary<string, List<string>> graph)
    {
        var state = new Dictionary<string, int>();   // 0 unvisited, 1 on stack, 2 done
        var stack = new List<string>();

        List<string>? Walk(string node)
        {
            state.TryGetValue(node, out var s);
            if (s == 1)
            {
                // Trim the trail to the point the loop actually closes.
                var start = stack.IndexOf(node);
                var loop = stack.Skip(start).ToList();
                loop.Add(node);
                return loop;
            }
            if (s == 2) return null;

            state[node] = 1;
            stack.Add(node);

            if (graph.TryGetValue(node, out var next))
                foreach (var child in next)
                {
                    var found = Walk(child);
                    if (found != null) return found;
                }

            stack.RemoveAt(stack.Count - 1);
            state[node] = 2;
            return null;
        }

        foreach (var node in graph.Keys)
        {
            var found = Walk(node);
            if (found != null) return found;
        }
        return null;
    }

    private static List<FinancialReportTemplateRow> BuildRows(FinancialReportTemplateRequest dto)
    {
        var lineNum = 1;
        return dto.Rows.Select(r => new FinancialReportTemplateRow
        {
            LineNum       = lineNum++,
            RowCode       = r.RowCode.Trim().ToUpper(),
            Description   = r.Description.Trim(),
            RowType       = Normalise(r.RowType, RowTypes) ?? "Account",
            Level         = r.Level < 1 ? 1 : r.Level,
            ParentRowCode = string.IsNullOrWhiteSpace(r.ParentRowCode) ? null : r.ParentRowCode.Trim().ToUpper(),
            AccountFrom   = string.IsNullOrWhiteSpace(r.AccountFrom) ? null : r.AccountFrom.Trim().ToUpper(),
            AccountTo     = string.IsNullOrWhiteSpace(r.AccountTo)   ? null : r.AccountTo.Trim().ToUpper(),
            Formula       = string.IsNullOrWhiteSpace(r.Formula)     ? null : r.Formula.Trim().ToUpper(),
            Sign          = r.Sign == -1 ? -1 : 1,
            Bold          = r.Bold,
            Underline     = r.Underline,
            PrintZero     = r.PrintZero,
            Remarks       = r.Remarks,
        }).ToList();
    }

    private static void ApplyHeader(FinancialReportTemplate t, FinancialReportTemplateRequest dto)
    {
        t.Code        = dto.Code.Trim().ToUpper();
        t.Name        = dto.Name.Trim();
        t.ReportType  = Normalise(dto.ReportType, ReportTypes)!;
        t.Currency    = string.IsNullOrWhiteSpace(dto.Currency) ? "USD" : dto.Currency.Trim().ToUpper();
        t.Description = dto.Description;
        t.IsActive    = dto.IsActive;
        t.Remarks     = dto.Remarks;
    }

    /// <summary>
    /// Clears the default flag from every other template of the same type.
    /// Done in one place so "default" can never be true twice for a type.
    /// </summary>
    private void ClearOtherDefaults(FinancialReportTemplate keep)
    {
        var others = _repo.GetAll()
            .Where(t => t.ReportType == keep.ReportType && t.IsDefault && t.Id != keep.Id)
            .ToList();

        foreach (var other in others)
        {
            other.IsDefault = false;
            other.UpdatedAt = DateTime.UtcNow;
            _repo.Update(other);
        }
    }

    // ── GET /FinancialReportTemplates/Types ────────────────────
    // Declared before {id:int} so "Types" is never parsed as an id.
    [HttpGet("Types")]
    public IActionResult GetTypes() => Ok(new
    {
        reportTypes = new[]
        {
            new { code = "BalanceSheet",  label = "Balance Sheet" },
            new { code = "ProfitAndLoss", label = "Profit and Loss" },
            new { code = "TrialBalance",  label = "Trial Balance" },
            new { code = "CashFlow",      label = "Cash Flow" },
        },
        rowTypes = new[]
        {
            new { code = "Title",     label = "Title" },
            new { code = "Account",   label = "Account" },
            new { code = "Formula",   label = "Formula" },
            new { code = "Text",      label = "Text" },
            new { code = "PageBreak", label = "Page break" },
        },
    });

    // ── GET /FinancialReportTemplates ──────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? reportType, [FromQuery] bool? active)
    {
        var q = _repo.GetAll().Include(x => x.Rows).AsQueryable();

        if (!string.IsNullOrWhiteSpace(reportType))
        {
            var type = Normalise(reportType, ReportTypes);
            if (type == null) return BadRequest($"'{reportType}' is not a valid report type.");
            q = q.Where(t => t.ReportType == type);
        }
        if (active.HasValue) q = q.Where(t => t.IsActive == active.Value);

        var list = q.OrderBy(t => t.ReportType).ThenBy(t => t.Code).ToList();
        var result = _mapper.Map<List<FinancialReportTemplateResponse>>(list);
        // The list screen shows counts, not layouts; rows are fetched per template.
        foreach (var r in result) r.Rows.Clear();
        return Ok(result);
    }

    // ── GET /FinancialReportTemplates/{id} ─────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var t = _repo.GetAll().Include(x => x.Rows).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Report template not found: {id}");

        var dto = _mapper.Map<FinancialReportTemplateResponse>(t);
        dto.Rows = dto.Rows.OrderBy(r => r.LineNum).ToList();
        return Ok(dto);
    }

    // ── POST /FinancialReportTemplates ─────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] FinancialReportTemplateRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var problems = Validate(dto, null);
        if (problems.Count > 0)
            return BadRequest(new { message = "This template cannot be saved.", problems });

        var t = new FinancialReportTemplate { CreatedAt = DateTime.UtcNow, InActive = false };
        ApplyHeader(t, dto);
        t.Rows = BuildRows(dto);
        t.IsDefault = dto.IsDefault;

        _repo.Add(t);
        _repo.Commit();

        if (t.IsDefault)
        {
            ClearOtherDefaults(t);
            _repo.Commit();
        }

        return Ok(_mapper.Map<FinancialReportTemplateResponse>(t));
    }

    // ── PUT /FinancialReportTemplates/{id} ─────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] FinancialReportTemplateRequest dto)
    {
        var t = _repo.GetAll().Include(x => x.Rows).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Report template not found: {id}");

        var problems = Validate(dto, id);
        if (problems.Count > 0)
            return BadRequest(new { message = "This template cannot be saved.", problems });

        ApplyHeader(t, dto);
        t.Rows.Clear();
        foreach (var row in BuildRows(dto)) t.Rows.Add(row);
        t.IsDefault = dto.IsDefault;
        t.UpdatedAt = DateTime.UtcNow;

        _repo.Update(t);
        if (t.IsDefault) ClearOtherDefaults(t);
        _repo.Commit();

        return Ok(_mapper.Map<FinancialReportTemplateResponse>(t));
    }

    // ── POST /FinancialReportTemplates/{id}/Duplicate ──────────
    [HttpPost("{id:int}/Duplicate")]
    public IActionResult Duplicate(int id, [FromBody] FinancialReportTemplateDuplicateRequest? body)
    {
        var source = _repo.GetAll().Include(x => x.Rows).FirstOrDefault(x => x.Id == id);
        if (source == null) return NotFound($"Report template not found: {id}");

        var code = string.IsNullOrWhiteSpace(body?.Code)
            ? NextCopyCode(source.Code)
            : body!.Code!.Trim().ToUpper();

        if (_repo.GetAll().Any(t => t.Code.ToUpper() == code))
            return BadRequest($"Template code '{code}' is already in use.");

        var copy = new FinancialReportTemplate
        {
            Code        = code,
            Name        = string.IsNullOrWhiteSpace(body?.Name) ? $"{source.Name} (copy)" : body!.Name!.Trim(),
            ReportType  = source.ReportType,
            Currency    = source.Currency,
            Description = source.Description,
            // A copy is never the default; that has to be chosen deliberately.
            IsDefault   = false,
            IsActive    = source.IsActive,
            Remarks     = source.Remarks,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };

        foreach (var r in source.Rows.OrderBy(r => r.LineNum))
            copy.Rows.Add(new FinancialReportTemplateRow
            {
                LineNum       = r.LineNum,
                RowCode       = r.RowCode,
                Description   = r.Description,
                RowType       = r.RowType,
                Level         = r.Level,
                ParentRowCode = r.ParentRowCode,
                AccountFrom   = r.AccountFrom,
                AccountTo     = r.AccountTo,
                Formula       = r.Formula,
                Sign          = r.Sign,
                Bold          = r.Bold,
                Underline     = r.Underline,
                PrintZero     = r.PrintZero,
                Remarks       = r.Remarks,
            });

        _repo.Add(copy);
        _repo.Commit();

        return Ok(_mapper.Map<FinancialReportTemplateResponse>(copy));
    }

    /// <summary>BS-STD → BS-STD-COPY, then -COPY2, -COPY3 … until one is free.</summary>
    private string NextCopyCode(string sourceCode)
    {
        var taken = _repo.GetAll().Select(t => t.Code.ToUpper()).ToHashSet();
        var baseCode = $"{sourceCode.ToUpper()}-COPY";
        if (!taken.Contains(baseCode)) return baseCode;

        for (var n = 2; n < 1000; n++)
        {
            var candidate = $"{baseCode}{n}";
            if (!taken.Contains(candidate)) return candidate;
        }
        return $"{baseCode}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }

    // ── POST /FinancialReportTemplates/{id}/SetDefault ─────────
    [HttpPost("{id:int}/SetDefault")]
    public IActionResult SetDefault(int id)
    {
        var t = _repo.GetAll().Include(x => x.Rows).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Report template not found: {id}");
        if (!t.IsActive)
            return BadRequest("An inactive template cannot be made the default.");

        t.IsDefault = true;
        t.UpdatedAt = DateTime.UtcNow;
        _repo.Update(t);
        ClearOtherDefaults(t);
        _repo.Commit();

        return Ok(_mapper.Map<FinancialReportTemplateResponse>(t));
    }

    // ── DELETE /FinancialReportTemplates/{id} ──────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var t = _repo.GetAll().Include(x => x.Rows).FirstOrDefault(x => x.Id == id);
        if (t == null) return NotFound($"Report template not found: {id}");
        if (t.IsDefault)
            return BadRequest("This is the default template for its report type. " +
                              "Make another template the default first.");

        _repo.Remove(t);
        _repo.Commit();
        return NoContent();
    }
}

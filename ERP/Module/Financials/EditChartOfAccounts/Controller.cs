using AutoMapper;
using FarmingApi.Modules.Financials.ChartOfAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Financials.EditChartOfAccounts;

// ═══════════════════════════════════════════════════════════════
// GET    /EditChartOfAccounts             → batches, newest first
// GET    /EditChartOfAccounts/{id}        → one batch with its lines
// GET    /EditChartOfAccounts/Accounts    → account picker (OACT projection)
// POST   /EditChartOfAccounts             → create a batch (Open)
// PUT    /EditChartOfAccounts/{id}        → amend an Open batch
// POST   /EditChartOfAccounts/{id}/Validate → dry run, returns problems only
// POST   /EditChartOfAccounts/{id}/Apply    → Open -> Applied, writes OACT
// POST   /EditChartOfAccounts/{id}/Cancel   → Open -> Cancelled
// DELETE /EditChartOfAccounts/{id}        → remove a non-Applied batch
//
// Apply is all-or-nothing: every line is validated against the live
// chart first, and if any line fails nothing is written. That is the
// point of batching the edit rather than saving account by account.
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class EditChartOfAccountsController : ControllerBase
{
    private readonly IChartOfAccountsEditBatchRepository _repo;
    private readonly IChartOfAccountsRepository _accounts;
    private readonly IMapper _mapper;

    public EditChartOfAccountsController(
        IChartOfAccountsEditBatchRepository repo,
        IChartOfAccountsRepository accounts,
        IMapper mapper)
    {
        _repo     = repo;
        _accounts = accounts;
        _mapper   = mapper;
    }

    private const string StatusOpen      = "Open";
    private const string StatusApplied   = "Applied";
    private const string StatusCancelled = "Cancelled";

    private static readonly string[] ChangeTypes =
    {
        "Rename", "Move", "Activate", "Deactivate", "AllowPosting", "BlockPosting",
    };

    /// <summary>Change types that need a NewValue; the rest are flag flips.</summary>
    private static bool NeedsValue(string changeType) =>
        changeType is "Rename" or "Move";

    private string NextDocNo(int year)
    {
        var prefix = $"COE-{year}-";
        var last = _repo.GetAll()
            .Where(x => x.DocNo != null && x.DocNo.StartsWith(prefix))
            .OrderByDescending(x => x.DocNo)
            .Select(x => x.DocNo)
            .FirstOrDefault();

        var next = 1;
        if (last != null && int.TryParse(last[prefix.Length..], out var n)) next = n + 1;
        return prefix + next.ToString("D5");
    }

    private static string? NormaliseChangeType(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return ChangeTypes.FirstOrDefault(
            c => string.Equals(c, raw.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Shape checks that need no database access.</summary>
    private string? ValidateShape(ChartOfAccountsEditBatchRequest dto)
    {
        if (dto.Lines == null || dto.Lines.Count == 0)
            return "Add at least one change before saving.";

        foreach (var (line, i) in dto.Lines.Select((l, i) => (l, i + 1)))
        {
            if (string.IsNullOrWhiteSpace(line.AcctCode))
                return $"Line {i}: account code is required.";

            var type = NormaliseChangeType(line.ChangeType);
            if (type == null)
                return $"Line {i}: '{line.ChangeType}' is not a valid change type. " +
                       $"Use one of: {string.Join(", ", ChangeTypes)}.";

            if (NeedsValue(type) && string.IsNullOrWhiteSpace(line.NewValue))
                return $"Line {i}: {type} needs a new value.";
        }

        // Two lines touching the same field of the same account would make the
        // outcome depend on line order, so they are rejected outright.
        var dup = dto.Lines
            .Select(l => (Code: l.AcctCode.Trim().ToUpper(), Type: NormaliseChangeType(l.ChangeType)))
            .GroupBy(x => x)
            .FirstOrDefault(g => g.Count() > 1);
        if (dup != null)
            return $"Account '{dup.Key.Code}' has more than one {dup.Key.Type} change in this batch.";

        return null;
    }

    /// <summary>
    /// Checks every line against the live chart. Returns one message per
    /// problem line; an empty list means the batch is safe to apply.
    /// </summary>
    private List<string> ValidateAgainstChart(ChartOfAccountsEditBatch batch)
    {
        var problems = new List<string>();

        // One read of the chart serves every line; the chart is small enough
        // that this beats a query per line.
        var chart = _accounts.GetAll().ToList();
        var byCode = chart.ToDictionary(a => a.AcctCode.ToUpper(), a => a);

        foreach (var line in batch.Lines.OrderBy(l => l.LineNum))
        {
            var code = line.AcctCode.ToUpper();
            if (!byCode.TryGetValue(code, out var account))
            {
                problems.Add($"Line {line.LineNum}: account '{line.AcctCode}' does not exist.");
                continue;
            }

            switch (line.ChangeType)
            {
                case "Rename":
                    var newName = line.NewValue?.Trim();
                    if (string.IsNullOrWhiteSpace(newName))
                        problems.Add($"Line {line.LineNum}: the new name is empty.");
                    else if (newName.Length > 200)
                        problems.Add($"Line {line.LineNum}: the new name is longer than 200 characters.");
                    break;

                case "Move":
                    var parentCode = line.NewValue?.Trim().ToUpper();
                    if (string.IsNullOrWhiteSpace(parentCode))
                    {
                        problems.Add($"Line {line.LineNum}: the new parent is empty.");
                        break;
                    }
                    if (parentCode == code)
                    {
                        problems.Add($"Line {line.LineNum}: '{line.AcctCode}' cannot be its own parent.");
                        break;
                    }
                    if (!byCode.TryGetValue(parentCode, out var parent))
                    {
                        problems.Add($"Line {line.LineNum}: parent account '{line.NewValue}' does not exist.");
                        break;
                    }
                    if (!parent.IsGroup)
                        problems.Add($"Line {line.LineNum}: '{parent.AcctCode}' is a posting account, " +
                                     "so it cannot hold child accounts.");
                    // Walking up from the proposed parent must never reach the
                    // account being moved, or the branch would detach into a loop.
                    if (IsDescendantOf(parent, code, byCode))
                        problems.Add($"Line {line.LineNum}: '{parent.AcctCode}' sits under " +
                                     $"'{account.AcctCode}', so the move would create a loop.");
                    break;

                case "Deactivate":
                    if (chart.Any(a => a.FatherNum != null &&
                                       a.FatherNum.ToUpper() == code && a.IsActive))
                        problems.Add($"Line {line.LineNum}: '{account.AcctCode}' still has active " +
                                     "child accounts. Deactivate those first.");
                    break;

                case "BlockPosting":
                    if (account.Balance != 0)
                        problems.Add($"Line {line.LineNum}: '{account.AcctCode}' has a non-zero " +
                                     "balance, so posting cannot be blocked yet.");
                    break;
            }
        }

        return problems;
    }

    /// <summary>True when <paramref name="node"/> sits anywhere under ancestorCode.</summary>
    private static bool IsDescendantOf(
        ChartOfAccounts.ChartOfAccounts node,
        string ancestorCode,
        IDictionary<string, ChartOfAccounts.ChartOfAccounts> byCode)
    {
        var seen = new HashSet<string>();
        var current = node;
        while (current?.FatherNum != null)
        {
            var parentCode = current.FatherNum.ToUpper();
            if (parentCode == ancestorCode) return true;
            // A chart that is already circular must not hang this walk.
            if (!seen.Add(parentCode)) return false;
            if (!byCode.TryGetValue(parentCode, out current)) return false;
        }
        return false;
    }

    private static List<ChartOfAccountsEditLine> BuildLines(ChartOfAccountsEditBatchRequest dto)
    {
        var lineNum = 1;
        return dto.Lines.Select(l => new ChartOfAccountsEditLine
        {
            LineNum    = lineNum++,
            AcctCode   = l.AcctCode.Trim().ToUpper(),
            ChangeType = NormaliseChangeType(l.ChangeType)!,
            NewValue   = l.NewValue?.Trim(),
            Note       = l.Note,
            Applied    = false,
        }).ToList();
    }

    /// <summary>Fills in each line's AcctName from the live chart, for display.</summary>
    private void StampAccountNames(ChartOfAccountsEditBatch batch)
    {
        var codes = batch.Lines.Select(l => l.AcctCode).ToList();
        var names = _accounts.GetAll()
            .Where(a => codes.Contains(a.AcctCode))
            .ToDictionary(a => a.AcctCode.ToUpper(), a => a.AcctName);

        foreach (var line in batch.Lines)
            line.AcctName = names.TryGetValue(line.AcctCode, out var n) ? n : null;
    }

    // ── GET /EditChartOfAccounts ───────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? status)
    {
        var q = _repo.GetAll().Include(x => x.Lines).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status.Trim());

        var list = q.OrderByDescending(x => x.BatchDate).ThenByDescending(x => x.Id).ToList();
        var result = _mapper.Map<List<ChartOfAccountsEditBatchResponse>>(list);
        foreach (var r in result) r.Lines.Clear();
        return Ok(result);
    }

    // ── GET /EditChartOfAccounts/Accounts ──────────────────────
    // Declared before the {id:int} route so "Accounts" is never read as an id.
    [HttpGet("Accounts")]
    public IActionResult GetAccounts([FromQuery] string? search, [FromQuery] int take = 200)
    {
        var q = _accounts.GetAll();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            q = q.Where(a => a.AcctCode.ToLower().Contains(term)
                          || a.AcctName.ToLower().Contains(term));
        }

        var list = q.OrderBy(a => a.AcctCode)
                    .Take(Math.Clamp(take, 1, 1000))
                    .ToList();

        return Ok(_mapper.Map<List<ChartOfAccountsEditCandidate>>(list));
    }

    // ── GET /EditChartOfAccounts/{id} ──────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var batch = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (batch == null) return NotFound($"Edit batch not found: {id}");

        var dto = _mapper.Map<ChartOfAccountsEditBatchResponse>(batch);
        dto.Lines = dto.Lines.OrderBy(l => l.LineNum).ToList();
        return Ok(dto);
    }

    // ── POST /EditChartOfAccounts ──────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] ChartOfAccountsEditBatchRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var problem = ValidateShape(dto);
        if (problem != null) return BadRequest(problem);

        var batch = new ChartOfAccountsEditBatch
        {
            Status      = StatusOpen,
            BatchDate   = (dto.BatchDate ?? DateTime.UtcNow).Date,
            Description = dto.Description,
            Remarks     = dto.Remarks,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };
        batch.Lines = BuildLines(dto);
        batch.DocNo = NextDocNo(batch.BatchDate.Year);
        StampAccountNames(batch);

        _repo.Add(batch);
        _repo.Commit();

        return Ok(_mapper.Map<ChartOfAccountsEditBatchResponse>(batch));
    }

    // ── PUT /EditChartOfAccounts/{id} ──────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ChartOfAccountsEditBatchRequest dto)
    {
        var batch = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (batch == null) return NotFound($"Edit batch not found: {id}");
        if (batch.Status != StatusOpen)
            return BadRequest($"This batch is {batch.Status.ToLower()} and can no longer be changed.");

        var problem = ValidateShape(dto);
        if (problem != null) return BadRequest(problem);

        batch.BatchDate   = (dto.BatchDate ?? batch.BatchDate).Date;
        batch.Description = dto.Description;
        batch.Remarks     = dto.Remarks;

        batch.Lines.Clear();
        foreach (var line in BuildLines(dto)) batch.Lines.Add(line);
        StampAccountNames(batch);
        batch.UpdatedAt = DateTime.UtcNow;

        _repo.Update(batch);
        _repo.Commit();

        return Ok(_mapper.Map<ChartOfAccountsEditBatchResponse>(batch));
    }

    // ── POST /EditChartOfAccounts/{id}/Validate ────────────────
    [HttpPost("{id:int}/Validate")]
    public IActionResult ValidateBatch(int id)
    {
        var batch = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (batch == null) return NotFound($"Edit batch not found: {id}");

        var problems = ValidateAgainstChart(batch);
        return Ok(new { ok = problems.Count == 0, problems });
    }

    // ── POST /EditChartOfAccounts/{id}/Apply ───────────────────
    [HttpPost("{id:int}/Apply")]
    public IActionResult Apply(int id, [FromBody] ChartOfAccountsEditApplyRequest? body)
    {
        var batch = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (batch == null) return NotFound($"Edit batch not found: {id}");
        if (batch.Status == StatusApplied)   return BadRequest("This batch has already been applied.");
        if (batch.Status == StatusCancelled) return BadRequest("A cancelled batch cannot be applied.");

        var problems = ValidateAgainstChart(batch);
        if (problems.Count > 0)
            return BadRequest(new { message = "This batch cannot be applied.", problems });

        var chart  = _accounts.GetAll().ToList();
        var byCode = chart.ToDictionary(a => a.AcctCode.ToUpper(), a => a);
        var touched = new HashSet<ChartOfAccounts.ChartOfAccounts>();

        foreach (var line in batch.Lines.OrderBy(l => l.LineNum))
        {
            var account = byCode[line.AcctCode.ToUpper()];

            switch (line.ChangeType)
            {
                case "Rename":
                    line.OldValue   = account.AcctName;
                    account.AcctName = line.NewValue!.Trim();
                    break;

                case "Move":
                    line.OldValue = account.FatherNum;
                    var parent = byCode[line.NewValue!.Trim().ToUpper()];
                    account.FatherNum = parent.AcctCode;
                    // The subtree keeps its shape, so every descendant shifts by
                    // the same amount the moved account did.
                    var shift = parent.Level + 1 - account.Level;
                    account.Level += shift;
                    if (shift != 0)
                        foreach (var d in Descendants(account, chart))
                        {
                            d.Level += shift;
                            touched.Add(d);
                        }
                    break;

                case "Activate":
                    line.OldValue    = account.IsActive.ToString();
                    account.IsActive = true;
                    break;

                case "Deactivate":
                    line.OldValue    = account.IsActive.ToString();
                    account.IsActive = false;
                    break;

                case "AllowPosting":
                    line.OldValue        = account.AllowPosting.ToString();
                    account.AllowPosting = true;
                    break;

                case "BlockPosting":
                    line.OldValue        = account.AllowPosting.ToString();
                    account.AllowPosting = false;
                    break;
            }

            account.UpdatedAt = DateTime.UtcNow;
            touched.Add(account);
            line.Applied = true;
        }

        foreach (var account in touched) _accounts.Update(account);

        batch.Status    = StatusApplied;
        batch.AppliedAt = DateTime.UtcNow;
        batch.AppliedBy = string.IsNullOrWhiteSpace(body?.AppliedBy) ? "system" : body!.AppliedBy!.Trim();
        batch.UpdatedAt = DateTime.UtcNow;
        _repo.Update(batch);

        // One Commit for the batch and the accounts — they share the DbContext,
        // so the whole apply lands in a single SaveChanges or not at all.
        _repo.Commit();

        return Ok(_mapper.Map<ChartOfAccountsEditBatchResponse>(batch));
    }

    /// <summary>Every account under <paramref name="root"/>, breadth first.</summary>
    private static List<ChartOfAccounts.ChartOfAccounts> Descendants(
        ChartOfAccounts.ChartOfAccounts root,
        List<ChartOfAccounts.ChartOfAccounts> chart)
    {
        var result  = new List<ChartOfAccounts.ChartOfAccounts>();
        var frontier = new Queue<string>();
        var seen    = new HashSet<string> { root.AcctCode.ToUpper() };
        frontier.Enqueue(root.AcctCode.ToUpper());

        while (frontier.Count > 0)
        {
            var code = frontier.Dequeue();
            foreach (var child in chart.Where(
                a => a.FatherNum != null && a.FatherNum.ToUpper() == code))
            {
                if (!seen.Add(child.AcctCode.ToUpper())) continue;
                result.Add(child);
                frontier.Enqueue(child.AcctCode.ToUpper());
            }
        }
        return result;
    }

    // ── POST /EditChartOfAccounts/{id}/Cancel ──────────────────
    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var batch = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (batch == null) return NotFound($"Edit batch not found: {id}");
        if (batch.Status == StatusApplied)
            return BadRequest("An applied batch cannot be cancelled. Raise a new batch to reverse it.");
        if (batch.Status == StatusCancelled) return BadRequest("This batch is already cancelled.");

        batch.Status    = StatusCancelled;
        batch.UpdatedAt = DateTime.UtcNow;
        _repo.Update(batch);
        _repo.Commit();

        return Ok(_mapper.Map<ChartOfAccountsEditBatchResponse>(batch));
    }

    // ── DELETE /EditChartOfAccounts/{id} ───────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var batch = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (batch == null) return NotFound($"Edit batch not found: {id}");
        if (batch.Status == StatusApplied)
            return BadRequest("An applied batch cannot be deleted; it is the audit trail of the change.");

        _repo.Remove(batch);
        _repo.Commit();
        return NoContent();
    }
}

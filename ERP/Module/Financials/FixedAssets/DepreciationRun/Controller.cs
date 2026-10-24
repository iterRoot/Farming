using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using JournalEntryNumberService = FarmingApi.Modules.Financials.JournalEntry.JournalEntryNumberService;
using CoaEntity           = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using PostingPeriodEntity = FarmingApi.Modules.Administration.PostingPeriods.PostingPeriod;

namespace FarmingApi.Modules.Financials.FixedAssets.DepreciationRun;

// ═══════════════════════════════════════════════════════════════════
// DEPRECIATION RUN  (route: /DepreciationRun)
// POST /DepreciationRun/Preview        → what a month's run would post
// POST /DepreciationRun                → post it (one JE for the whole run)
// GET  /DepreciationRun                → run history
// GET  /DepreciationRun/{id}
// POST /DepreciationRun/{id}/Reverse   → void the JE, move the assets back
//
// Months run in sequence: once any run is posted, the next one must be the
// following month, so no month is silently skipped or posted twice.
// Monthly amount per method:
//   Straight Line        (cost − salvage − accumulated) ÷ remaining months
//   Declining Balance    net book value × 2 ÷ useful life months, the last
//                        remaining month takes whatever is left
//   Immediate Write-off  everything depreciable, in one run
//   No Depreciation      skipped
// Nothing ever depreciates below salvage value.
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class DepreciationRunController : ControllerBase
{
    private readonly IDepreciationRunRepository _repository;
    private readonly IMapper                    _mapper;
    private readonly MyDbContext                _db;

    public DepreciationRunController(IDepreciationRunRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines)
            .OrderByDescending(x => x.PeriodYear).ThenByDescending(x => x.PeriodMonth).ThenByDescending(x => x.Id)
            .ToList();
        return Ok(_mapper.Map<List<DepreciationRunResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<DepreciationRunResponse>(e));
    }

    [HttpPost("Preview")]
    public IActionResult Preview([FromBody] DepreciationPeriodRequest req)
    {
        var periodError = ValidatePeriod(req.PeriodYear, req.PeriodMonth);
        if (periodError != null) return BadRequest(periodError);

        var (lines, skipped) = Compute(req.PeriodYear, req.PeriodMonth);
        return Ok(new DepreciationPreviewResponse
        {
            PeriodYear    = req.PeriodYear,
            PeriodMonth   = req.PeriodMonth,
            BlockedReason = SequenceError(req.PeriodYear, req.PeriodMonth),
            TotalAmount   = lines.Sum(l => l.Amount),
            Lines         = lines,
            Skipped       = skipped,
        });
    }

    [HttpPost]
    public IActionResult Create([FromBody] DepreciationRunCreateRequest req)
    {
        var periodError = ValidatePeriod(req.PeriodYear, req.PeriodMonth);
        if (periodError != null) return BadRequest(periodError);

        var periodStart = new DateTime(req.PeriodYear, req.PeriodMonth, 1);
        var periodEnd   = periodStart.AddMonths(1).AddDays(-1);
        if (req.PostingDate.Date < periodStart || req.PostingDate.Date > periodEnd)
            return BadRequest($"Posting date must fall in {periodStart:MMMM yyyy}.");

        var sequenceError = SequenceError(req.PeriodYear, req.PeriodMonth);
        if (sequenceError != null) return BadRequest(sequenceError);

        var (lines, _) = Compute(req.PeriodYear, req.PeriodMonth);
        if (lines.Count == 0) return BadRequest("No asset has depreciation due for this period.");

        var accountError = CheckPostable(lines.SelectMany(l => new[] { l.ExpenseAccount, l.AccumulatedAccount }));
        if (accountError != null) return BadRequest(accountError);

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var run = new DepreciationRun
            {
                RunNo       = NextNumber(),
                PeriodYear  = req.PeriodYear,
                PeriodMonth = req.PeriodMonth,
                PostingDate = req.PostingDate.Date,
                Status      = "Posted",
                TotalAmount = lines.Sum(l => l.Amount),
                Remarks     = req.Remarks,
                CreatedAt   = DateTime.UtcNow,
                InActive    = false,
            };

            var assets = _db.Set<AssetMaster>().Where(a => lines.Select(l => l.AssetId).Contains(a.Id))
                .ToDictionary(a => a.Id);
            var n = 1;
            foreach (var l in lines)
            {
                var asset = assets[l.AssetId];
                run.Lines.Add(new DepreciationRunLine
                {
                    LineNum = n++, AssetId = asset.Id, ItemNo = asset.ItemNo, Description = asset.Description,
                    Method = asset.DepreciationMethod, Amount = l.Amount,
                    AccumulatedBefore = l.AccumulatedBefore, AccumulatedAfter = l.AccumulatedAfter,
                    RemainingLifeBefore = l.RemainingLifeBefore, RemainingLifeAfter = l.RemainingLifeAfter,
                    StatusBefore = asset.AssetStatus,
                    ExpenseAccount = l.ExpenseAccount, AccumulatedAccount = l.AccumulatedAccount,
                });

                asset.AccumulatedDepreciation = l.AccumulatedAfter;
                asset.RemainingLifeMonths     = l.RemainingLifeAfter;
                if (asset.AssetStatus == "New") asset.AssetStatus = "Active";
                asset.UpdatedAt = DateTime.UtcNow;
            }

            _repository.Add(run);
            _db.SaveChanges();

            var je = BuildJournalEntry(run);
            _db.Set<JournalEntryDoc>().Add(je);
            _db.SaveChanges();

            run.JournalEntryId = je.Id;
            _db.SaveChanges();
            tx.Commit();

            return Ok(new
            {
                message        = $"Depreciation run {run.RunNo} posted.",
                id             = run.Id,
                runNo          = run.RunNo,
                totalAmount    = run.TotalAmount,
                assets         = run.Lines.Count,
                journalEntryId = je.Id,
                journalNo      = je.JrnlNo,
            });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpPost("{id:int}/Reverse")]
    public IActionResult Reverse(int id)
    {
        var run = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (run == null) return NotFound();
        if (run.Status != "Posted") return BadRequest($"Run {run.RunNo} is {run.Status}, only a Posted run can be reversed.");

        var runKey = run.PeriodYear * 12 + run.PeriodMonth;
        var laterPosted = _repository.GetAll()
            .Where(r => r.Status == "Posted" && r.Id != run.Id)
            .AsEnumerable()
            .Any(r => r.PeriodYear * 12 + r.PeriodMonth > runKey);
        if (laterPosted) return BadRequest("Reverse the later runs first: only the latest posted run can be reversed.");

        var lockError = PeriodLockError(run.PostingDate);
        if (lockError != null) return BadRequest(lockError);

        var assetIds = run.Lines.Select(l => l.AssetId).ToList();
        var assets = _db.Set<AssetMaster>().Where(a => assetIds.Contains(a.Id)).ToDictionary(a => a.Id);
        foreach (var l in run.Lines)
        {
            if (!assets.TryGetValue(l.AssetId, out var asset))
                return BadRequest($"Asset {l.ItemNo} no longer exists, so run {run.RunNo} can't be reversed.");
            if (asset.AccumulatedDepreciation != l.AccumulatedAfter)
                return BadRequest($"Asset {l.ItemNo}'s accumulated depreciation has changed since this run ({asset.AccumulatedDepreciation:N2}, expected {l.AccumulatedAfter:N2}).");
        }

        using var tx = _db.Database.BeginTransaction();
        try
        {
            foreach (var l in run.Lines)
            {
                var asset = assets[l.AssetId];
                asset.AccumulatedDepreciation = l.AccumulatedBefore;
                asset.RemainingLifeMonths     = l.RemainingLifeBefore;
                asset.AssetStatus             = l.StatusBefore;
                asset.UpdatedAt               = DateTime.UtcNow;
            }

            if (run.JournalEntryId != null)
            {
                var je = _db.Set<JournalEntryDoc>().FirstOrDefault(j => j.Id == run.JournalEntryId);
                if (je != null && je.Status != "V")
                {
                    je.Status    = "V";
                    je.UpdatedAt = DateTime.UtcNow;
                }
            }

            run.Status    = "Reversed";
            run.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();
            tx.Commit();
            return Ok(new { message = $"Depreciation run {run.RunNo} reversed." });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ── Calculation ──────────────────────────────────────────────
    private (List<DepreciationPreviewLine> Lines, List<DepreciationSkippedAsset> Skipped) Compute(int year, int month)
    {
        var periodEnd = DateOnly.FromDateTime(new DateTime(year, month, 1).AddMonths(1).AddDays(-1));
        var lines   = new List<DepreciationPreviewLine>();
        var skipped = new List<DepreciationSkippedAsset>();

        foreach (var a in _db.Set<AssetMaster>().OrderBy(a => a.ItemNo).ToList())
        {
            void Skip(string reason) => skipped.Add(new DepreciationSkippedAsset { ItemNo = a.ItemNo, Description = a.Description, Reason = reason });

            if (!a.Active || a.InActive == true || a.AssetStatus is "Inactive" or "Deactivated") { Skip("Asset is inactive"); continue; }
            if (a.DepreciationMethod == "No Depreciation") { Skip("Method is No Depreciation"); continue; }
            if (a.CapitalizationDate == null) { Skip("No capitalization date"); continue; }
            if (a.CapitalizationDate > periodEnd) { Skip($"Capitalized after this period ({a.CapitalizationDate:yyyy-MM-dd})"); continue; }

            var depreciable = a.AcquisitionCost - a.SalvageValue - a.AccumulatedDepreciation;
            if (depreciable <= 0) { Skip("Fully depreciated"); continue; }
            if (string.IsNullOrWhiteSpace(a.DepreciationAccount)) { Skip("No depreciation expense account"); continue; }
            if (string.IsNullOrWhiteSpace(a.AccumulatedDepreciationAccount)) { Skip("No accumulated depreciation account"); continue; }

            decimal amount;
            int remainingAfter;
            switch (a.DepreciationMethod)
            {
                case "Immediate Write-off":
                    amount = depreciable;
                    remainingAfter = 0;
                    break;
                case "Straight Line":
                    if (a.RemainingLifeMonths <= 0) { Skip("No remaining useful life"); continue; }
                    amount = a.RemainingLifeMonths == 1 ? depreciable : Math.Round(depreciable / a.RemainingLifeMonths, 2);
                    remainingAfter = a.RemainingLifeMonths - 1;
                    break;
                case "Declining Balance":
                    if (a.RemainingLifeMonths <= 0 || a.UsefulLifeMonths <= 0) { Skip("No remaining useful life"); continue; }
                    amount = a.RemainingLifeMonths == 1
                        ? depreciable
                        : Math.Round((a.AcquisitionCost - a.AccumulatedDepreciation) * 2m / a.UsefulLifeMonths, 2);
                    remainingAfter = a.RemainingLifeMonths - 1;
                    break;
                default:
                    Skip($"Unknown method \"{a.DepreciationMethod}\"");
                    continue;
            }

            amount = Math.Min(amount, depreciable);
            if (amount <= 0) { Skip("Nothing to depreciate this month"); continue; }

            lines.Add(new DepreciationPreviewLine
            {
                AssetId = a.Id, ItemNo = a.ItemNo, Description = a.Description, Method = a.DepreciationMethod,
                AcquisitionCost = a.AcquisitionCost, SalvageValue = a.SalvageValue,
                AccumulatedBefore = a.AccumulatedDepreciation, Amount = amount,
                AccumulatedAfter = a.AccumulatedDepreciation + amount,
                NetBookValueAfter = a.AcquisitionCost - a.AccumulatedDepreciation - amount,
                RemainingLifeBefore = a.RemainingLifeMonths, RemainingLifeAfter = remainingAfter,
                ExpenseAccount = a.DepreciationAccount!, AccumulatedAccount = a.AccumulatedDepreciationAccount!,
            });
        }
        return (lines, skipped);
    }

    // ── Guards ───────────────────────────────────────────────────
    private static string? ValidatePeriod(int year, int month)
    {
        if (year < 2000 || year > 2100) return "Period year is out of range.";
        if (month < 1 || month > 12) return "Period month must be 1–12.";
        return null;
    }

    private string? SequenceError(int year, int month)
    {
        var key = year * 12 + month;
        var posted = _repository.GetAll().Where(r => r.Status == "Posted")
            .Select(r => new { r.PeriodYear, r.PeriodMonth, r.RunNo }).ToList();
        var same = posted.FirstOrDefault(r => r.PeriodYear == year && r.PeriodMonth == month);
        if (same != null) return $"{new DateTime(year, month, 1):MMMM yyyy} already has a posted run ({same.RunNo}).";
        if (posted.Count == 0) return null;

        var latest = posted.Max(r => r.PeriodYear * 12 + r.PeriodMonth);
        if (key < latest) return "A later month has already been posted. Reverse the later runs first.";
        if (key > latest + 1)
        {
            var nextKey = latest + 1;   // key = year*12 + month, month 1–12
            var next = new DateTime((nextKey - 1) / 12, (nextKey - 1) % 12 + 1, 1);
            return $"Months must run in order. The next month to post is {next:MMMM yyyy}.";
        }
        return null;
    }

    private string? PeriodLockError(DateTime date)
    {
        var d = DateOnly.FromDateTime(date);
        var period = _db.Set<PostingPeriodEntity>().AsNoTracking()
            .FirstOrDefault(p => p.PostingDateFrom != null && p.PostingDateTo != null
                              && p.PostingDateFrom <= d && p.PostingDateTo >= d);
        if (period == null) return null;
        return period.PeriodStatus is "Locked" or "Locked Except Sales"
            ? $"The posting period for {date:yyyy-MM-dd} is {period.PeriodStatus}."
            : null;
    }

    private string? CheckPostable(IEnumerable<string> codes)
    {
        foreach (var code in codes.Distinct())
        {
            var acct = _db.Set<CoaEntity>().FirstOrDefault(a => a.AcctCode == code);
            if (acct == null) return $"G/L account {code} doesn't exist in the Chart of Accounts.";
            if (acct.IsGroup || !acct.IsActive || !acct.AllowPosting || acct.Frozen)
                return $"G/L account {code} can't be posted to (it's a title, inactive, frozen, or blocks posting).";
        }
        return null;
    }

    private string NextNumber()
    {
        var prefix = $"DEPR-{DateTime.UtcNow.Year}-";
        var highest = _repository.GetAll().Where(r => r.RunNo.StartsWith(prefix))
            .Select(r => r.RunNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }

    private JournalEntryDoc BuildJournalEntry(DepreciationRun run)
    {
        var je = new JournalEntryDoc
        {
            JrnlNo          = JournalEntryNumberService.Next(_db, run.PostingDate),
            TransDate       = run.PostingDate,
            DueDate         = run.PostingDate,
            DocDate         = run.PostingDate,
            RefNo           = run.RunNo,
            Memo            = $"Depreciation {new DateTime(run.PeriodYear, run.PeriodMonth, 1):MMMM yyyy} — {run.RunNo}",
            TransType       = "KDPR",
            BaseDocType     = "KDPR",
            BaseDocEntry    = run.Id,
            ExchangeRate    = 1,
            Status          = "O",
            IsAutoGenerated = true,
            VersionNum      = 1,
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };

        var names = _db.Set<CoaEntity>()
            .Where(a => run.Lines.Select(l => l.ExpenseAccount).Contains(a.AcctCode)
                     || run.Lines.Select(l => l.AccumulatedAccount).Contains(a.AcctCode))
            .ToDictionary(a => a.AcctCode, a => a.AcctName);

        var lineNum = 1;
        foreach (var l in run.Lines.OrderBy(l => l.LineNum))
        {
            var memo = $"{run.RunNo} — {l.ItemNo}";
            je.Lines.Add(new JournalEntryLine
            {
                LineNum = lineNum++, AccountCode = l.ExpenseAccount, AccountName = names.GetValueOrDefault(l.ExpenseAccount, l.ExpenseAccount),
                Debit = l.Amount, DebitLC = l.Amount, LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
            });
            je.Lines.Add(new JournalEntryLine
            {
                LineNum = lineNum++, AccountCode = l.AccumulatedAccount, AccountName = names.GetValueOrDefault(l.AccumulatedAccount, l.AccumulatedAccount),
                Credit = l.Amount, CreditLC = l.Amount, LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
            });
        }

        je.TotalDebit  = je.Lines.Sum(x => x.Debit);
        je.TotalCredit = je.Lines.Sum(x => x.Credit);
        je.TotalLC     = je.TotalDebit;
        je.TotalFC     = je.TotalDebit;
        return je;
    }
}

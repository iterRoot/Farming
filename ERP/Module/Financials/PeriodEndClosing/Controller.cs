using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using Je = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JeLineEntity = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using JeNumber = FarmingApi.Modules.Financials.JournalEntry.JournalEntryNumberService;
using Coa = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using Gld = FarmingApi.Modules.Financials.GLAccountDetermination.GLAccountDetermination;

namespace FarmingApi.Modules.Financials.PeriodEndClosing;

// ═══════════════════════════════════════════════════════════════════
// PERIOD-END / YEAR-END CLOSING  (route: /PeriodEndClosing)
// GET  /PeriodEndClosing                 → past runs
// GET  /PeriodEndClosing/{id}
// GET  /PeriodEndClosing/Preview?from=&to=&retainedEarnings=
//        → what the run would close, with warnings. Posts nothing.
// POST /PeriodEndClosing                 → posts the closing journal entry
// POST /PeriodEndClosing/{id}/Void       → voids it; the year is open again
// ═══════════════════════════════════════════════════════════════════
public class PeriodEndClosingController : MyController
{
    private readonly IPeriodEndClosingRepository _repository;
    private readonly MyDbContext                 _db;

    public PeriodEndClosingController(IPeriodEndClosingRepository repository, MyDbContext db)
    {
        _repository = repository;
        _db         = db;
    }

    [HttpGet]
    public IActionResult Gets()
    {
        var runs = _repository.GetAll().Include(r => r.Lines)
            .OrderByDescending(r => r.ToDate).ThenByDescending(r => r.Id).ToList();
        return Ok(runs.Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var run = _repository.GetAll().Include(r => r.Lines).FirstOrDefault(r => r.Id == id);
        return run == null ? NotFound($"Closing run {id} not found") : Ok(ToResponse(run));
    }

    [HttpGet("Preview")]
    public IActionResult Preview([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? retainedEarnings = null)
    {
        if (from == default || to == default) return BadRequest("Enter the first and last day of the year to close.");
        if (from > to) return BadRequest("The start date must be on or before the end date.");

        var reCode = string.IsNullOrWhiteSpace(retainedEarnings)
            ? _db.Set<Gld>().AsNoTracking().FirstOrDefault()?.RetainedEarnings
            : retainedEarnings.Trim();
        var re = string.IsNullOrWhiteSpace(reCode) ? null
            : _db.Set<Coa>().AsNoTracking().FirstOrDefault(a => a.AcctCode == reCode);

        var lines = BuildLines(from, to, reCode, out var warnings);
        var prior = _repository.GetAll().FirstOrDefault(r => r.Status == "P" && r.FromDate <= to && r.ToDate >= from);

        if (re == null) warnings.Insert(0, "No retained earnings account set. Choose one, or set it in GL Account Determination.");
        else if (!re.AllowPosting) warnings.Insert(0, $"Retained earnings account {re.AcctCode} does not allow posting.");
        if (prior != null) warnings.Insert(0, $"This period was already closed by {prior.DocNum}. Void it before closing again.");
        if (lines.Count == 0) warnings.Add("No profit-and-loss account has a balance in this period — there is nothing to close.");

        var revenue = lines.Where(l => l.AccountType == "I").Sum(l => -l.Balance);
        var expense = lines.Where(l => l.AccountType == "X").Sum(l => l.Balance);

        return Ok(new PeriodEndClosingPreview
        {
            FromDate = from, ToDate = to,
            RetainedEarnings = re?.AcctCode ?? reCode, RetainedEarningsName = re?.AcctName,
            TotalRevenue = revenue, TotalExpense = expense, NetProfit = revenue - expense,
            AlreadyClosed = prior != null, ClosedBy = prior?.DocNum,
            Warnings = warnings, Lines = lines,
        });
    }

    [HttpPost]
    public IActionResult Create([FromBody] PeriodEndClosingRequest r)
    {
        if (r.FromDate == default || r.ToDate == default) return BadRequest("Enter the first and last day of the year to close.");
        if (r.FromDate > r.ToDate) return BadRequest("The start date must be on or before the end date.");
        var postingDate = r.PostingDate == default ? r.ToDate : r.PostingDate;
        if (postingDate < r.FromDate) return BadRequest("The posting date must fall on or after the start of the period.");

        if (_repository.GetAll().Any(x => x.Status == "P" && x.FromDate <= r.ToDate && x.ToDate >= r.FromDate))
            return BadRequest("This period has already been closed. Void the existing run before closing it again.");

        var reCode = string.IsNullOrWhiteSpace(r.RetainedEarnings)
            ? _db.Set<Gld>().AsNoTracking().FirstOrDefault()?.RetainedEarnings
            : r.RetainedEarnings.Trim();
        if (string.IsNullOrWhiteSpace(reCode))
            return BadRequest("No retained earnings account set. Choose one, or set it in GL Account Determination.");
        var re = _db.Set<Coa>().AsNoTracking().FirstOrDefault(a => a.AcctCode == reCode);
        if (re == null) return BadRequest($"Retained earnings account '{reCode}' is not in the chart of accounts.");
        if (!re.AllowPosting) return BadRequest($"Retained earnings account {re.AcctCode} does not allow posting.");

        var lines = BuildLines(r.FromDate, r.ToDate, re.AcctCode, out var warnings);
        if (lines.Count == 0) return BadRequest("No profit-and-loss account has a balance in this period — there is nothing to close.");
        var blocking = warnings.Where(w => w.StartsWith("Account ")).ToList();
        if (blocking.Count > 0) return BadRequest(string.Join(" ", blocking));

        var revenue = lines.Where(l => l.AccountType == "I").Sum(l => -l.Balance);
        var expense = lines.Where(l => l.AccountType == "X").Sum(l => l.Balance);
        var docNum  = GenerateDocNum(r.ToDate);
        var memo    = string.IsNullOrWhiteSpace(r.Remarks)
            ? $"Year-end closing {docNum} — {r.FromDate:yyyy-MM-dd} to {r.ToDate:yyyy-MM-dd}"
            : r.Remarks.Trim();

        var names = _db.Set<Coa>().AsNoTracking()
            .Where(a => a.AcctCode == re.AcctCode || lines.Select(l => l.ClosedTo).Contains(a.AcctCode))
            .ToDictionary(a => a.AcctCode, a => a.AcctName);

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var je = new Je
            {
                JrnlNo = JeNumber.Next(_db, postingDate),
                TransDate = postingDate, DueDate = postingDate, DocDate = postingDate,
                RefNo = docNum, Memo = memo,
                TransType = "KPEC", BaseDocType = "KPEC",
                Currency = "USD", ExchangeRate = 1, Status = "O",
                IsAutoGenerated = true, VersionNum = 1,
                CreatedAt = DateTime.UtcNow, InActive = false,
            };

            int n = 1;
            // Each P&L account is written back to zero: a credit balance is debited, a debit balance credited.
            foreach (var l in lines)
            {
                var dr = l.Balance < 0 ? -l.Balance : 0;
                var cr = l.Balance > 0 ? l.Balance : 0;
                je.Lines.Add(JeLine(n++, l.AccountCode, l.AccountName, dr, cr, $"Closing {l.AccountCode} — {docNum}"));
            }
            // The balancing side: profit credits retained earnings, a loss debits it.
            // Accounts with their own ClosingAcc go there instead.
            foreach (var g in lines.GroupBy(l => l.ClosedTo))
            {
                var net = g.Sum(l => l.Balance);        // debit-positive
                if (Math.Abs(net) < 0.005m) continue;
                var dr = net > 0 ? net : 0;             // net expense → debit the closing account
                var cr = net < 0 ? -net : 0;            // net profit  → credit it
                je.Lines.Add(JeLine(n++, g.Key, names.GetValueOrDefault(g.Key, "Retained earnings"), dr, cr, memo));
            }

            var totalDr = je.Lines.Sum(l => l.Debit);
            var totalCr = je.Lines.Sum(l => l.Credit);
            if (Math.Abs(totalDr - totalCr) > 0.01m)
                return BadRequest($"The closing entry would be unbalanced: Dr {totalDr:N2} Cr {totalCr:N2}.");
            je.TotalDebit = totalDr; je.TotalCredit = totalCr; je.TotalLC = totalDr; je.TotalFC = totalDr;

            _db.Set<Je>().Add(je);
            _db.SaveChanges();

            var run = new PeriodEndClosingRun
            {
                DocNum = docNum, FromDate = r.FromDate, ToDate = r.ToDate, PostingDate = postingDate,
                RetainedEarnings = re.AcctCode, TotalRevenue = revenue, TotalExpense = expense,
                NetProfit = revenue - expense, AccountCount = lines.Count, Remarks = r.Remarks?.Trim(),
                Status = "P", JournalEntryId = je.Id, JournalNo = je.JrnlNo,
                CreatedAt = DateTime.UtcNow, InActive = false,
            };
            foreach (var l in lines)
                run.Lines.Add(new PeriodEndClosingLine
                {
                    AccountCode = l.AccountCode, AccountName = l.AccountName, AccountType = l.AccountType,
                    Balance = l.Balance, ClosedTo = l.ClosedTo, CreatedAt = DateTime.UtcNow, InActive = false,
                });
            _repository.Add(run);
            _db.SaveChanges();

            je.BaseDocEntry = run.Id;
            _db.SaveChanges();
            tx.Commit();

            return Ok(new
            {
                message = $"Year closed as {docNum}: {lines.Count} account(s) cleared, net {(revenue - expense):N2} to {re.AcctCode}",
                id = run.Id, docNum, journalEntryId = je.Id, journalNo = je.JrnlNo,
            });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpPost("{id:int}/Void")]
    public IActionResult Void(int id)
    {
        var run = _repository.GetSingle(x => x.Id == id);
        if (run == null) return NotFound();
        if (run.Status == "V") return BadRequest($"Closing run {run.DocNum} is already void.");

        var je = run.JournalEntryId == null ? null : _db.Set<Je>().FirstOrDefault(j => j.Id == run.JournalEntryId);
        if (je != null && je.Status != "V") { je.Status = "V"; je.UpdatedAt = DateTime.UtcNow; }   // GLBalanceSync reverses it
        run.Status    = "V";
        run.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(new { message = $"Closing run {run.DocNum} voided — the period is open again" });
    }

    // ── HELPERS ──────────────────────────────────────────────────
    // Every P&L account with a balance in the period, from posted journal lines.
    // Balance is debit-positive: expenses come out positive, revenue negative.
    private List<PeriodEndClosingLineDto> BuildLines(DateTime from, DateTime to, string? retainedEarnings, out List<string> warnings)
    {
        warnings = new List<string>();
        var accounts = _db.Set<Coa>().AsNoTracking()
            .Where(a => a.AcctType == "I" || a.AcctType == "X")
            .ToDictionary(a => a.AcctCode, a => a);

        var codes = accounts.Keys.ToList();
        var sums = _db.Set<JeLineEntity>().AsNoTracking()
            .Where(l => codes.Contains(l.AccountCode))
            .Join(_db.Set<Je>().AsNoTracking().Where(j => j.Status != "V" && j.TransDate >= from && j.TransDate <= to),
                  l => l.JournalEntryId, j => j.Id, (l, j) => l)
            .GroupBy(l => l.AccountCode)
            .Select(g => new { Code = g.Key, Balance = g.Sum(x => x.Debit - x.Credit) })
            .ToList();

        var lines = new List<PeriodEndClosingLineDto>();
        foreach (var s in sums.OrderBy(s => s.Code))
        {
            if (Math.Abs(s.Balance) < 0.005m) continue;
            var a = accounts[s.Code];
            var closedTo = string.IsNullOrWhiteSpace(a.ClosingAcc) ? retainedEarnings : a.ClosingAcc.Trim();
            if (string.IsNullOrWhiteSpace(closedTo))
            {
                warnings.Add($"Account {a.AcctCode} {a.AcctName} has no account to close into.");
                continue;
            }
            if (!a.AllowPosting)
            {
                warnings.Add($"Account {a.AcctCode} {a.AcctName} has a balance but does not allow posting.");
                continue;
            }
            lines.Add(new PeriodEndClosingLineDto
            {
                AccountCode = a.AcctCode, AccountName = a.AcctName, AccountType = a.AcctType!,
                Balance = Math.Round(s.Balance, 2), ClosedTo = closedTo,
            });
        }
        return lines;
    }

    private static JeLineEntity JeLine(int n, string code, string name, decimal dr, decimal cr, string memo) => new()
    {
        LineNum = n, AccountCode = code, AccountName = name,
        Debit = dr, Credit = cr, DebitLC = dr, CreditLC = cr,
        LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
    };

    private string GenerateDocNum(DateTime date)
    {
        var prefix = $"PEC-{date.Year}-";
        var last = _repository.GetAll().Where(x => x.DocNum.StartsWith(prefix)).OrderByDescending(x => x.Id).FirstOrDefault();
        if (last == null) return $"{prefix}00001";
        var parts = last.DocNum.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n) ? $"{prefix}{n + 1:D5}" : $"{prefix}00001";
    }

    private static PeriodEndClosingResponse ToResponse(PeriodEndClosingRun r) => new()
    {
        Id = r.Id, DocNum = r.DocNum, FromDate = r.FromDate, ToDate = r.ToDate, PostingDate = r.PostingDate,
        RetainedEarnings = r.RetainedEarnings, TotalRevenue = r.TotalRevenue, TotalExpense = r.TotalExpense,
        NetProfit = r.NetProfit, AccountCount = r.AccountCount, Remarks = r.Remarks, Status = r.Status,
        JournalEntryId = r.JournalEntryId, JournalNo = r.JournalNo, CreatedAt = r.CreatedAt,
        Lines = r.Lines.OrderBy(l => l.AccountCode).Select(l => new PeriodEndClosingLineDto
        {
            AccountCode = l.AccountCode, AccountName = l.AccountName, AccountType = l.AccountType,
            Balance = l.Balance, ClosedTo = l.ClosedTo,
        }).ToList(),
    };
}

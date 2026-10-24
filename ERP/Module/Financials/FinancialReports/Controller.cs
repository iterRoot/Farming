using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetDoc     = FarmingApi.Modules.Financials.Budget.Budget;
using BudgetLineDoc = FarmingApi.Modules.Financials.Budget.BudgetLine;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;

namespace FarmingApi.Modules.Financials.FinancialReports;

// ═══════════════════════════════════════════════════════════════
// FINANCIAL REPORTS  (route: /FinancialReport)
// GET TrialBalance?from&to[&compareTo][&includeZero]
// GET BalanceSheet?asOf[&compareAsOf][&includeZero]
// GET ProfitAndLoss?from&to[&compareFrom&compareTo][&includeZero]
// GET CashFlow?from&to
// GET Ledger?account&from&to          → drill-down: every entry line behind an account's figure
// GET BudgetVsActual?year[&type=PL|BS|ALL]
// All figures come from posted (non-void) journal entries.
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("FinancialReport")]
public class FinancialReportController : ControllerBase
{
    private readonly MyDbContext _db;
    public FinancialReportController(MyDbContext db) => _db = db;

    private static decimal R(decimal v) => Math.Round(v, 2);

    private string? CheckRange(DateTime from, DateTime to) =>
        from.Year < 1900 || to.Year < 1900 ? "Give both a start date and an end date." :
        to.Date < from.Date ? "The end date is before the start date." : null;

    // ── shared statement building ───────────────────────────────
    public class Row
    {
        public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string Type { get; set; } = "";
        public int Level { get; set; } public bool IsGroup { get; set; }
        public decimal Amount { get; set; } public decimal? Compare { get; set; }
        public decimal? Variance => Compare == null ? null : R(Amount - Compare.Value);
        public decimal? VariancePct => Compare == null || Compare == 0 ? null : R((Amount - Compare.Value) / Math.Abs(Compare.Value) * 100);
    }

    // Rows for the accounts of the given types, natural sign applied (sign −1 turns credit balances positive).
    private List<Row> RowsFor(ReportEngine e, string[] types, int sign, Dictionary<string, decimal> main, Dictionary<string, decimal>? cmp, bool includeZero)
    {
        var mainRoll = e.Rollup(main); var cmpRoll = cmp == null ? null : e.Rollup(cmp);
        var rows = new List<Row>();
        foreach (var a in e.Accounts.Where(a => types.Contains(a.Type)))
        {
            var amt = R(sign * mainRoll.GetValueOrDefault(a.Code));
            decimal? c = cmpRoll == null ? null : R(sign * cmpRoll.GetValueOrDefault(a.Code));
            if (!includeZero && amt == 0 && (c ?? 0) == 0) continue;
            rows.Add(new Row { Code = a.Code, Name = a.Name, Type = a.Type, Level = a.Level, IsGroup = a.IsGroup, Amount = amt, Compare = c });
        }
        return rows;
    }

    // Sum of the posting accounts' figures for the given types (not the roll-up, so nothing counts twice).
    private static decimal Total(ReportEngine e, string[] types, Dictionary<string, decimal> m) =>
        R(e.Accounts.Where(a => types.Contains(a.Type)).Sum(a => m.GetValueOrDefault(a.Code)));

    private static Dictionary<string, decimal> Net(Dictionary<string, DrCr> s) => s.ToDictionary(k => k.Key, k => k.Value.Net);

    private static Dictionary<string, decimal> Add(Dictionary<string, decimal> a, Dictionary<string, decimal> b)
    {
        var r = new Dictionary<string, decimal>(a);
        foreach (var kv in b) r[kv.Key] = r.GetValueOrDefault(kv.Key) + kv.Value;
        return r;
    }

    private object Line(string key, string label, string kind, decimal total, decimal? cmp, List<Row>? rows = null) =>
        new { key, label, kind, total, compare = cmp, rows = rows ?? new List<Row>() };

    // ── trial balance ───────────────────────────────────────────
    [HttpGet("TrialBalance")]
    public IActionResult TrialBalance([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] DateTime? compareTo = null, [FromQuery] bool includeZero = false)
    {
        var err = CheckRange(from, to); if (err != null) return BadRequest(err);
        var e = new ReportEngine(_db);
        var before = Net(e.Sum(null, from.AddDays(-1)));
        var act = e.Sum(from, to);
        var debit = act.ToDictionary(k => k.Key, k => k.Value.Dr);
        var credit = act.ToDictionary(k => k.Key, k => k.Value.Cr);
        var closing = Add(before, act.ToDictionary(k => k.Key, k => k.Value.Net));
        Dictionary<string, decimal>? cmp = compareTo == null ? null : Net(e.Sum(null, compareTo));

        var rOpen = e.Rollup(before); var rDr = e.Rollup(debit); var rCr = e.Rollup(credit); var rCl = e.Rollup(closing);
        var rCmp = cmp == null ? null : e.Rollup(cmp);
        var rows = new List<object>();
        foreach (var a in e.Accounts.Where(a => a.Type != "M"))
        {
            var o = R(rOpen.GetValueOrDefault(a.Code)); var d = R(rDr.GetValueOrDefault(a.Code)); var c = R(rCr.GetValueOrDefault(a.Code)); var cl = R(rCl.GetValueOrDefault(a.Code));
            if (!includeZero && o == 0 && d == 0 && c == 0 && cl == 0) continue;
            rows.Add(new { code = a.Code, name = a.Name, type = a.Type, level = a.Level, isGroup = a.IsGroup, opening = o, debit = d, credit = c, closing = cl,
                           compareClosing = rCmp == null ? (decimal?)null : R(rCmp.GetValueOrDefault(a.Code)) });
        }
        var posting = e.Accounts.Where(a => a.Type != "M").ToList();   // own figures, so postings on title accounts count once
        var totals = new
        {
            opening = R(posting.Sum(a => before.GetValueOrDefault(a.Code))), debit = R(posting.Sum(a => debit.GetValueOrDefault(a.Code))),
            credit = R(posting.Sum(a => credit.GetValueOrDefault(a.Code))), closing = R(posting.Sum(a => closing.GetValueOrDefault(a.Code))),
            closingDebit = R(posting.Sum(a => Math.Max(0, closing.GetValueOrDefault(a.Code)))), closingCredit = R(posting.Sum(a => Math.Max(0, -closing.GetValueOrDefault(a.Code)))),
        };
        return Ok(new { from = from.Date, to = to.Date, compareTo = compareTo?.Date, rows, totals, balanced = totals.debit == totals.credit && totals.closing == 0 });
    }

    // ── balance sheet ───────────────────────────────────────────
    [HttpGet("BalanceSheet")]
    public IActionResult BalanceSheet([FromQuery] DateTime asOf, [FromQuery] DateTime? compareAsOf = null, [FromQuery] bool includeZero = false)
    {
        if (asOf.Year < 1900) return BadRequest("Give an as-of date.");
        var e = new ReportEngine(_db);
        var main = Net(e.Sum(null, asOf));
        var cmp = compareAsOf == null ? null : Net(e.Sum(null, compareAsOf));
        string[] pl = { "I", "C", "X", "N", "T" };

        decimal Earn(Dictionary<string, decimal> m) => R(-Total(e, pl, m));
        var earn = Earn(main); decimal? earnC = cmp == null ? null : Earn(cmp);

        var assets = RowsFor(e, new[] { "A" }, 1, main, cmp, includeZero);
        var liab = RowsFor(e, new[] { "L" }, -1, main, cmp, includeZero);
        var eq = RowsFor(e, new[] { "E" }, -1, main, cmp, includeZero);
        var ta = R(Total(e, new[] { "A" }, main)); var tl = R(-Total(e, new[] { "L" }, main)); var te = R(-Total(e, new[] { "E" }, main));
        decimal? taC = cmp == null ? null : R(Total(e, new[] { "A" }, cmp)); decimal? tlC = cmp == null ? null : R(-Total(e, new[] { "L" }, cmp)); decimal? teC = cmp == null ? null : R(-Total(e, new[] { "E" }, cmp));

        var lines = new List<object>
        {
            Line("assets", "Assets", "section", ta, taC, assets),
            Line("liabilities", "Liabilities", "section", tl, tlC, liab),
            Line("equity", "Equity", "section", te, teC, eq),
            Line("earnings", "Profit (loss) not yet closed to equity", "total", earn, earnC),
            Line("equityTotal", "Total equity including profit", "total", R(te + earn), cmp == null ? null : R(teC!.Value + earnC!.Value)),
            Line("liabEquity", "Total liabilities and equity", "total", R(tl + te + earn), cmp == null ? null : R(tlC!.Value + teC!.Value + earnC!.Value)),
        };
        var diff = R(ta - (tl + te + earn));
        return Ok(new { asOf = asOf.Date, compareAsOf = compareAsOf?.Date, lines, difference = diff, balanced = diff == 0 });
    }

    // ── profit and loss ─────────────────────────────────────────
    [HttpGet("ProfitAndLoss")]
    public IActionResult ProfitAndLoss([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] DateTime? compareFrom = null, [FromQuery] DateTime? compareTo = null, [FromQuery] bool includeZero = false)
    {
        var err = CheckRange(from, to); if (err != null) return BadRequest(err);
        if ((compareFrom == null) != (compareTo == null)) return BadRequest("Give both compare dates, or neither.");
        if (compareFrom != null) { err = CheckRange(compareFrom.Value, compareTo!.Value); if (err != null) return BadRequest(err); }
        var e = new ReportEngine(_db);
        var main = Net(e.Sum(from, to));
        var cmp = compareFrom == null ? null : Net(e.Sum(compareFrom, compareTo));

        (List<Row> rows, decimal total, decimal? c) Sec(string type, int sign) =>
            (RowsFor(e, new[] { type }, sign, main, cmp, includeZero), R(sign * Total(e, new[] { type }, main)), cmp == null ? null : R(sign * Total(e, new[] { type }, cmp)));
        var rev = Sec("I", -1); var cos = Sec("C", 1); var opx = Sec("X", 1); var non = Sec("N", -1); var tax = Sec("T", 1);
        decimal? Sub(decimal? a, decimal? b) => a == null || b == null ? null : R(a.Value - b.Value);
        decimal? Plus(decimal? a, decimal? b) => a == null || b == null ? null : R(a.Value + b.Value);

        var gross = R(rev.total - cos.total); var grossC = Sub(rev.c, cos.c);
        var op = R(gross - opx.total); var opC = Sub(grossC, opx.c);
        var pbt = R(op + non.total); var pbtC = Plus(opC, non.c);
        var net = R(pbt - tax.total); var netC = Sub(pbtC, tax.c);
        var lines = new List<object>
        {
            Line("revenue", "Revenue", "section", rev.total, rev.c, rev.rows),
            Line("cos", "Cost of sales", "section", cos.total, cos.c, cos.rows),
            Line("gross", "Gross profit", "total", gross, grossC),
            Line("opex", "Operating expenses", "section", opx.total, opx.c, opx.rows),
            Line("operating", "Operating profit", "total", op, opC),
            Line("nonop", "Non-operating income (expenses)", "section", non.total, non.c, non.rows),
            Line("pbt", "Profit before tax", "total", pbt, pbtC),
            Line("tax", "Taxation", "section", tax.total, tax.c, tax.rows),
            Line("net", "Net profit (loss)", "total", net, netC),
        };
        return Ok(new { from = from.Date, to = to.Date, compareFrom = compareFrom?.Date, compareTo = compareTo?.Date, lines });
    }

    // ── cash flow (direct, by the account on the other side of each cash movement) ──
    [HttpGet("CashFlow")]
    public IActionResult CashFlow([FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var err = CheckRange(from, to); if (err != null) return BadRequest(err);
        var e = new ReportEngine(_db);
        var cash = e.Accounts.Where(a => a.Cash).Select(a => a.Code).ToHashSet();
        if (cash.Count == 0) return BadRequest("No cash or bank accounts are flagged in the Chart of Accounts (Cash Account / Cash Box), so a cash flow can't be built.");

        var f = ReportEngine.Day(from); var t = ReportEngine.Day(to).AddDays(1);
        var lines = _db.Set<JournalEntryLine>().AsNoTracking().Where(l => l.JournalEntry.Status != "V" && l.JournalEntry.TransDate >= f && l.JournalEntry.TransDate < t
                && _db.Set<JournalEntryLine>().Any(c => c.JournalEntryId == l.JournalEntryId && cash.Contains(c.AccountCode)))
            .Select(l => new { l.JournalEntryId, l.AccountCode, l.Debit, l.Credit }).ToList();

        var byCounter = new Dictionary<string, decimal>(); decimal internalMoves = 0;
        foreach (var g in lines.GroupBy(l => l.JournalEntryId))
        {
            var cashNet = g.Where(l => cash.Contains(l.AccountCode)).Sum(l => l.Debit - l.Credit);
            if (cashNet == 0) { internalMoves += g.Where(l => cash.Contains(l.AccountCode)).Sum(l => l.Debit); continue; }
            foreach (var l in g.Where(l => !cash.Contains(l.AccountCode)))
                byCounter[l.AccountCode] = byCounter.GetValueOrDefault(l.AccountCode) + (l.Credit - l.Debit);
        }

        var opening = R(Net(e.Sum(null, from.AddDays(-1))).Where(kv => cash.Contains(kv.Key)).Sum(kv => kv.Value));
        var closingBooks = R(Net(e.Sum(null, to)).Where(kv => cash.Contains(kv.Key)).Sum(kv => kv.Value));
        var by = e.ByCode;
        (string key, string label, string[] types)[] secs =
        {
            ("operating", "Operations — income, cost and expense accounts", new[] { "I", "C", "X", "N", "T" }),
            ("assets", "Other assets", new[] { "A" }), ("liabilities", "Liabilities", new[] { "L" }), ("equity", "Equity", new[] { "E" }),
        };
        var outLines = new List<object>(); decimal netChange = 0;
        foreach (var (key, label, types) in secs)
        {
            var rows = byCounter.Where(kv => by.TryGetValue(kv.Key, out var a) && types.Contains(a.Type) && R(kv.Value) != 0)
                .OrderBy(kv => kv.Key).Select(kv => new Row { Code = kv.Key, Name = by[kv.Key].Name, Type = by[kv.Key].Type, Amount = R(kv.Value) }).ToList();
            var tot = R(rows.Sum(r => r.Amount)); netChange += tot;
            outLines.Add(Line(key, label, "section", tot, null, rows));
        }
        var other = R(byCounter.Where(kv => !by.ContainsKey(kv.Key) || by[kv.Key].Type == "M").Sum(kv => kv.Value));
        if (other != 0) { netChange += other; outLines.Add(Line("other", "Other", "section", other, null)); }
        netChange = R(netChange);
        var closing = R(opening + netChange);
        return Ok(new { from = from.Date, to = to.Date, opening, netChange, closing, closingPerBooks = closingBooks, reconciles = closing == closingBooks,
                        transfersBetweenCashAccounts = R(internalMoves), lines = outLines });
    }

    // ── ledger drill-down ───────────────────────────────────────
    [HttpGet("Ledger")]
    public IActionResult Ledger([FromQuery] string account, [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var err = CheckRange(from, to); if (err != null) return BadRequest(err);
        var e = new ReportEngine(_db);
        if (!e.ByCode.TryGetValue((account ?? "").Trim(), out var a)) return NotFound($"Account '{account}' doesn't exist.");

        // A title account drills down into every posting account beneath it.
        var kids = e.Children(); var codes = new List<string>(); var stack = new Stack<string>(); stack.Push(a.Code);
        while (stack.Count > 0) { var c = stack.Pop(); codes.Add(c); if (kids.TryGetValue(c, out var ch)) foreach (var x in ch) stack.Push(x); }
        if (codes.Count == 0) return Ok(new { account = new { a.Code, a.Name, a.Type }, from = from.Date, to = to.Date, opening = 0m, closing = 0m, debit = 0m, credit = 0m, lines = new List<object>(), truncated = false });

        var f = ReportEngine.Day(from); var t = ReportEngine.Day(to).AddDays(1);
        var opening = R(_db.Set<JournalEntryLine>().Where(l => l.JournalEntry.Status != "V" && l.JournalEntry.TransDate < f && codes.Contains(l.AccountCode)).Sum(l => (decimal?)(l.Debit - l.Credit)) ?? 0m);
        var rows = _db.Set<JournalEntryLine>().AsNoTracking().Where(l => l.JournalEntry.Status != "V" && l.JournalEntry.TransDate >= f && l.JournalEntry.TransDate < t && codes.Contains(l.AccountCode))
            .OrderBy(l => l.JournalEntry.TransDate).ThenBy(l => l.JournalEntryId).ThenBy(l => l.LineNum).Take(5001)
            .Select(l => new { l.JournalEntryId, jrnlNo = l.JournalEntry.JrnlNo, date = l.JournalEntry.TransDate, refNo = l.JournalEntry.RefNo, memo = l.LineMemo ?? l.JournalEntry.Memo,
                               l.AccountCode, l.CardCode, l.Debit, l.Credit }).ToList();
        var truncated = rows.Count > 5000; if (truncated) rows = rows.Take(5000).ToList();
        var running = opening;
        var lines = rows.Select(r => { running += r.Debit - r.Credit; return new { r.JournalEntryId, r.jrnlNo, date = r.date, r.refNo, r.memo, account = r.AccountCode, r.CardCode, r.Debit, r.Credit, balance = R(running) }; }).ToList();
        return Ok(new { account = new { a.Code, a.Name, a.Type, a.IsGroup }, from = from.Date, to = to.Date, opening, debit = R(rows.Sum(r => r.Debit)), credit = R(rows.Sum(r => r.Credit)), closing = R(running), lines, truncated });
    }

    // ── budget vs actual ────────────────────────────────────────
    [HttpGet("BudgetVsActual")]
    public IActionResult BudgetVsActual([FromQuery] int year, [FromQuery] string type = "ALL")
    {
        if (year < 2000 || year > 2100) return BadRequest("Year must be between 2000 and 2100.");
        type = (type ?? "ALL").ToUpperInvariant();
        if (type is not ("PL" or "BS" or "ALL")) return BadRequest("type must be PL, BS or ALL.");
        var e = new ReportEngine(_db);
        var budgets = _db.Set<BudgetDoc>().Where(b => b.FiscalYear == year && (b.Status == "A" || b.Status == "C")).ToList();
        var lines = _db.Set<BudgetLineDoc>().Where(l => l.Budget.FiscalYear == year && (l.Budget.Status == "A" || l.Budget.Status == "C")).GroupBy(l => l.AcctCode)
            .Select(g => new { g.Key, Annual = g.Sum(x => x.Annual) }).ToDictionary(x => x.Key, x => x.Annual);

        var yStart = new DateTime(year, 1, 1); var yEnd = new DateTime(year, 12, 31);
        var act = e.Sum(yStart, yEnd); var netAct = Net(act); var closeAct = Net(e.Sum(null, yEnd));
        var by = e.ByCode; var rows = new List<object>();
        foreach (var a in e.Accounts.Where(a => a.Type != "M").OrderBy(a => a.Code))
        {
            var isPl = ReportEngine.IsProfitLoss(a.Type);
            if (type == "PL" && !isPl) continue; if (type == "BS" && isPl) continue;
            var budget = lines.GetValueOrDefault(a.Code);
            var raw = isPl ? netAct.GetValueOrDefault(a.Code) : closeAct.GetValueOrDefault(a.Code);
            var creditNormal = a.Type is "I" or "L" or "E" or "N";
            var actual = R(creditNormal ? -raw : raw);
            if (budget == 0 && actual == 0) continue;
            // favourable = ahead of budget: more income/equity, or less cost
            var variance = R(a.Type is "I" or "E" or "L" or "N" ? actual - budget : a.Type == "A" ? actual - budget : budget - actual);
            rows.Add(new { code = a.Code, name = a.Name, type = a.Type, level = a.Level, budget = R(budget), actual, variance, variancePct = budget == 0 ? (decimal?)null : R(variance / Math.Abs(budget) * 100) });
        }
        return Ok(new { year, type, budgetsUsed = budgets.Select(b => new { b.BudgetNo, b.BudgetName }).ToList(), rows, note = budgets.Count == 0 ? $"No approved budget exists for {year}." : null });
    }
}

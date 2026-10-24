using Microsoft.EntityFrameworkCore;
using CoaEntity = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;

namespace FarmingApi.Modules.Financials.FinancialReports;

public record Acct(string Code, string Name, string Type, bool IsGroup, string? Parent, int Level, bool Cash);
public record DrCr(decimal Dr, decimal Cr) { public decimal Net => Dr - Cr; }

// ═══════════════════════════════════════════════════════════════
// The numbers behind every financial statement: the chart of accounts, and debit / credit
// totals per account from posted journal entries (Void entries excluded), for a date window.
//   Type  A asset · L liability · E equity · I income · C cost of sales · X expense
//         N non-operating · T tax · M memo (left out of every statement)
// ═══════════════════════════════════════════════════════════════
public class ReportEngine
{
    private readonly MyDbContext _db;
    private List<Acct>? _accts;
    public ReportEngine(MyDbContext db) => _db = db;

    public static DateTime Day(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);

    public IReadOnlyList<Acct> Accounts => _accts ??= _db.Set<CoaEntity>().AsNoTracking().OrderBy(a => a.AcctCode)
        .Select(a => new Acct(a.AcctCode, a.AcctName, a.AcctType, a.IsGroup, a.FatherNum, a.Level, a.CashAccount || a.CashBox)).ToList();

    public Dictionary<string, Acct> ByCode => Accounts.ToDictionary(a => a.Code);

    /// <summary>Debit and credit per account for entries dated <paramref name="from"/> … <paramref name="to"/> inclusive (either may be open).</summary>
    public Dictionary<string, DrCr> Sum(DateTime? from, DateTime? to)
    {
        var f = from == null ? (DateTime?)null : Day(from.Value);
        var t = to == null ? (DateTime?)null : Day(to.Value).AddDays(1);
        return _db.Set<JournalEntryLine>().Where(l => l.JournalEntry.Status != "V"
                && (f == null || l.JournalEntry.TransDate >= f) && (t == null || l.JournalEntry.TransDate < t))
            .GroupBy(l => l.AccountCode)
            .Select(g => new { g.Key, Dr = g.Sum(x => x.Debit), Cr = g.Sum(x => x.Credit) })
            .ToDictionary(x => x.Key, x => new DrCr(x.Dr, x.Cr));
    }

    /// <summary>Children of each account (by FatherNum), for rolling posting accounts up into their titles.</summary>
    public Dictionary<string, List<string>> Children()
    {
        var d = new Dictionary<string, List<string>>();
        foreach (var a in Accounts.Where(a => !string.IsNullOrEmpty(a.Parent)))
        {
            if (!d.TryGetValue(a.Parent!, out var l)) d[a.Parent!] = l = new();
            l.Add(a.Code);
        }
        return d;
    }

    /// <summary>Rolls a per-account figure up the account tree (a title's figure = the sum of everything below it).</summary>
    public Dictionary<string, decimal> Rollup(Dictionary<string, decimal> posting)
    {
        var kids = Children(); var byCode = ByCode; var res = new Dictionary<string, decimal>();
        decimal Get(string code)
        {
            if (res.TryGetValue(code, out var v)) return v;
            // an account's own postings count even when it is a title (default accounts are sometimes title accounts)
            decimal sum = posting.GetValueOrDefault(code);
            if (kids.TryGetValue(code, out var ch)) foreach (var c in ch) sum += Get(c);
            return res[code] = sum;
        }
        foreach (var a in Accounts) Get(a.Code);
        return res;
    }

    public static bool IsProfitLoss(string type) => type is "I" or "C" or "X" or "N" or "T";
    public static bool IsBalanceSheet(string type) => type is "A" or "L" or "E";
}

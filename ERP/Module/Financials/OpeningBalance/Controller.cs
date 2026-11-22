using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KamnttraApi.Core;
using KamnttraApi.Modules.Financials.JournalEntry;
using Coa = KamnttraApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using Gld = KamnttraApi.Modules.Financials.GLAccountDetermination.GLAccountDetermination;
using BPEntity = KamnttraApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace KamnttraApi.Modules.Financials.OpeningBalance;

// ═══════════════════════════════════════════════════════════════════════════
// OPENING BALANCES
//
// Moves the closing balances of an old system (or paper books) into this one. Each
// call posts ONE journal entry (TransType "OB") dated at the opening date. Whatever
// the lines don't balance against goes to the Opening Balance account chosen in
// G/L Account Determination, so the books always balance and the offset is visible.
//
//   • /Accounts          — a debit or credit per G/L account.
//   • /BusinessPartners  — one balance per customer or vendor, posted to its control
//                          account with the partner's code on the line, so it shows in
//                          the G/L-and-Business-Partner report and the ledger.
//
// Stock opening balances are not a journal entry — enter them as a Goods Receipt.
// A posted opening entry is reversed like any journal entry (void it on the Journal
// Entry screen); the Chart of Accounts balances follow automatically.
// ═══════════════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class OpeningBalanceController : ControllerBase
{
    private const string Trans = "OB";
    private readonly MyDbContext _db;
    private readonly SecurityAudit _audit;
    public OpeningBalanceController(MyDbContext db, SecurityAudit audit) { _db = db; _audit = audit; }

    public class AccountLine { public string? AccountCode { get; set; } public decimal Debit { get; set; } public decimal Credit { get; set; } public string? Memo { get; set; } }
    public class AccountsRequest { public DateTime? Date { get; set; } public string? Memo { get; set; } public List<AccountLine> Lines { get; set; } = new(); }
    public class PartnerLine { public string? CardCode { get; set; } public decimal Balance { get; set; } }
    public class PartnersRequest { public DateTime? Date { get; set; } public string? Memo { get; set; } public List<PartnerLine> Lines { get; set; } = new(); }

    // ── GET /OpeningBalance ───────────────────────────────────────
    // The opening entries posted so far, and whether the offset account is set up.
    [HttpGet]
    public IActionResult List()
    {
        var gl = _db.Set<Gld>().AsNoTracking().FirstOrDefault();
        var entries = _db.Set<JournalEntry.JournalEntry>().AsNoTracking()
            .Where(j => j.TransType == Trans)
            .OrderByDescending(j => j.Id)
            .Select(j => new { j.Id, j.JrnlNo, j.TransDate, j.Memo, j.Status, j.TotalDebit, j.TotalCredit, j.BaseDocType })
            .ToList();
        return Ok(new { offsetAccount = gl?.OpeningBalance, entries });
    }

    // ── POST /OpeningBalance/Accounts ─────────────────────────────
    [HttpPost("Accounts")]
    public IActionResult PostAccounts([FromBody] AccountsRequest req)
    {
        if (req.Lines == null || req.Lines.Count == 0) return BadRequest("Add at least one account line.");
        var date = req.Date ?? throw new DocumentRuleException("The opening balance date is required.");
        var offset = OffsetAccount();

        var codes = req.Lines.Select(l => (l.AccountCode ?? "").Trim()).ToList();
        var accounts = _db.Set<Coa>().AsNoTracking().Where(a => codes.Contains(a.AcctCode)).ToDictionary(a => a.AcctCode);

        var lines = new List<JournalEntryLine>();
        var seen = new HashSet<string>();
        for (var i = 0; i < req.Lines.Count; i++)
        {
            var l = req.Lines[i]; var code = codes[i]; var row = i + 1;
            if (code == "") throw new DocumentRuleException($"Line {row}: choose an account.");
            if (!seen.Add(code)) throw new DocumentRuleException($"Line {row}: account {code} appears twice. Combine them into one line.");
            if (!accounts.TryGetValue(code, out var a)) throw new DocumentRuleException($"Line {row}: account {code} doesn't exist.");
            if (a.IsGroup || !a.AllowPosting || !a.IsActive) throw new DocumentRuleException($"Line {row}: {code} {a.AcctName} can't be posted to (title, inactive or posting not allowed).");
            if (code == offset.AcctCode) throw new DocumentRuleException($"Line {row}: {code} is the Opening Balance account itself — it receives the difference automatically.");
            if (l.Debit < 0 || l.Credit < 0) throw new DocumentRuleException($"Line {row}: amounts can't be negative.");
            if ((l.Debit > 0) == (l.Credit > 0)) throw new DocumentRuleException($"Line {row}: enter either a debit or a credit, not both and not neither.");
            lines.Add(Line(a.AcctCode, a.AcctName, l.Debit, l.Credit, null, null, l.Memo));
        }
        return Post(date, req.Memo ?? "Opening balances — accounts", lines, offset);
    }

    // ── POST /OpeningBalance/BusinessPartners ─────────────────────
    // balance > 0: a customer owes us / we owe a vendor. balance < 0: the opposite (a credit balance).
    [HttpPost("BusinessPartners")]
    public IActionResult PostPartners([FromBody] PartnersRequest req)
    {
        if (req.Lines == null || req.Lines.Count == 0) return BadRequest("Add at least one business partner.");
        var date = req.Date ?? throw new DocumentRuleException("The opening balance date is required.");
        var offset = OffsetAccount();
        var gl = _db.Set<Gld>().AsNoTracking().First();

        var codes = req.Lines.Select(l => (l.CardCode ?? "").Trim()).ToList();
        var partners = _db.Set<BPEntity>().AsNoTracking().Where(b => codes.Contains(b.Code)).ToDictionary(b => b.Code);
        var coa = _db.Set<Coa>().AsNoTracking().ToDictionary(a => a.AcctCode);

        var lines = new List<JournalEntryLine>();
        var seen = new HashSet<string>();
        for (var i = 0; i < req.Lines.Count; i++)
        {
            var l = req.Lines[i]; var code = codes[i]; var row = i + 1;
            if (code == "") throw new DocumentRuleException($"Line {row}: choose a business partner.");
            if (!seen.Add(code)) throw new DocumentRuleException($"Line {row}: {code} appears twice. Combine them into one line.");
            if (!partners.TryGetValue(code, out var bp)) throw new DocumentRuleException($"Line {row}: business partner {code} doesn't exist.");
            if (l.Balance == 0) throw new DocumentRuleException($"Line {row}: {code} has a zero balance — remove the line.");

            var customer = bp.TypeStatus == 0;
            if (!customer && bp.TypeStatus != 1) throw new DocumentRuleException($"Line {row}: {code} is a lead, not a customer or vendor.");
            var control = customer
                ? FirstNonEmpty(bp.ARControlAccount, gl.ARControlAccount, gl.AccountsReceivable)
                : FirstNonEmpty(gl.AccountsPayable);
            if (control == null) throw new DocumentRuleException(customer
                ? "Set the Accounts Receivable account in G/L Account Determination first."
                : "Set the Accounts Payable account in G/L Account Determination first.");
            if (!coa.TryGetValue(control, out var acct)) throw new DocumentRuleException($"The control account {control} for {code} isn't in the Chart of Accounts.");

            var amount = Math.Abs(l.Balance);
            // customer: positive = receivable = debit.  vendor: positive = payable = credit.
            var debit = customer == (l.Balance > 0);
            lines.Add(Line(acct.AcctCode, acct.AcctName, debit ? amount : 0, debit ? 0 : amount, bp.Code, bp.CardName, $"Opening balance — {bp.CardName}"));
        }
        return Post(date, req.Memo ?? "Opening balances — business partners", lines, offset);
    }

    // ── helpers ───────────────────────────────────────────────────
    private IActionResult Post(DateTime date, string memo, List<JournalEntryLine> lines, Coa offset)
    {
        var dr = lines.Sum(l => l.Debit); var cr = lines.Sum(l => l.Credit);
        var diff = dr - cr;                       // what the offset account has to carry
        if (diff != 0)
            lines.Add(Line(offset.AcctCode, offset.AcctName, diff < 0 ? -diff : 0, diff < 0 ? 0 : diff, null, null, "Opening balance offset"));

        var totalDr = lines.Sum(l => l.Debit);
        var n = 1; foreach (var l in lines) l.LineNum = n++;

        var je = new JournalEntry.JournalEntry
        {
            JrnlNo = JournalEntryNumberService.Next(_db, date),
            TransDate = date, DueDate = date, DocDate = date,
            Memo = memo, TransType = Trans, BaseDocType = "OPENBAL",
            Currency = "USD", ExchangeRate = 1,
            TotalDebit = totalDr, TotalCredit = totalDr, TotalLC = totalDr, TotalFC = totalDr,
            Status = "O", IsAutoGenerated = false, VersionNum = 1, CreatedAt = DateTime.UtcNow, InActive = false,
        };
        foreach (var l in lines) je.Lines.Add(l);
        _db.Set<JournalEntry.JournalEntry>().Add(je);
        _db.SaveChanges();
        _audit.Changed("opening-balance.post", je.JrnlNo, $"{lines.Count} lines, {totalDr:0.00}, offset {(diff == 0 ? "none" : $"{offset.AcctCode} {Math.Abs(diff):0.00}")}");

        return Ok(new { id = je.Id, jrnlNo = je.JrnlNo, totalDebit = totalDr, totalCredit = totalDr,
                        offset = diff == 0 ? null : new { account = offset.AcctCode, amount = Math.Abs(diff), side = diff < 0 ? "debit" : "credit" } });
    }

    private Coa OffsetAccount()
    {
        var code = _db.Set<Gld>().AsNoTracking().Select(g => g.OpeningBalance).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(code))
            throw new DocumentRuleException("Choose the Opening Balance account in G/L Account Determination first — it receives any difference between debits and credits.");
        var a = _db.Set<Coa>().AsNoTracking().FirstOrDefault(x => x.AcctCode == code)
            ?? throw new DocumentRuleException($"The Opening Balance account {code} isn't in the Chart of Accounts.");
        if (a.IsGroup || !a.AllowPosting || !a.IsActive)
            throw new DocumentRuleException($"The Opening Balance account {code} {a.AcctName} can't be posted to (title, inactive or posting not allowed). Choose a postable account in G/L Account Determination.");
        return a;
    }

    private static JournalEntryLine Line(string code, string name, decimal debit, decimal credit, string? card, string? cardName, string? memo) => new()
    {
        AccountCode = code, AccountName = name, CardCode = card, CardName = cardName,
        Debit = debit, Credit = credit, DebitLC = debit, CreditLC = credit, DebitFC = 0, CreditFC = 0,
        LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
    };

    private static string? FirstNonEmpty(params string?[] v) => v.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
}

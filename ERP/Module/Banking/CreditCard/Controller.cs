using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.ChartOfAccounts;
using FarmingApi.Modules.Financials.JournalEntry;
using IPay = FarmingApi.Modules.Banking.IncomingPayment.IncomingPayment;

namespace FarmingApi.Modules.Banking.CreditCard;

// ═══════════════════════════════════════════════════════════════════
// CREDIT CARD SETUP  (route: /CreditCard)
// GET /CreditCard · GET /CreditCard/{id} · POST · PUT /{id} · DELETE /{id}
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("CreditCard")]
public class CreditCardController : ControllerBase
{
    private readonly MyDbContext _db;
    public CreditCardController(MyDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Gets()
    {
        var cards = _db.Set<CreditCardSetup>().AsNoTracking().OrderBy(c => c.Code).ToList();
        var waiting = CreditCardData.Unsettled(_db).ToList();
        return Ok(cards.Select(c =>
        {
            var mine = waiting.Where(p => p.BankAccount == c.ClearingAccount).ToList();
            var dto = ToDto(c);
            dto.UnsettledAmount = mine.Sum(p => p.DocTotal);
            dto.UnsettledCount  = mine.Count;
            return dto;
        }).ToList());
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var c = _db.Set<CreditCardSetup>().AsNoTracking().FirstOrDefault(x => x.Id == id);
        return c == null ? NotFound() : Ok(ToDto(c));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreditCardSetupDto r)
    {
        var err = Validate(r, null);
        if (err != null) return BadRequest(err);
        var c = new CreditCardSetup { CreatedAt = DateTime.UtcNow, InActive = false };
        Apply(c, r);
        _db.Set<CreditCardSetup>().Add(c);
        _db.SaveChanges();
        return Ok(new { message = $"Card {c.Code} created", id = c.Id });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CreditCardSetupDto r)
    {
        var c = _db.Set<CreditCardSetup>().FirstOrDefault(x => x.Id == id);
        if (c == null) return NotFound();
        var err = Validate(r, id);
        if (err != null) return BadRequest(err);
        Apply(c, r);
        c.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var c = _db.Set<CreditCardSetup>().FirstOrDefault(x => x.Id == id);
        if (c == null) return NotFound();
        if (_db.Set<CreditCardSettlement>().Any(s => s.CardCode == c.Code))
            return BadRequest($"Card {c.Code} has settlements. Mark it inactive instead.");
        _db.Set<CreditCardSetup>().Remove(c);
        _db.SaveChanges();
        return NoContent();
    }

    private string? Validate(CreditCardSetupDto r, int? id)
    {
        if (string.IsNullOrWhiteSpace(r.Code) || string.IsNullOrWhiteSpace(r.Name)) return "Enter a code and a name for the card.";
        var code = r.Code.Trim().ToUpperInvariant();
        if (_db.Set<CreditCardSetup>().Any(c => c.Code == code && c.Id != id)) return $"Card code {code} already exists.";
        if (r.FeePercent is < 0 or > 100) return "The fee % must be between 0 and 100.";
        if (string.IsNullOrWhiteSpace(r.ClearingAccount)) return "Choose the clearing account card receipts are posted to.";
        var err = CreditCardData.CheckAccount(_db, r.ClearingAccount, "Clearing")
               ?? (string.IsNullOrWhiteSpace(r.SettlementAccount) ? null : CreditCardData.CheckAccount(_db, r.SettlementAccount, "Settlement"))
               ?? (string.IsNullOrWhiteSpace(r.FeeAccount) ? null : CreditCardData.CheckAccount(_db, r.FeeAccount, "Fee"));
        if (err != null) return err;
        if (r.FeePercent > 0 && string.IsNullOrWhiteSpace(r.FeeAccount)) return "Choose the expense account for the card fee.";
        if (r.ClearingAccount.Trim() == r.SettlementAccount?.Trim()) return "The clearing and settlement accounts must be different.";
        return null;
    }

    private static void Apply(CreditCardSetup c, CreditCardSetupDto r)
    {
        c.Code              = r.Code.Trim().ToUpperInvariant();
        c.Name              = r.Name.Trim();
        c.ClearingAccount   = r.ClearingAccount.Trim();
        c.SettlementAccount = string.IsNullOrWhiteSpace(r.SettlementAccount) ? null : r.SettlementAccount.Trim();
        c.FeePercent        = r.FeePercent;
        c.FeeAccount        = string.IsNullOrWhiteSpace(r.FeeAccount) ? null : r.FeeAccount.Trim();
        c.IsActive          = r.IsActive;
    }

    private static CreditCardSetupDto ToDto(CreditCardSetup c) => new()
    {
        Id = c.Id, Code = c.Code, Name = c.Name, ClearingAccount = c.ClearingAccount, SettlementAccount = c.SettlementAccount,
        FeePercent = c.FeePercent, FeeAccount = c.FeeAccount, IsActive = c.IsActive,
    };
}

// ═══════════════════════════════════════════════════════════════════
// CREDIT CARD SETTLEMENT  (route: /CreditCardSettlement)
// GET  /CreditCardSettlement                     → all settlements
// GET  /CreditCardSettlement/Candidates?card=X   → card receipts waiting in X's clearing account
// POST /CreditCardSettlement                     → post the payout and mark receipts settled
// POST /CreditCardSettlement/{id}/Void           → void the journal entry, receipts wait again
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("CreditCardSettlement")]
public class CreditCardSettlementController : ControllerBase
{
    private readonly MyDbContext _db;
    public CreditCardSettlementController(MyDbContext db) => _db = db;

    [HttpGet]
    public IActionResult Gets()
    {
        var names = _db.Set<CreditCardSetup>().AsNoTracking().ToDictionary(c => c.Code, c => c.Name);
        var list = _db.Set<CreditCardSettlement>().AsNoTracking().Include(s => s.Lines)
            .OrderByDescending(s => s.PostingDate).ThenByDescending(s => s.Id).ToList();
        return Ok(list.Select(s => ToResponse(s, names.GetValueOrDefault(s.CardCode))).ToList());
    }

    [HttpGet("Candidates")]
    public IActionResult Candidates([FromQuery] string card)
    {
        var c = _db.Set<CreditCardSetup>().AsNoTracking().FirstOrDefault(x => x.Code == card);
        if (c == null) return NotFound($"Card {card} not found");
        return Ok(CreditCardData.Unsettled(_db).Where(p => p.BankAccount == c.ClearingAccount)
            .OrderBy(p => p.DocDate).ToList()
            .Select(p => new CreditCardCandidate
            {
                IncomingPaymentId = p.Id, DocNum = p.DocNum, DocDate = p.DocDate,
                CustomerCode = p.CardCode, CustomerName = p.CardName, Reference = p.Reference, Amount = p.DocTotal,
            }).ToList());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreditCardSettlementRequest r)
    {
        var card = _db.Set<CreditCardSetup>().AsNoTracking().FirstOrDefault(x => x.Code == r.CardCode);
        if (card == null) return BadRequest("Choose the card being settled.");
        if (r.PostingDate == default) return BadRequest("Enter the settlement date.");
        if (r.PaymentIds.Count == 0) return BadRequest("Select at least one card receipt to settle.");

        var payments = _db.Set<IPay>().Where(p => r.PaymentIds.Contains(p.Id)).ToList();
        var wrong = payments.Where(p => p.PaymentMethod != "Card" || p.Status == "V" || p.Deposited || p.BankAccount != card.ClearingAccount).ToList();
        if (payments.Count != r.PaymentIds.Distinct().Count() || wrong.Count > 0)
            return BadRequest("Some selected receipts are no longer waiting for this card (already settled, voided, or posted to another account). Refresh and select again.");

        var bankCode = string.IsNullOrWhiteSpace(r.BankAccount) ? card.SettlementAccount : r.BankAccount.Trim();
        if (string.IsNullOrWhiteSpace(bankCode)) return BadRequest("Choose the bank account the payout went into.");
        var err = CreditCardData.CheckAccount(_db, bankCode, "Bank");
        if (err != null) return BadRequest(err);

        var gross = payments.Sum(p => p.DocTotal);
        var fee = Math.Round(r.FeeAmount ?? gross * card.FeePercent / 100m, 2);
        if (fee < 0 || fee >= gross) return BadRequest("The fee must be at least zero and less than the amount settled.");
        if (fee > 0 && string.IsNullOrWhiteSpace(card.FeeAccount)) return BadRequest($"Card {card.Code} has no fee account. Set one in the card setup.");
        var net = gross - fee;

        var accounts = _db.Set<ChartOfAccounts>().AsNoTracking()
            .Where(a => a.AcctCode == bankCode || a.AcctCode == card.ClearingAccount || a.AcctCode == card.FeeAccount)
            .ToDictionary(a => a.AcctCode, a => a.AcctName);
        var docNum = CreditCardData.NextDocNum(_db, r.PostingDate);
        var memo = $"Card settlement {docNum} — {card.Name}";

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var je = new JournalEntry
            {
                JrnlNo = JournalEntryNumberService.Next(_db, r.PostingDate),
                TransDate = r.PostingDate, DueDate = r.PostingDate, DocDate = r.PostingDate,
                RefNo = docNum, Ref2 = r.Reference?.Trim(), Memo = memo,
                TransType = "KBCT", BaseDocType = "KBCT", Currency = "USD", ExchangeRate = 1,
                Status = "O", IsAutoGenerated = true, VersionNum = 1,
                TotalDebit = gross, TotalCredit = gross, TotalLC = gross, TotalFC = gross,
                CreatedAt = DateTime.UtcNow, InActive = false,
            };
            int n = 1;
            je.Lines.Add(Line(n++, bankCode, accounts.GetValueOrDefault(bankCode, "Bank"), net, 0, memo));
            if (fee > 0) je.Lines.Add(Line(n++, card.FeeAccount!, accounts.GetValueOrDefault(card.FeeAccount!, "Card fee"), fee, 0, $"Card fee — {docNum}"));
            je.Lines.Add(Line(n++, card.ClearingAccount, accounts.GetValueOrDefault(card.ClearingAccount, "Card clearing"), 0, gross, memo));
            _db.Set<JournalEntry>().Add(je);
            _db.SaveChanges();

            var s = new CreditCardSettlement
            {
                DocNum = docNum, CardCode = card.Code, PostingDate = r.PostingDate,
                BankAccount = bankCode, ClearingAccount = card.ClearingAccount, FeeAccount = fee > 0 ? card.FeeAccount : null,
                GrossAmount = gross, FeeAmount = fee, NetAmount = net, Reference = r.Reference?.Trim(),
                Status = "P", JournalEntryId = je.Id, JournalNo = je.JrnlNo, CreatedAt = DateTime.UtcNow, InActive = false,
            };
            foreach (var p in payments)
            {
                s.Lines.Add(new CreditCardSettlementLine
                {
                    IncomingPaymentId = p.Id, PaymentDocNum = p.DocNum, CustomerName = p.CardName, Amount = p.DocTotal,
                    CreatedAt = DateTime.UtcNow, InActive = false,
                });
                p.Deposited = true;   // settled — the same flag Deposit uses for cash and cheques
                p.UpdatedAt = DateTime.UtcNow;
            }
            _db.Set<CreditCardSettlement>().Add(s);
            _db.SaveChanges();
            je.BaseDocEntry = s.Id;
            _db.SaveChanges();
            tx.Commit();
            return Ok(new { message = $"Settlement {docNum} posted as {je.JrnlNo}: {net:N2} to the bank, {fee:N2} fee", id = s.Id, docNum, journalNo = je.JrnlNo });
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
        var s = _db.Set<CreditCardSettlement>().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (s == null) return NotFound();
        if (s.Status == "V") return BadRequest($"Settlement {s.DocNum} is already void.");

        var je = s.JournalEntryId == null ? null : _db.Set<JournalEntry>().FirstOrDefault(j => j.Id == s.JournalEntryId);
        if (je != null && je.Status != "V") { je.Status = "V"; je.UpdatedAt = DateTime.UtcNow; }   // GLBalanceSync reverses it
        var ids = s.Lines.Select(l => l.IncomingPaymentId).ToList();
        foreach (var p in _db.Set<IPay>().Where(p => ids.Contains(p.Id))) { p.Deposited = false; p.UpdatedAt = DateTime.UtcNow; }
        s.Status = "V";
        s.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(new { message = $"Settlement {s.DocNum} voided — its receipts are waiting to be settled again" });
    }

    private static JournalEntryLine Line(int n, string code, string name, decimal dr, decimal cr, string memo) => new()
    {
        LineNum = n, AccountCode = code, AccountName = name, Debit = dr, Credit = cr, DebitLC = dr, CreditLC = cr,
        LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
    };

    private static CreditCardSettlementResponse ToResponse(CreditCardSettlement s, string? cardName) => new()
    {
        Id = s.Id, DocNum = s.DocNum, CardCode = s.CardCode, CardName = cardName, PostingDate = s.PostingDate,
        BankAccount = s.BankAccount, ClearingAccount = s.ClearingAccount, GrossAmount = s.GrossAmount,
        FeeAmount = s.FeeAmount, NetAmount = s.NetAmount, FeeAccount = s.FeeAccount, Reference = s.Reference,
        Status = s.Status, JournalEntryId = s.JournalEntryId, JournalNo = s.JournalNo,
        Lines = s.Lines.Select(l => new CreditCardSettlementLineDto
        {
            IncomingPaymentId = l.IncomingPaymentId, PaymentDocNum = l.PaymentDocNum, CustomerName = l.CustomerName, Amount = l.Amount,
        }).ToList(),
    };
}

internal static class CreditCardData
{
    // Card receipts not yet settled.
    public static IQueryable<IPay> Unsettled(MyDbContext db) =>
        db.Set<IPay>().AsNoTracking().Where(p => p.PaymentMethod == "Card" && p.Status != "V" && !p.Deposited);

    public static string? CheckAccount(MyDbContext db, string code, string role)
    {
        var a = db.Set<ChartOfAccounts>().AsNoTracking().FirstOrDefault(x => x.AcctCode == code.Trim());
        if (a == null) return $"{role} account '{code.Trim()}' is not in the chart of accounts.";
        if (!a.AllowPosting) return $"{role} account {a.AcctCode} does not allow posting.";
        if (!a.IsActive) return $"{role} account {a.AcctCode} is inactive.";
        return null;
    }

    public static string NextDocNum(MyDbContext db, DateTime date)
    {
        var prefix = $"CS-{date.Year}-";
        var last = db.Set<CreditCardSettlement>().Where(s => s.DocNum.StartsWith(prefix)).OrderByDescending(s => s.Id).FirstOrDefault();
        if (last == null) return $"{prefix}00001";
        var parts = last.DocNum.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n) ? $"{prefix}{n + 1:D5}" : $"{prefix}00001";
    }
}

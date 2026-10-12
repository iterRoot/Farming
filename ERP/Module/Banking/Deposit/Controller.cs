using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.JournalEntry;
using IPay = FarmingApi.Modules.Banking.IncomingPayment.IncomingPayment;

namespace FarmingApi.Modules.Banking.Deposit;

// ═══════════════════════════════════════════════════════════════════
// DEPOSIT  (route: /Deposit)
// GET    /Deposit               → all deposits
// GET    /Deposit/Candidates    → undeposited Cash/Check incoming payments
// GET    /Deposit/{id}
// POST   /Deposit               → Draft, no GL impact
// PUT    /Deposit/{id}          → Draft only — change bank/date/selection
// POST   /Deposit/{id}/Post     → Dr real bank, Cr each holding account
// POST   /Deposit/{id}/Cancel   → Draft only — unlinks candidates, deletes
// DELETE /Deposit/{id}          → Draft only
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class DepositController : ControllerBase
{
    private readonly IDepositRepository _repository;
    private readonly IMapper            _mapper;
    private readonly MyDbContext        _db;

    public DepositController(IDepositRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<DepositResponse>>(list));
    }

    // Eligible = Cash/Check incoming payments, not Void, not already deposited.
    [HttpGet("Candidates")]
    public IActionResult Candidates()
    {
        var rows = _db.Set<IPay>()
            .Where(p => (p.PaymentMethod == "Cash" || p.PaymentMethod == "Check")
                     && p.Status != "V"
                     && !p.Deposited)
            .OrderBy(p => p.DocDate)
            .Select(p => new DepositCandidateResponse
            {
                IncomingPaymentId = p.Id,
                DocNum            = p.DocNum,
                DocDate           = p.DocDate,
                CardName          = p.CardName,
                PaymentMethod     = p.PaymentMethod,
                CheckNumber       = p.CheckNumber,
                HoldingAccount    = p.BankAccount ?? "",
                Amount            = p.DocTotal,
                Currency          = p.Currency,
            })
            .ToList();
        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<DepositResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] DepositRequest dto) => SaveDraft(null, dto);

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] DepositRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Draft") return BadRequest("Only a Draft deposit can be edited.");
        return SaveDraft(id, dto);
    }

    private IActionResult SaveDraft(int? id, DepositRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.BankAccount)) return BadRequest("Bank account is required.");
        var bank = _db.Set<FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts>()
            .FirstOrDefault(a => a.AcctCode == dto.BankAccount);
        if (bank == null) return BadRequest($"GL account '{dto.BankAccount}' does not exist.");

        var ids = (dto.IncomingPaymentIds ?? new()).Distinct().ToList();
        if (ids.Count == 0) return BadRequest("Select at least one payment to deposit.");

        var payments = _db.Set<IPay>().Where(p => ids.Contains(p.Id)).ToList();
        var missing  = ids.Except(payments.Select(p => p.Id)).ToList();
        if (missing.Count > 0) return BadRequest($"Payment(s) not found: {string.Join(", ", missing)}.");

        // Ignore this same draft's own current lines when checking "already deposited".
        var alreadyDeposited = payments.Where(p => p.Deposited).ToList();
        if (id != null)
        {
            var ownLineIds = _repository.GetSingle(x => x.Id == id)?.Lines.Select(l => l.IncomingPaymentId).ToHashSet()
                             ?? new HashSet<int>();
            alreadyDeposited = alreadyDeposited.Where(p => !ownLineIds.Contains(p.Id)).ToList();
        }
        if (alreadyDeposited.Count > 0)
            return BadRequest($"Already deposited: {string.Join(", ", alreadyDeposited.Select(p => p.DocNum))}.");

        var notEligible = payments.Where(p => p.PaymentMethod != "Cash" && p.PaymentMethod != "Check" || p.Status == "V").ToList();
        if (notEligible.Count > 0)
            return BadRequest($"Not eligible for deposit (must be Cash/Check, not Void): {string.Join(", ", notEligible.Select(p => p.DocNum))}.");

        var currencies = payments.Select(p => p.Currency).Distinct().ToList();
        if (currencies.Count > 1)
            return BadRequest($"Selected payments span multiple currencies ({string.Join(", ", currencies)}) — deposit them separately.");

        Deposit entity;
        if (id is int existingId)
        {
            entity = _repository.GetAll().Include(x => x.Lines).First(x => x.Id == existingId);
            entity.Lines.Clear();
            entity.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            entity = new Deposit { DocNum = NextNumber(), CreatedAt = DateTime.UtcNow, InActive = false };
            _repository.Add(entity);
        }

        entity.DepositDate = dto.DepositDate == default ? DateTime.UtcNow.Date : dto.DepositDate;
        entity.BankAccount = dto.BankAccount;
        entity.Memo        = dto.Memo;
        entity.Currency    = currencies.FirstOrDefault() ?? "USD";
        entity.TotalAmount = payments.Sum(p => p.DocTotal);

        var lineNum = 1;
        foreach (var p in payments)
        {
            entity.Lines.Add(new DepositLine
            {
                LineNum           = lineNum++,
                IncomingPaymentId = p.Id,
                PaymentDocNum     = p.DocNum,
                CardName          = p.CardName,
                HoldingAccount    = string.IsNullOrWhiteSpace(p.BankAccount) ? "(unset)" : p.BankAccount,
                Amount            = p.DocTotal,
            });
        }

        _repository.Commit();
        return Ok(_mapper.Map<DepositResponse>(ReloadWithLines(entity.Id)));
    }

    [HttpPost("{id:int}/Post")]
    public IActionResult Post(int id)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Draft") return BadRequest($"Deposit is {entity.Status}, only a Draft can be posted.");
        if (entity.Lines.Count == 0) return BadRequest("Deposit has no lines.");

        var paymentIds = entity.Lines.Select(l => l.IncomingPaymentId).ToList();
        var payments   = _db.Set<IPay>().Where(p => paymentIds.Contains(p.Id)).ToList();
        var reDeposited = payments.Where(p => p.Deposited).ToList();
        if (reDeposited.Count > 0)
            return BadRequest($"Already deposited elsewhere since this draft was saved: {string.Join(", ", reDeposited.Select(p => p.DocNum))}. Refresh and re-select.");

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var je = new JournalEntry
            {
                JrnlNo          = JournalEntryNumberService.Next(_db, entity.DepositDate),
                TransDate       = entity.DepositDate,
                DueDate         = entity.DepositDate,
                DocDate         = entity.DepositDate,
                RefNo           = entity.DocNum,
                Memo            = $"Deposit {entity.DocNum}" + (string.IsNullOrWhiteSpace(entity.Memo) ? "" : $" — {entity.Memo}"),
                TransType       = "KDEP",
                BaseDocType     = "KDEP",
                BaseDocEntry    = entity.Id,
                Currency        = entity.Currency,
                ExchangeRate    = 1,
                Status          = "O",
                IsAutoGenerated = true,
                VersionNum      = 1,
                CreatedAt       = DateTime.UtcNow,
                InActive        = false,
            };

            int lineNum = 1;
            je.Lines.Add(new JournalEntryLine
            {
                LineNum     = lineNum++,
                AccountCode = entity.BankAccount,
                AccountName = "Bank",
                Debit       = entity.TotalAmount,
                Credit      = 0,
                DebitLC     = entity.TotalAmount,
                CreditLC    = 0,
                LineMemo    = $"Deposit — {entity.DocNum}",
                CreatedAt   = DateTime.UtcNow,
                InActive    = false,
            });

            foreach (var grp in entity.Lines.GroupBy(l => l.HoldingAccount))
            {
                je.Lines.Add(new JournalEntryLine
                {
                    LineNum     = lineNum++,
                    AccountCode = grp.Key,
                    AccountName = "Undeposited receipts",
                    Debit       = 0,
                    Credit      = grp.Sum(l => l.Amount),
                    DebitLC     = 0,
                    CreditLC    = grp.Sum(l => l.Amount),
                    LineMemo    = $"Deposited — {string.Join(", ", grp.Select(l => l.PaymentDocNum))}",
                    CreatedAt   = DateTime.UtcNow,
                    InActive    = false,
                });
            }

            var totalDr = je.Lines.Sum(l => l.Debit);
            var totalCr = je.Lines.Sum(l => l.Credit);
            if (Math.Abs(totalDr - totalCr) > 0.01m)
                return BadRequest($"Journal entry would be unbalanced: Dr={totalDr:N2} Cr={totalCr:N2}.");
            je.TotalDebit = totalDr; je.TotalCredit = totalCr; je.TotalLC = totalDr; je.TotalFC = totalDr;

            _db.Set<JournalEntry>().Add(je);
            _db.SaveChanges();

            foreach (var p in payments)
            {
                p.Deposited   = true;
                if (p.PaymentMethod == "Check") p.CheckStatus = "Deposited";
                p.UpdatedAt   = DateTime.UtcNow;
            }

            entity.Status         = "Posted";
            entity.JournalEntryId = je.Id;
            entity.UpdatedAt      = DateTime.UtcNow;
            _db.SaveChanges();

            tx.Commit();
            return Ok(new { message = $"Deposit {entity.DocNum} posted.", journalEntryId = je.Id, journalNo = je.JrnlNo });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Draft") return BadRequest("Only a Draft deposit can be cancelled — a Posted deposit is final.");
        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"Deposit {entity.DocNum} cancelled." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Draft") return BadRequest("Only a Draft deposit can be deleted — a Posted deposit is final.");
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private string NextNumber()
    {
        var year   = DateTime.UtcNow.Year;
        var prefix = $"DEP-{year}-";
        var highest = _repository.GetAll()
            .Where(d => d.DocNum.StartsWith(prefix))
            .Select(d => d.DocNum)
            .ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0)
            .Max();
        return $"{prefix}{highest + 1:D5}";
    }

    private Deposit ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}

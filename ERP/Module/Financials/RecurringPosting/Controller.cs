using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using JE = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JELine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using COA = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;

namespace FarmingApi.Modules.Financials.RecurringPosting;

// ══════════════════════════════════════════════════════════════════
// RECURRING POSTINGS  (route: /RecurringPosting)
// GET    /RecurringPosting            → all templates
// GET    /RecurringPosting/Due        → templates due today or earlier
// GET    /RecurringPosting/{id}        → one
// POST   /RecurringPosting             → create template
// PUT    /RecurringPosting/{id}         → update
// DELETE /RecurringPosting/{id}         → delete
// POST   /RecurringPosting/{id}/Execute → generate a Journal Entry now
// ══════════════════════════════════════════════════════════════════
public class RecurringPostingController : MyController
{
    private static readonly string[] Frequencies =
        { "Daily", "Weekly", "Monthly", "Quarterly", "Annually" };

    private readonly IRecurringPostingRepository _repo;
    private readonly MyDbContext _db;
    private readonly IMapper _mapper;

    public RecurringPostingController(IRecurringPostingRepository repo, MyDbContext db, IMapper mapper)
    {
        _repo = repo; _db = db; _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var list = _repo.GetAll().Include(x => x.Lines).OrderByDescending(x => x.CreatedAt).ToList();
        return Ok(_mapper.Map<List<RecurringPostingResponse>>(list));
    }

    // Templates that are Active and due (NextExecution <= today).
    [AllowAnonymous]
    [HttpGet("Due")]
    public IActionResult GetDue()
    {
        var today = DateTime.UtcNow.Date;
        var list = _repo.GetAll().Include(x => x.Lines)
            .Where(x => x.Status == "Active" && x.NextExecution <= today)
            .OrderBy(x => x.NextExecution).ToList();
        return Ok(_mapper.Map<List<RecurringPostingResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        return e == null ? NotFound() : Ok(_mapper.Map<RecurringPostingResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] RecurringPostingRequest req)
    {
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        var e = _mapper.Map<RecurringPosting>(req);
        FillAccountNames(e);
        e.Code          = GenerateCode();
        e.NextExecution = req.StartDate.Date;
        e.CreatedAt     = DateTime.UtcNow;

        _repo.Add(e);
        _repo.Commit();
        return Ok(new { message = "Recurring posting created", id = e.Id, code = e.Code });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] RecurringPostingUpdateRequest req)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound($"Recurring posting {id} not found");

        var err = Validate(req);
        if (err != null) return BadRequest(err);

        var code = e.Code; var next = e.NextExecution; var times = e.TimesExecuted;
        _db.RemoveRange(e.Lines);
        _mapper.Map(req, e);
        FillAccountNames(e);
        e.Code = code; e.TimesExecuted = times;
        e.NextExecution = next < req.StartDate.Date ? req.StartDate.Date : next;
        e.UpdatedAt = DateTime.UtcNow;

        _repo.Update(e);
        _repo.Commit();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Recurring posting {id} not found");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    // ── Generate a Journal Entry from the template and advance the schedule ──
    [HttpPost("{id:int}/Execute")]
    public IActionResult Execute(int id)
    {
        var tpl = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (tpl == null) return NotFound($"Recurring posting {id} not found");
        if (tpl.Status != "Active") return BadRequest($"Template is {tpl.Status} — only Active templates can be executed.");
        if (tpl.Lines.Count == 0) return BadRequest("Template has no lines.");

        var when = DateTime.UtcNow.Date;
        var je = new JE
        {
            JrnlNo          = GenerateJournalNumber(),
            TransDate       = when, DueDate = when, DocDate = when,
            RefNo           = tpl.Code,
            TransType       = "JE",
            BaseDocType     = "RECUR",
            BaseDocEntry    = tpl.Id,
            Currency        = tpl.Currency,
            ExchangeRate    = 1,
            Memo            = $"Recurring: {tpl.Description}",
            Status          = "O",
            IsAutoGenerated = true,
            TotalDebit      = tpl.Lines.Sum(l => l.Debit),
            TotalCredit     = tpl.Lines.Sum(l => l.Credit),
            CreatedAt       = DateTime.UtcNow,
        };
        je.TotalLC = je.TotalDebit;
        int n = 1;
        foreach (var l in tpl.Lines.OrderBy(l => l.LineNum))
        {
            je.Lines.Add(new JELine
            {
                LineNum = n++, AccountCode = l.AccountCode, AccountName = l.AccountName,
                Debit = l.Debit, Credit = l.Credit, DebitLC = l.Debit, CreditLC = l.Credit,
                LineMemo = l.LineMemo, CreatedAt = DateTime.UtcNow,
            });
        }

        _db.Set<JE>().Add(je);

        // Advance schedule
        tpl.NextExecution = Advance(tpl.NextExecution < when ? when : tpl.NextExecution, tpl.Frequency);
        tpl.TimesExecuted += 1;
        if (tpl.ValidUntil.HasValue && tpl.NextExecution > tpl.ValidUntil.Value.Date)
            tpl.Status = "Expired";

        _db.SaveChanges();
        return Ok(new { message = "Journal Entry generated", journalEntryId = je.Id, jrnlNo = je.JrnlNo, nextExecution = tpl.NextExecution });
    }

    // ── Helpers ────────────────────────────────────────────────────
    private string? Validate(RecurringPostingRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Description)) return "Description is required";
        if (!Frequencies.Contains(req.Frequency))
            return $"Frequency must be one of: {string.Join(", ", Frequencies)}";
        if (req.Lines.Count == 0) return "At least one line is required";

        var totalD = req.Lines.Sum(l => l.Debit);
        var totalC = req.Lines.Sum(l => l.Credit);
        if (totalD == 0 && totalC == 0) return "Total amount cannot be zero";
        if (Math.Round(totalD, 2) != Math.Round(totalC, 2))
            return $"Debit ({totalD:N2}) must equal Credit ({totalC:N2})";

        foreach (var l in req.Lines)
        {
            if (string.IsNullOrWhiteSpace(l.AccountCode)) return "Every line needs an Account Code";
            if (l.Debit != 0 && l.Credit != 0) return $"Account {l.AccountCode}: a line cannot have both Debit and Credit";
            var acct = _db.Set<COA>().FirstOrDefault(a => a.AcctCode == l.AccountCode);
            if (acct == null) return $"Account '{l.AccountCode}' does not exist";
            if (acct.IsGroup) return $"Account '{l.AccountCode}' is a Title/Group — postings need a leaf account";
        }
        return null;
    }

    private void FillAccountNames(RecurringPosting e)
    {
        int n = 1;
        foreach (var l in e.Lines)
        {
            l.AccountName = _db.Set<COA>().FirstOrDefault(a => a.AcctCode == l.AccountCode)?.AcctName ?? "";
            l.LineNum = n++;
        }
    }

    private static DateTime Advance(DateTime from, string freq) => freq switch
    {
        "Daily"     => from.AddDays(1),
        "Weekly"    => from.AddDays(7),
        "Quarterly" => from.AddMonths(3),
        "Annually"  => from.AddYears(1),
        _            => from.AddMonths(1), // Monthly
    };

    private string GenerateCode()
    {
        var prefix = $"RCR-{DateTime.Now.Year}-";
        var last = _repo.GetAll().Where(x => x.Code.StartsWith(prefix))
            .OrderByDescending(x => x.Id).FirstOrDefault();
        var next = 1;
        if (last != null)
        {
            var p = last.Code.Split('-');
            if (p.Length >= 3 && int.TryParse(p[2], out var num)) next = num + 1;
        }
        return $"{prefix}{next:D8}";
    }

    // Matches the Journal Entry module's own numbering (JE-YYYY-00001).
    private string GenerateJournalNumber()
    {
        var prefix = $"JE-{DateTime.Now.Year}-";
        var last = _db.Set<JE>().Where(j => j.JrnlNo.StartsWith(prefix))
            .OrderByDescending(j => j.Id).FirstOrDefault();
        if (last == null) return $"{prefix}00001";
        var p = last.JrnlNo.Split('-');
        return (p.Length >= 3 && int.TryParse(p[2], out var num)) ? $"{prefix}{(num + 1):D5}" : $"{prefix}00001";
    }
}

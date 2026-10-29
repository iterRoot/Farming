using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.ChartOfAccounts;
using FarmingApi.Modules.Financials.JournalEntry;

namespace FarmingApi.Modules.Banking.BankStatement;

// ═══════════════════════════════════════════════════════════════════
// BANK STATEMENT  (route: /BankStatement)
// GET    /BankStatement                              → all statements (?accountCode, ?status)
// GET    /BankStatement/{id}
// POST   /BankStatement                              → header + rows, no GL impact
// PUT    /BankStatement/{id}                         → Open only; matched/posted rows are kept
// DELETE /BankStatement/{id}                         → Open and nothing posted
// GET    /BankStatement/{id}/Lines/{lineId}/Candidates → journal entries this row could match
// POST   /BankStatement/{id}/Lines/{lineId}/Match    → link a row to an existing journal entry
// POST   /BankStatement/{id}/Lines/{lineId}/Unmatch
// POST   /BankStatement/{id}/Lines/{lineId}/Post     → new JE: bank vs an offset account
// POST   /BankStatement/{id}/AutoMatch               → match rows with exactly one clear candidate
// POST   /BankStatement/{id}/Close                   → every row matched/posted and balances agree
// POST   /BankStatement/{id}/Reopen
// ═══════════════════════════════════════════════════════════════════
public class BankStatementController : MyController
{
    private const int CandidateWindowDays = 30;

    private readonly IBankStatementRepository _repository;
    private readonly MyDbContext              _db;

    public BankStatementController(IBankStatementRepository repository, MyDbContext db)
    {
        _repository = repository;
        _db         = db;
    }

    [HttpGet]
    public IActionResult Gets([FromQuery] string? accountCode = null, [FromQuery] string? status = null)
    {
        var query = _repository.GetAll().Include(b => b.Lines).AsQueryable();
        if (!string.IsNullOrEmpty(accountCode)) query = query.Where(b => b.AccountCode == accountCode);
        if (!string.IsNullOrEmpty(status))      query = query.Where(b => b.Status == status);

        return Ok(query.OrderByDescending(b => b.StatementDate).ThenByDescending(b => b.Id)
            .ToList().Select(ToResponse).ToList());
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var bs = Load(id);
        return bs == null ? NotFound($"Bank Statement {id} not found") : Ok(ToResponse(bs));
    }

    [HttpPost]
    public IActionResult Create([FromBody] BankStatementRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountCode)) return BadRequest("Choose the bank account.");
        var acct = _db.Set<ChartOfAccounts>().FirstOrDefault(a => a.AcctCode == request.AccountCode);
        if (acct == null) return BadRequest($"Account '{request.AccountCode}' not found in Chart of Accounts.");

        var error = ValidateHeader(request) ?? ValidateLines(request.Lines);
        if (error != null) return BadRequest(error);

        var entity = new BankStatement
        {
            StatementNo   = GenerateStatementNo(),
            AccountCode   = acct.AcctCode,
            AccountName   = acct.AcctName,
            Currency      = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency,
            Status        = "O",
            CreatedAt     = DateTime.UtcNow,
            InActive      = false,
        };
        ApplyHeader(entity, request);

        foreach (var lr in request.Lines) entity.Lines.Add(NewLine(lr));
        Renumber(entity);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(new { message = "Bank Statement created", id = entity.Id, statementNo = entity.StatementNo });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BankStatementRequest request)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Cannot edit a Closed bank statement. Reopen it first.");

        var error = ValidateHeader(request) ?? ValidateLines(request.Lines);
        if (error != null) return BadRequest(error);

        // Matched and posted rows are tied to a journal entry: keep them exactly as they are.
        var keptIds = request.Lines.Where(l => l.Id != null).Select(l => l.Id!.Value).ToHashSet();
        var droppedLinked = bs.Lines.Where(l => l.Status != "U" && !keptIds.Contains(l.Id)).ToList();
        if (droppedLinked.Count > 0)
            return BadRequest($"Row {string.Join(", ", droppedLinked.Select(l => l.LineNum))} is matched or posted. Unmatch it before removing it.");

        ApplyHeader(bs, request);
        bs.UpdatedAt = DateTime.UtcNow;

        foreach (var line in bs.Lines.Where(l => l.Status == "U" && !keptIds.Contains(l.Id)).ToList())
            bs.Lines.Remove(line);

        foreach (var lr in request.Lines)
        {
            var existing = lr.Id == null ? null : bs.Lines.FirstOrDefault(l => l.Id == lr.Id);
            if (existing == null) { bs.Lines.Add(NewLine(lr)); continue; }
            if (existing.Status != "U") continue;

            existing.ValueDate   = lr.ValueDate;
            existing.Reference   = Trim(lr.Reference);
            existing.Description = Trim(lr.Description);
            existing.AmountIn    = lr.AmountIn;
            existing.AmountOut   = lr.AmountOut;
            existing.Remarks     = Trim(lr.Remarks);
            existing.UpdatedAt   = DateTime.UtcNow;
        }
        Renumber(bs);

        _repository.Update(bs);
        _repository.Commit();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Cannot delete a Closed bank statement.");
        if (bs.Lines.Any(l => l.Status == "P"))
            return BadRequest("Rows of this statement were posted as journal entries. Void those journal entries and unmatch the rows first.");

        bs.DeletedAt = DateTime.UtcNow;
        _repository.Remove(bs);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // MATCHING
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("{id:int}/Lines/{lineId:int}/Candidates")]
    public IActionResult Candidates(int id, int lineId)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        var line = bs.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null) return NotFound($"Row {lineId} not found");

        return Ok(CandidatesFor(line, BookMovements(bs)));
    }

    [HttpPost("{id:int}/Lines/{lineId:int}/Match")]
    public IActionResult Match(int id, int lineId, [FromBody] BankStatementMatchRequest request)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Cannot modify a Closed bank statement.");
        var line = bs.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null) return NotFound($"Row {lineId} not found");
        if (line.Status != "U") return BadRequest($"Row {line.LineNum} is already {(line.Status == "M" ? "matched" : "posted")}.");

        var movement = BookMovements(bs).FirstOrDefault(m => m.JournalEntryId == request.JournalEntryId);
        if (movement == null)
            return BadRequest("That journal entry does not move this bank account, is void, or is already matched to another row.");
        if (Math.Abs((line.AmountIn - line.AmountOut) - (movement.AmountIn - movement.AmountOut)) > 0.005m)
            return BadRequest($"Amounts differ: the row is {Net(line):N2}, journal entry {movement.JournalNo} is {movement.AmountIn - movement.AmountOut:N2}.");

        Link(line, "M", movement.JournalEntryId, movement.JournalNo);
        bs.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { message = $"Row {line.LineNum} matched to {movement.JournalNo}" });
    }

    [HttpPost("{id:int}/Lines/{lineId:int}/Unmatch")]
    public IActionResult Unmatch(int id, int lineId)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Cannot modify a Closed bank statement.");
        var line = bs.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null) return NotFound($"Row {lineId} not found");
        if (line.Status == "U") return BadRequest($"Row {line.LineNum} is not matched.");

        if (line.Status == "P")
        {
            // The posting is real GL: undo it through the journal entry, not here.
            var je = _db.Set<JournalEntry>().AsNoTracking().FirstOrDefault(j => j.Id == line.JournalEntryId);
            if (je != null && je.Status != "V")
                return BadRequest($"Row {line.LineNum} was posted as journal entry {je.JrnlNo}. Void that journal entry first.");
        }

        Link(line, "U", null, null);
        bs.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { message = $"Row {line.LineNum} unmatched" });
    }

    [HttpPost("{id:int}/AutoMatch")]
    public IActionResult AutoMatch(int id)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Cannot modify a Closed bank statement.");

        var movements = BookMovements(bs);
        int matched = 0;
        foreach (var line in bs.Lines.Where(l => l.Status == "U").OrderBy(l => l.LineNum))
        {
            var candidates = CandidatesFor(line, movements);
            var byReference = candidates.Where(c => c.ReferenceMatch).ToList();
            var pick = byReference.Count == 1 ? byReference[0]
                     : byReference.Count == 0 && candidates.Count == 1 ? candidates[0]
                     : null;   // none, or more than one equally good — leave it for the user
            if (pick == null) continue;

            Link(line, "M", pick.JournalEntryId, pick.JournalNo);
            movements.RemoveAll(m => m.JournalEntryId == pick.JournalEntryId);
            matched++;
        }

        if (matched > 0) { bs.UpdatedAt = DateTime.UtcNow; _repository.Commit(); }
        var left = bs.Lines.Count(l => l.Status == "U");
        return Ok(new { message = $"{matched} row(s) matched, {left} still unmatched", matched, unmatched = left });
    }

    // ═══════════════════════════════════════════════════════════════
    // POST — a row with nothing in the books (bank fee, interest)
    // money in : Dr bank / Cr offset     money out: Dr offset / Cr bank
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Lines/{lineId:int}/Post")]
    public IActionResult Post(int id, int lineId, [FromBody] BankStatementPostRequest request)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Cannot modify a Closed bank statement.");
        var line = bs.Lines.FirstOrDefault(l => l.Id == lineId);
        if (line == null) return NotFound($"Row {lineId} not found");
        if (line.Status != "U") return BadRequest($"Row {line.LineNum} is already {(line.Status == "M" ? "matched" : "posted")}.");

        var offset = _db.Set<ChartOfAccounts>().FirstOrDefault(a => a.AcctCode == request.OffsetAccountCode);
        if (offset == null) return BadRequest("Choose the offset account (for example bank charges or interest income).");
        if (!offset.AllowPosting) return BadRequest($"Account {offset.AcctCode} does not allow posting.");
        if (offset.AcctCode == bs.AccountCode) return BadRequest("The offset account cannot be the bank account itself.");

        var amount = Math.Abs(Net(line));
        var isIn   = Net(line) > 0;
        var date   = request.PostingDate == default ? line.ValueDate : request.PostingDate;
        var memo   = string.IsNullOrWhiteSpace(request.Memo)
            ? $"Bank statement {bs.StatementNo} row {line.LineNum}" + (string.IsNullOrWhiteSpace(line.Description) ? "" : $" — {line.Description}")
            : request.Memo.Trim();

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var je = new JournalEntry
            {
                JrnlNo          = JournalEntryNumberService.Next(_db, date),
                TransDate       = date,
                DueDate         = date,
                DocDate         = date,
                RefNo           = bs.StatementNo,
                Ref2            = line.Reference,
                Memo            = memo,
                TransType       = "KBST",
                BaseDocType     = "KBST",
                BaseDocEntry    = bs.Id,
                Currency        = bs.Currency,
                ExchangeRate    = 1,
                Status          = "O",
                IsAutoGenerated = true,
                VersionNum      = 1,
                TotalDebit      = amount,
                TotalCredit     = amount,
                TotalLC         = amount,
                TotalFC         = amount,
                CreatedAt       = DateTime.UtcNow,
                InActive        = false,
            };
            je.Lines.Add(JeLine(1, bs.AccountCode, bs.AccountName, isIn ? amount : 0, isIn ? 0 : amount, memo));
            je.Lines.Add(JeLine(2, offset.AcctCode, offset.AcctName, isIn ? 0 : amount, isIn ? amount : 0, memo));

            _db.Set<JournalEntry>().Add(je);
            _db.SaveChanges();

            Link(line, "P", je.Id, je.JrnlNo);
            bs.UpdatedAt = DateTime.UtcNow;
            _db.SaveChanges();

            tx.Commit();
            return Ok(new { message = $"Row {line.LineNum} posted as {je.JrnlNo}", journalEntryId = je.Id, journalNo = je.JrnlNo });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CLOSE / REOPEN
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Close")]
    public IActionResult Close(int id)
    {
        var bs = Load(id);
        if (bs == null) return NotFound();
        if (bs.Status == "C") return BadRequest("Already closed.");
        if (bs.Lines.Count == 0) return BadRequest("The statement has no rows.");

        var open = bs.Lines.Where(l => l.Status == "U").Select(l => l.LineNum).ToList();
        if (open.Count > 0)
            return BadRequest($"Cannot close — row {string.Join(", ", open)} is not matched or posted yet.");

        var r = ToResponse(bs);
        if (Math.Abs(r.Difference) > 0.01m)
            return BadRequest($"Cannot close — the rows add up to a closing balance of {r.ComputedClosing:N2}, but the statement says {bs.ClosingBalance:N2} (difference {r.Difference:N2}). Check for a missing or mistyped row.");

        bs.Status    = "C";
        bs.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { message = $"Bank Statement {bs.StatementNo} closed" });
    }

    [HttpPost("{id:int}/Reopen")]
    public IActionResult Reopen(int id)
    {
        var bs = _repository.GetSingle(b => b.Id == id);
        if (bs == null) return NotFound();
        if (bs.Status != "C") return BadRequest("Only a Closed bank statement can be reopened.");

        bs.Status    = "O";
        bs.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(new { message = $"Bank Statement {bs.StatementNo} reopened" });
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private BankStatement? Load(int id) =>
        _repository.GetAll().Include(b => b.Lines).FirstOrDefault(b => b.Id == id);

    private static decimal Net(BankStatementLine l) => l.AmountIn - l.AmountOut;

    private static string? Trim(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? ValidateHeader(BankStatementRequest r)
    {
        if (r.FromDate == default || r.ToDate == default) return "Enter the period the statement covers.";
        if (r.FromDate > r.ToDate) return "The period start must be on or before the period end.";
        return null;
    }

    private static string? ValidateLines(List<BankStatementLineRequest> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            var l = lines[i];
            if (l.ValueDate == default) return $"Row {i + 1}: enter the date.";
            if (l.AmountIn < 0 || l.AmountOut < 0) return $"Row {i + 1}: amounts cannot be negative — put money out in the Out column.";
            if ((l.AmountIn > 0) == (l.AmountOut > 0)) return $"Row {i + 1}: enter an amount in either In or Out, not both.";
        }
        return null;
    }

    private static void ApplyHeader(BankStatement e, BankStatementRequest r)
    {
        e.BankName       = Trim(r.BankName);
        e.BankAccountNo  = Trim(r.BankAccountNo);
        e.StatementDate  = r.StatementDate == default ? r.ToDate : r.StatementDate;
        e.FromDate       = r.FromDate;
        e.ToDate         = r.ToDate;
        e.OpeningBalance = r.OpeningBalance;
        e.ClosingBalance = r.ClosingBalance;
        e.Remarks        = Trim(r.Remarks);
    }

    private static BankStatementLine NewLine(BankStatementLineRequest r) => new()
    {
        ValueDate   = r.ValueDate,
        Reference   = Trim(r.Reference),
        Description = Trim(r.Description),
        AmountIn    = r.AmountIn,
        AmountOut   = r.AmountOut,
        Remarks     = Trim(r.Remarks),
        Status      = "U",
        CreatedAt   = DateTime.UtcNow,
        InActive    = false,
    };

    private static void Renumber(BankStatement bs)
    {
        int n = 1;
        foreach (var l in bs.Lines.OrderBy(l => l.ValueDate).ThenBy(l => l.LineNum == 0 ? int.MaxValue : l.LineNum))
            l.LineNum = n++;
    }

    private static void Link(BankStatementLine line, string status, int? jeId, string? jeNo)
    {
        line.Status         = status;
        line.JournalEntryId = jeId;
        line.JournalNo      = jeNo;
        line.UpdatedAt      = DateTime.UtcNow;
    }

    private static JournalEntryLine JeLine(int n, string code, string name, decimal dr, decimal cr, string memo) => new()
    {
        LineNum     = n,
        AccountCode = code,
        AccountName = name,
        Debit       = dr,
        Credit      = cr,
        DebitLC     = dr,
        CreditLC    = cr,
        LineMemo    = memo,
        CreatedAt   = DateTime.UtcNow,
        InActive    = false,
    };

    // Every non-void journal entry that moved this bank account around the
    // statement period and isn't already matched or posted from any statement.
    // Payments, deposits and manual entries all reach the bank through a
    // journal entry, so this one source covers them without double counting.
    private List<BankStatementCandidate> BookMovements(BankStatement bs)
    {
        var from = bs.FromDate.AddDays(-CandidateWindowDays);
        var to   = bs.ToDate.AddDays(CandidateWindowDays);

        var taken = _db.Set<BankStatementLine>().AsNoTracking()
            .Where(l => l.Status != "U" && l.JournalEntryId != null)
            .Select(l => l.JournalEntryId!.Value)
            .ToHashSet();

        var rows = _db.Set<JournalEntry>().AsNoTracking()
            .Where(j => j.Status != "V" && j.TransDate >= from && j.TransDate <= to
                     && j.Lines.Any(l => l.AccountCode == bs.AccountCode))
            .Select(j => new
            {
                j.Id, j.JrnlNo, j.TransDate, j.TransType, j.RefNo, j.Ref2, j.Memo,
                Debit  = j.Lines.Where(l => l.AccountCode == bs.AccountCode).Sum(l => l.Debit),
                Credit = j.Lines.Where(l => l.AccountCode == bs.AccountCode).Sum(l => l.Credit),
            })
            .ToList();

        return rows
            .Where(r => !taken.Contains(r.Id) && r.Debit != r.Credit)
            .Select(r => new BankStatementCandidate
            {
                JournalEntryId = r.Id,
                JournalNo      = r.JrnlNo,
                TransDate      = r.TransDate,
                Source         = SourceLabel(r.TransType),
                RefNo          = r.RefNo ?? r.Ref2,
                Memo           = r.Memo,
                AmountIn       = r.Debit > r.Credit ? r.Debit - r.Credit : 0,
                AmountOut      = r.Credit > r.Debit ? r.Credit - r.Debit : 0,
            })
            .ToList();
    }

    // Same direction and same amount; best reference match first, then nearest date.
    private static List<BankStatementCandidate> CandidatesFor(BankStatementLine line, List<BankStatementCandidate> movements)
    {
        var reference = line.Reference?.Trim();
        return movements
            .Where(m => Math.Abs((m.AmountIn - m.AmountOut) - Net(line)) <= 0.005m)
            .Select(m => new BankStatementCandidate
            {
                JournalEntryId = m.JournalEntryId,
                JournalNo      = m.JournalNo,
                TransDate      = m.TransDate,
                Source         = m.Source,
                RefNo          = m.RefNo,
                Memo           = m.Memo,
                AmountIn       = m.AmountIn,
                AmountOut      = m.AmountOut,
                DaysApart      = Math.Abs((m.TransDate.Date - line.ValueDate.Date).Days),
                ReferenceMatch = !string.IsNullOrEmpty(reference) &&
                    (string.Equals(m.RefNo, reference, StringComparison.OrdinalIgnoreCase)
                     || (m.Memo?.Contains(reference, StringComparison.OrdinalIgnoreCase) ?? false)),
            })
            .OrderByDescending(c => c.ReferenceMatch)
            .ThenBy(c => c.DaysApart)
            .ToList();
    }

    private static string SourceLabel(string? transType) => transType switch
    {
        "KPYI" => "Incoming Payment",
        "KPYO" => "Outgoing Payment",
        "KDEP" => "Deposit",
        "KBST" => "Bank Statement",
        "JE" or null or "" => "Journal Entry",
        _ => transType,
    };

    private static BankStatementResponse ToResponse(BankStatement bs)
    {
        var lines = bs.Lines.OrderBy(l => l.LineNum).ToList();
        var running = bs.OpeningBalance;
        var lineResponses = lines.Select(l =>
        {
            running += Net(l);
            return new BankStatementLineResponse
            {
                Id = l.Id, LineNum = l.LineNum, ValueDate = l.ValueDate, Reference = l.Reference,
                Description = l.Description, AmountIn = l.AmountIn, AmountOut = l.AmountOut,
                Balance = running, Status = l.Status, JournalEntryId = l.JournalEntryId,
                JournalNo = l.JournalNo, Remarks = l.Remarks,
            };
        }).ToList();

        var totalIn  = lines.Sum(l => l.AmountIn);
        var totalOut = lines.Sum(l => l.AmountOut);
        var computed = bs.OpeningBalance + totalIn - totalOut;

        return new BankStatementResponse
        {
            Id = bs.Id, StatementNo = bs.StatementNo, AccountCode = bs.AccountCode, AccountName = bs.AccountName,
            BankName = bs.BankName, BankAccountNo = bs.BankAccountNo, Currency = bs.Currency,
            StatementDate = bs.StatementDate, FromDate = bs.FromDate, ToDate = bs.ToDate,
            OpeningBalance = bs.OpeningBalance, ClosingBalance = bs.ClosingBalance, Status = bs.Status,
            Remarks = bs.Remarks, CreatedAt = bs.CreatedAt, UpdatedAt = bs.UpdatedAt,
            TotalIn = totalIn, TotalOut = totalOut, ComputedClosing = computed,
            Difference = computed - bs.ClosingBalance,
            TotalLines = lines.Count,
            MatchedLines = lines.Count(l => l.Status == "M"),
            PostedLines = lines.Count(l => l.Status == "P"),
            UnmatchedLines = lines.Count(l => l.Status == "U"),
            Lines = lineResponses,
        };
    }

    private string GenerateStatementNo()
    {
        var prefix = $"BS-{DateTime.Now.Year}-";
        var last = _repository.GetAll()
            .Where(b => b.StatementNo.StartsWith(prefix))
            .OrderByDescending(b => b.Id)
            .FirstOrDefault();

        if (last == null) return $"{prefix}00001";
        var parts = last.StatementNo.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n) ? $"{prefix}{n + 1:D5}" : $"{prefix}00001";
    }
}

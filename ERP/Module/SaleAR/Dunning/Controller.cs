using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using BPEntity        = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using ARInvoiceEntity = FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice;
using IPayInvoice     = FarmingApi.Modules.Banking.IncomingPayment.IncomingPaymentInvoice;
using DunningTermEntity = FarmingApi.Modules.SaleAR.DunningTerm.DunningTerm;

namespace FarmingApi.Modules.SaleAR.Dunning;

// ═══════════════════════════════════════════════════════════════════
// DUNNING  (route: /Dunning)
// A wizard, not a new posting document — informational only, no GL
// impact, matching real SAP B1 dunning behavior.
//   GET  /Dunning/Candidates?runDate=   → who would be lettered, at
//                                          what level, today
//   POST /Dunning/Run                    → actually create the letters
//   GET  /Dunning                        → letter history
//   GET  /Dunning/{id}                   → one letter's detail
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class DunningController : ControllerBase
{
    private readonly IDunningLetterRepository _repository;
    private readonly IMapper                  _mapper;
    private readonly MyDbContext              _db;

    public DunningController(IDunningLetterRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines).OrderByDescending(x => x.RunDate).ToList();
        return Ok(_mapper.Map<List<DunningLetterResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<DunningLetterResponse>(e));
    }

    [HttpGet("Candidates")]
    public IActionResult Candidates([FromQuery] DateTime? runDate)
    {
        var (candidates, skippedNoTerm, skippedBlocked) = BuildCandidates(runDate?.Date ?? DateTime.UtcNow.Date);
        return Ok(new DunningCandidatesResponse
        {
            Candidates = candidates, SkippedNoTerm = skippedNoTerm, SkippedBlocked = skippedBlocked,
        });
    }

    [HttpPost("Run")]
    public IActionResult Run([FromBody] DunningRunRequest req)
    {
        if (req.CustomerCodes == null || req.CustomerCodes.Count == 0)
            return BadRequest("Select at least one customer to letter.");

        var runDate = req.RunDate.Date;
        var (candidates, _, _) = BuildCandidates(runDate);
        var selected = candidates.Where(c => req.CustomerCodes.Contains(c.CustomerCode)).ToList();
        if (selected.Count == 0)
            return BadRequest("None of the selected customers are still valid dunning candidates for this run date.");

        var results = new List<DunningRunResultLine>();
        foreach (var candidate in selected)
        {
            try
            {
                var letter = new DunningLetter
                {
                    LetterNo = NextNumber(), CustomerCode = candidate.CustomerCode, CustomerName = candidate.CustomerName,
                    DunningTermCode = candidate.DunningTermCode, Level = candidate.RecommendedLevel,
                    LetterText = candidate.RecommendedText, RunDate = runDate, TotalOverdue = candidate.TotalOverdue,
                    CreatedAt = DateTime.UtcNow, InActive = false,
                };
                foreach (var inv in candidate.Invoices)
                    letter.Lines.Add(new DunningLetterLine
                    {
                        ARInvoiceId = inv.InvoiceId, DocNum = inv.DocNum, DueDate = inv.DueDate,
                        DaysOverdue = inv.DaysOverdue, Balance = inv.Balance,
                    });

                _repository.Add(letter);

                // The BP's own DunningLevel/DunningDate fields exist but were never
                // written by anything until this run — now they're genuinely current.
                var bp = _db.Set<BPEntity>().First(b => b.Code == candidate.CustomerCode);
                bp.DunningLevel = candidate.RecommendedLevel.ToString();
                bp.DunningDate  = runDate;

                _repository.Commit();

                results.Add(new DunningRunResultLine
                {
                    CustomerCode = candidate.CustomerCode, CustomerName = candidate.CustomerName, Success = true,
                    LetterId = letter.Id, LetterNo = letter.LetterNo, Level = letter.Level, Amount = letter.TotalOverdue,
                });
            }
            catch (Exception ex)
            {
                results.Add(new DunningRunResultLine
                {
                    CustomerCode = candidate.CustomerCode, CustomerName = candidate.CustomerName, Success = false,
                    Error = ex.InnerException?.Message ?? ex.Message, Amount = candidate.TotalOverdue,
                });
            }
        }

        return Ok(new DunningRunResponse
        {
            LettersCreated = results.Count(r => r.Success),
            Results        = results,
        });
    }

    // ── Shared candidate-building logic for both Candidates (preview) and Run (execute) ──
    private (List<DunningCandidate>, List<string>, List<string>) BuildCandidates(DateTime runDate)
    {
        var terms = _db.Set<DunningTermEntity>().Include(t => t.Levels).ToDictionary(t => t.Code, t => t);

        var invoices = _db.Set<ARInvoiceEntity>().Where(i => i.Status == "O" && i.DueDate != null).ToList();
        var overdue = invoices
            .Select(i => new { Invoice = i, DaysOverdue = (runDate - i.DueDate!.Value.Date).Days })
            .Where(x => x.DaysOverdue > 0)
            .ToList();

        var customerIds = overdue.Select(x => x.Invoice.CustomerId).Distinct().ToList();
        var customers = _db.Set<BPEntity>().Where(b => customerIds.Contains(b.Id))
            .ToDictionary(b => b.Id, b => b);

        var candidates     = new List<DunningCandidate>();
        var skippedNoTerm  = new List<string>();
        var skippedBlocked = new List<string>();

        foreach (var group in overdue.GroupBy(x => x.Invoice.CustomerId))
        {
            if (!customers.TryGetValue(group.Key, out var bp)) continue;

            var lines = group.Select(x => new DunningCandidateInvoice
            {
                InvoiceId = x.Invoice.Id, DocNum = x.Invoice.DocNum, DueDate = x.Invoice.DueDate,
                DaysOverdue = x.DaysOverdue, Balance = OpenArBalance(x.Invoice.Id, x.Invoice.Total),
            }).Where(l => l.Balance > 0).ToList();
            if (lines.Count == 0) continue;

            if (bp.BlockDunningLetters) { skippedBlocked.Add($"{bp.Code} — {bp.CardName}"); continue; }
            if (string.IsNullOrWhiteSpace(bp.DunningTerm) || !terms.TryGetValue(bp.DunningTerm, out var term))
            {
                skippedNoTerm.Add($"{bp.Code} — {bp.CardName}");
                continue;
            }

            var worstDaysOverdue = lines.Max(l => l.DaysOverdue);
            var matchedLevel = term.Levels.Where(l => l.DaysOverdue <= worstDaysOverdue)
                .OrderByDescending(l => l.Level).FirstOrDefault();
            if (matchedLevel == null) continue; // overdue, but not yet past even level 1's threshold

            candidates.Add(new DunningCandidate
            {
                CustomerCode = bp.Code, CustomerName = bp.CardName,
                DunningTermCode = term.Code, DunningTermName = term.Name,
                RecommendedLevel = matchedLevel.Level, RecommendedText = matchedLevel.LetterText,
                TotalOverdue = lines.Sum(l => l.Balance), Invoices = lines.OrderBy(l => l.DueDate).ToList(),
            });
        }

        return (candidates.OrderBy(c => c.CustomerName).ToList(), skippedNoTerm, skippedBlocked);
    }

    // Same open-balance calculation PaymentRunController uses for AR invoices.
    private decimal OpenArBalance(int invoiceId, decimal total)
    {
        var applied = _db.Set<IPayInvoice>()
            .Where(l => l.InvoiceId == invoiceId && l.IncomingPayment.Status != "V")
            .Sum(l => (decimal?)l.AppliedAmount) ?? 0m;
        return total - applied;
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"DUN-{year}-";
        var highest = _repository.GetAll().Where(c => c.LetterNo.StartsWith(prefix))
            .Select(c => c.LetterNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Financials.ConversionDifferences;

// ═══════════════════════════════════════════════════════════════
// GET    /ConversionDifferences            → run headers, newest first
// GET    /ConversionDifferences/{id}       → one run with its lines
// POST   /ConversionDifferences            → create a run (Open)
// PUT    /ConversionDifferences/{id}       → amend an Open run
// POST   /ConversionDifferences/{id}/Post    → Open -> Posted
// POST   /ConversionDifferences/{id}/Cancel  → Open -> Cancelled
// DELETE /ConversionDifferences/{id}       → remove a non-Posted run
//
// Line differences and header totals are always computed here rather
// than trusted from the client, so a run's arithmetic cannot disagree
// with the rate it claims to have used.
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ConversionDifferencesController : ControllerBase
{
    private readonly IConversionDifferenceRepository _repo;
    private readonly IMapper _mapper;

    public ConversionDifferencesController(
        IConversionDifferenceRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    private const string StatusOpen      = "Open";
    private const string StatusPosted    = "Posted";
    private const string StatusCancelled = "Cancelled";

    /// <summary>Next CDF-{year}-{00001} for the run's posting year.</summary>
    private string NextDocNo(int year)
    {
        var prefix = $"CDF-{year}-";
        var last = _repo.GetAll()
            .Where(x => x.DocNo != null && x.DocNo.StartsWith(prefix))
            .OrderByDescending(x => x.DocNo)
            .Select(x => x.DocNo)
            .FirstOrDefault();

        var next = 1;
        if (last != null && int.TryParse(last[prefix.Length..], out var n)) next = n + 1;
        return prefix + next.ToString("D5");
    }

    /// <summary>
    /// Restates each line at the run's conversion rate and rolls the results
    /// into the header totals. A positive difference is a gain, a negative one
    /// a loss; TotalLoss is stored unsigned so both totals read as magnitudes.
    /// </summary>
    private static void Recalculate(ConversionDifference run)
    {
        decimal gain = 0m, loss = 0m;
        var lineNum = 1;

        foreach (var line in run.Lines)
        {
            line.LineNum             = lineNum++;
            line.CurrentRate         = run.ConversionRate;
            line.SystemBalanceBefore = Math.Round(line.LocalBalance * line.HistoricalRate, 2);
            line.SystemBalanceAfter  = Math.Round(line.LocalBalance * run.ConversionRate, 2);
            line.Difference          = line.SystemBalanceAfter - line.SystemBalanceBefore;

            if (line.Difference >= 0) gain += line.Difference;
            else                      loss += -line.Difference;
        }

        run.TotalGain     = gain;
        run.TotalLoss     = loss;
        run.NetDifference = gain - loss;
    }

    private string? Validate(ConversionDifferenceRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.SystemCurrency))
            return "System currency is required.";
        if (dto.ConversionRate <= 0)
            return "Conversion rate must be greater than zero.";
        if (dto.Lines == null || dto.Lines.Count == 0)
            return "Add at least one account line before saving.";

        var local = string.IsNullOrWhiteSpace(dto.LocalCurrency) ? "KHR" : dto.LocalCurrency.Trim();
        if (string.Equals(local, dto.SystemCurrency.Trim(), StringComparison.OrdinalIgnoreCase))
            return "System currency must differ from the local currency.";

        foreach (var (line, i) in dto.Lines.Select((l, i) => (l, i + 1)))
        {
            if (string.IsNullOrWhiteSpace(line.AccountCode))
                return $"Line {i}: account code is required.";
            if (line.HistoricalRate <= 0)
                return $"Line {i}: historical rate must be greater than zero.";
        }

        var codes = dto.Lines.Select(l => l.AccountCode.Trim().ToUpper()).ToList();
        var dup = codes.FirstOrDefault(c => codes.Count(x => x == c) > 1);
        if (dup != null) return $"Account '{dup}' appears on more than one line.";

        return null;
    }

    private static void ApplyHeader(ConversionDifference run, ConversionDifferenceRequest dto)
    {
        run.PostingDate    = (dto.PostingDate  ?? DateTime.UtcNow).Date;
        run.DocumentDate   = (dto.DocumentDate ?? dto.PostingDate ?? DateTime.UtcNow).Date;
        run.LocalCurrency  = string.IsNullOrWhiteSpace(dto.LocalCurrency)
            ? "KHR" : dto.LocalCurrency.Trim().ToUpper();
        run.SystemCurrency = dto.SystemCurrency.Trim().ToUpper();
        run.ConversionRate = dto.ConversionRate;
        run.AccountFrom    = dto.AccountFrom;
        run.AccountTo      = dto.AccountTo;
        run.Remarks        = dto.Remarks;
    }

    private static List<ConversionDifferenceLine> BuildLines(ConversionDifferenceRequest dto) =>
        dto.Lines.Select(l => new ConversionDifferenceLine
        {
            SourceType     = string.IsNullOrWhiteSpace(l.SourceType) ? "GL" : l.SourceType.Trim().ToUpper(),
            AccountCode    = l.AccountCode.Trim().ToUpper(),
            AccountName    = l.AccountName,
            LocalBalance   = l.LocalBalance,
            HistoricalRate = l.HistoricalRate,
            Remarks        = l.Remarks,
        }).ToList();

    // ── GET /ConversionDifferences ─────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? currency, [FromQuery] string? status)
    {
        var q = _repo.GetAll().Include(x => x.Lines).AsQueryable();

        if (!string.IsNullOrWhiteSpace(currency))
            q = q.Where(x => x.SystemCurrency == currency.Trim().ToUpper());
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(x => x.Status == status.Trim());

        var list = q.OrderByDescending(x => x.PostingDate).ThenByDescending(x => x.Id).ToList();
        var result = _mapper.Map<List<ConversionDifferenceResponse>>(list);
        // The list screen only needs headers; lines are fetched per run.
        foreach (var r in result) r.Lines.Clear();
        return Ok(result);
    }

    // ── GET /ConversionDifferences/{id} ────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var run = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (run == null) return NotFound($"Conversion difference run not found: {id}");

        var dto = _mapper.Map<ConversionDifferenceResponse>(run);
        dto.Lines = dto.Lines.OrderBy(l => l.LineNum).ToList();
        return Ok(dto);
    }

    // ── POST /ConversionDifferences ────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] ConversionDifferenceRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var problem = Validate(dto);
        if (problem != null) return BadRequest(problem);

        var run = new ConversionDifference { Status = StatusOpen, CreatedAt = DateTime.UtcNow, InActive = false };
        ApplyHeader(run, dto);
        run.Lines = BuildLines(dto);
        run.DocNo = NextDocNo(run.PostingDate.Year);
        Recalculate(run);

        _repo.Add(run);
        _repo.Commit();

        return Ok(_mapper.Map<ConversionDifferenceResponse>(run));
    }

    // ── PUT /ConversionDifferences/{id} ────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ConversionDifferenceRequest dto)
    {
        var run = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (run == null) return NotFound($"Conversion difference run not found: {id}");
        if (run.Status != StatusOpen)
            return BadRequest($"This run is {run.Status.ToLower()} and can no longer be changed.");

        var problem = Validate(dto);
        if (problem != null) return BadRequest(problem);

        ApplyHeader(run, dto);
        run.Lines.Clear();
        foreach (var line in BuildLines(dto)) run.Lines.Add(line);
        Recalculate(run);
        run.UpdatedAt = DateTime.UtcNow;

        _repo.Update(run);
        _repo.Commit();

        return Ok(_mapper.Map<ConversionDifferenceResponse>(run));
    }

    // ── POST /ConversionDifferences/{id}/Post ──────────────────
    [HttpPost("{id:int}/Post")]
    public IActionResult Post(int id)
    {
        var run = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (run == null) return NotFound($"Conversion difference run not found: {id}");
        if (run.Status == StatusPosted)    return BadRequest("This run has already been posted.");
        if (run.Status == StatusCancelled) return BadRequest("A cancelled run cannot be posted.");

        run.Status    = StatusPosted;
        run.UpdatedAt = DateTime.UtcNow;
        _repo.Update(run);
        _repo.Commit();

        return Ok(_mapper.Map<ConversionDifferenceResponse>(run));
    }

    // ── POST /ConversionDifferences/{id}/Cancel ────────────────
    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var run = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (run == null) return NotFound($"Conversion difference run not found: {id}");
        if (run.Status == StatusPosted)
            return BadRequest("A posted run cannot be cancelled. Raise a reversing run instead.");
        if (run.Status == StatusCancelled) return BadRequest("This run is already cancelled.");

        run.Status    = StatusCancelled;
        run.UpdatedAt = DateTime.UtcNow;
        _repo.Update(run);
        _repo.Commit();

        return Ok(_mapper.Map<ConversionDifferenceResponse>(run));
    }

    // ── DELETE /ConversionDifferences/{id} ─────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var run = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (run == null) return NotFound($"Conversion difference run not found: {id}");
        if (run.Status == StatusPosted)
            return BadRequest("A posted run cannot be deleted.");

        _repo.Remove(run);
        _repo.Commit();
        return NoContent();
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.CRM.Opportunity;

[ApiController]
[Route("[controller]")]
public class OpportunityController : ControllerBase
{
    private readonly IOpportunityRepository _repo;
    private readonly IMapper                _mapper;
    private readonly MyDbContext            _db;

    public OpportunityController(IOpportunityRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /Opportunity ───────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? status        = null,
        [FromQuery] string? salesEmployee = null,
        [FromQuery] string? bpCode        = null)
    {
        var list = IncludeAll(_repo.GetAll())
            .Where(x => status        == null || x.Status        == status)
            .Where(x => salesEmployee == null || x.SalesEmployee == salesEmployee)
            .Where(x => bpCode        == null || x.BPCode        == bpCode)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
        return Ok(_mapper.Map<List<OpportunityResponse>>(list));
    }

    // ── GET /Opportunity/{id} ──────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = IncludeAll(_repo.GetAll()).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<OpportunityResponse>(e));
    }

    // ── GET /Opportunity/Summary ───────────────────────────────
    [AllowAnonymous][HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        return Ok(new OpportunitySummary
        {
            Total          = all.Count,
            Open           = all.Count(x => x.Status == "Open"),
            Won            = all.Count(x => x.Status == "Won"),
            Lost           = all.Count(x => x.Status == "Lost"),
            TotalPotential = all.Where(x => x.Status == "Open").Sum(x => x.PotentialAmount),
            TotalWeighted  = all.Where(x => x.Status == "Open").Sum(x => x.WeightedAmount),
            WonValue       = all.Where(x => x.Status == "Won").Sum(x => x.PotentialAmount),
        });
    }

    // ── POST /Opportunity ──────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] OpportunityRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(dto.OpportunityName))
            return BadRequest("Opportunity Name is required");

        var entity = _mapper.Map<Opportunity>(dto);
        entity.OpportunityNo = GenerateOppNo();
        entity.Status        = "Open";
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;

        ApplyCalculations(entity);
        RenumberLines(entity);

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<OpportunityResponse>(entity));
    }

    // ── PUT /Opportunity/{id} ──────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] OpportunityRequest dto)
    {
        var entity = IncludeAll(_repo.GetAll()).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Open")
            return BadRequest($"Cannot edit an opportunity with status '{entity.Status}'");

        // Clear child grids before remap
        _db.RemoveRange(entity.InterestRanges);
        _db.RemoveRange(entity.Stages);
        _db.RemoveRange(entity.Partners);
        _db.RemoveRange(entity.Competitors);
        _db.RemoveRange(entity.Reasons);
        _db.RemoveRange(entity.Attachments);

        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        ApplyCalculations(entity);
        RenumberLines(entity);

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<OpportunityResponse>(entity));
    }

    // ── PUT /Opportunity/{id}/Status ───────────────────────────
    // Transition to Won / Lost / Open
    [AllowAnonymous][HttpPut("{id:int}/Status")]
    public IActionResult UpdateStatus(int id, [FromBody] OpportunityStatusRequest dto)
    {
        var allowed = new[] { "Open", "Won", "Lost" };
        if (!allowed.Contains(dto.Status)) return BadRequest("Status must be Open, Won or Lost");

        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        entity.Status = dto.Status;
        if (dto.Status is "Won" or "Lost")
        {
            entity.ClosingDate   = DateTime.UtcNow.Date;
            entity.ClosingPercent= dto.Status == "Won" ? 100 : entity.ClosingPercent;
        }
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { id, status = entity.Status, closingDate = entity.ClosingDate });
    }

    // ── DELETE /Opportunity/{id} ───────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ─── Helpers ──────────────────────────────────────────────
    private static IQueryable<Opportunity> IncludeAll(IQueryable<Opportunity> q) => q
        .Include(x => x.InterestRanges)
        .Include(x => x.Stages)
        .Include(x => x.Partners)
        .Include(x => x.Competitors)
        .Include(x => x.Reasons)
        .Include(x => x.Attachments);

    private string GenerateOppNo()
    {
        var year  = DateTime.UtcNow.Year;
        var count = _repo.GetAll().Count() + 1;
        return $"OPP-{year}-{count:D5}";
    }

    private static void ApplyCalculations(Opportunity entity)
    {
        // Weighted Amount = Potential Amount × Closing %
        entity.WeightedAmount = Math.Round(
            entity.PotentialAmount * (entity.ClosingPercent / 100m), 2);

        // Gross Profit Total = Potential Amount × Gross Profit %
        entity.GrossProfitTotal = Math.Round(
            entity.PotentialAmount * (entity.GrossProfitPercent / 100m), 2);

        // Mirror latest stage % onto header closing % when stages exist
        var latestStage = entity.Stages.OrderByDescending(s => s.LineNum).FirstOrDefault();
        if (latestStage != null && entity.ClosingPercent == 0)
            entity.ClosingPercent = (int)latestStage.Percent;
    }

    private static void RenumberLines(Opportunity entity)
    {
        int i = 1; foreach (var x in entity.InterestRanges) x.LineNum = i++;
        i = 1; foreach (var x in entity.Stages)             x.LineNum = i++;
        i = 1; foreach (var x in entity.Partners)           x.LineNum = i++;
        i = 1; foreach (var x in entity.Competitors)        x.LineNum = i++;
        i = 1; foreach (var x in entity.Reasons)             x.LineNum = i++;
        i = 1; foreach (var x in entity.Attachments)         x.LineNum = i++;
    }
}
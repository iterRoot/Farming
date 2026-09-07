using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.PostingPeriods;

// ═══════════════════════════════════════════════════════════════
// POSTING PERIODS
// GET /PostingPeriod        → { items, createNextYearDueDates, ... }
// PUT /PostingPeriod        → replaces the grid + options in one call
// DELETE /PostingPeriod/{id}
// ═══════════════════════════════════════════════════════════════
public class PostingPeriodController : MyController
{
    private readonly IPostingPeriodRepository _repository;
    private readonly IPostingPeriodSettingRepository _settings;

    public PostingPeriodController(
        IPostingPeriodRepository repository,
        IPostingPeriodSettingRepository settings)
    {
        _repository = repository;
        _settings   = settings;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        var opts = _settings.GetAll().OrderBy(x => x.Id).FirstOrDefault();

        return Ok(new PostingPeriodsResponse
        {
            Items = _repository.GetAll()
                .OrderBy(x => x.PostingDateFrom).ThenBy(x => x.PeriodCode)
                .Select(p => new PostingPeriodDto
                {
                    Id = p.Id, PeriodCode = p.PeriodCode, PeriodName = p.PeriodName,
                    PeriodStatus = p.PeriodStatus,
                    PostingDateFrom = p.PostingDateFrom, PostingDateTo = p.PostingDateTo,
                    DueDateFrom = p.DueDateFrom, DueDateTo = p.DueDateTo,
                    DocumentDateFrom = p.DocumentDateFrom, DocumentDateTo = p.DocumentDateTo,
                }).ToList(),
            CreateNextYearDueDates = opts?.CreateNextYearDueDates ?? false,
            AutoUpdateStatus       = opts?.AutoUpdateStatus       ?? false,
            DaysAfterNewPeriod     = opts?.DaysAfterNewPeriod     ?? 0,
        });
    }

    [AllowAnonymous]
    [HttpPut]
    public IActionResult Save([FromBody] PostingPeriodsSaveRequest request)
    {
        if (request == null) return BadRequest("No posting periods supplied.");

        var rows = request.Periods
            .Where(p => !string.IsNullOrWhiteSpace(p.PeriodCode))
            .ToList();

        if (rows.Count == 0) return BadRequest("Add at least one posting period.");

        var dup = rows.GroupBy(p => p.PeriodCode.Trim().ToUpperInvariant())
                      .FirstOrDefault(g => g.Count() > 1);
        if (dup != null)
            return BadRequest($"Period code '{dup.First().PeriodCode}' appears more than once.");

        // Grid semantics: what's on screen becomes the full set.
        var existing = _repository.GetAll().ToList();
        var keepIds  = rows.Where(r => r.Id.HasValue).Select(r => r.Id!.Value).ToHashSet();
        foreach (var gone in existing.Where(e => !keepIds.Contains(e.Id)))
            _repository.Remove(gone);

        foreach (var r in rows)
        {
            var code   = r.PeriodCode.Trim();
            var entity = r.Id.HasValue ? existing.FirstOrDefault(e => e.Id == r.Id.Value) : null;
            var isNew  = entity == null;
            entity ??= new PostingPeriod { CreatedAt = DateTime.UtcNow, InActive = false };

            entity.PeriodCode       = code;
            entity.PeriodName       = r.PeriodName;
            entity.PeriodStatus     = string.IsNullOrWhiteSpace(r.PeriodStatus) ? "Unlocked" : r.PeriodStatus;
            entity.PostingDateFrom  = r.PostingDateFrom;
            entity.PostingDateTo    = r.PostingDateTo;
            entity.DueDateFrom      = r.DueDateFrom;
            entity.DueDateTo        = r.DueDateTo;
            entity.DocumentDateFrom = r.DocumentDateFrom;
            entity.DocumentDateTo   = r.DocumentDateTo;

            if (isNew) _repository.Add(entity);
            else { entity.UpdatedAt = DateTime.UtcNow; _repository.Update(entity); }
        }

        // Options (singleton)
        var opts = _settings.GetAll().OrderBy(x => x.Id).FirstOrDefault();
        var newOpts = opts == null;
        opts ??= new PostingPeriodSetting { CreatedAt = DateTime.UtcNow, InActive = false };
        opts.CreateNextYearDueDates = request.CreateNextYearDueDates;
        opts.AutoUpdateStatus       = request.AutoUpdateStatus;
        opts.DaysAfterNewPeriod     = request.DaysAfterNewPeriod;
        if (newOpts) _settings.Add(opts);
        else { opts.UpdatedAt = DateTime.UtcNow; _settings.Update(opts); }

        _repository.Commit();
        _settings.Commit();

        return Ok(new { message = $"{rows.Count} posting period(s) saved successfully" });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Posting period not found: {id}");
        _repository.Remove(item);
        _repository.Commit();
        return NoContent();
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.CRM.Campaign;

// ═══════════════════════════════════════════════════════════════════
// CAMPAIGN  (route: /Campaign)
// GET    /Campaign
// GET    /Campaign/{id}
// POST   /Campaign
// PUT    /Campaign/{id}
// PUT    /Campaign/{id}/Targets/{targetId}/Status  → record a response
// DELETE /Campaign/{id}
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class CampaignController : ControllerBase
{
    private readonly ICampaignRepository _repository;
    private readonly IMapper             _mapper;
    private readonly MyDbContext         _db;

    public CampaignController(ICampaignRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? status = null)
    {
        var list = _repository.GetAll().Include(x => x.Targets)
            .Where(x => status == null || x.Status == status)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<CampaignResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Targets).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<CampaignResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CampaignRequest dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new Campaign
        {
            CampaignNo      = NextNumber(),
            CampaignName    = dto.CampaignName.Trim(),
            CampaignType    = dto.CampaignType,
            Status          = dto.Status,
            StartDate       = dto.StartDate == default ? DateTime.UtcNow.Date : dto.StartDate,
            EndDate         = dto.EndDate,
            Budget          = dto.Budget,
            ActualCost      = dto.ActualCost,
            ExpectedRevenue = dto.ExpectedRevenue,
            Owner           = dto.Owner,
            Remarks         = dto.Remarks,
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };
        AddTargets(entity, dto.Targets);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<CampaignResponse>(ReloadWithTargets(entity.Id)));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CampaignRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Targets).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.CampaignName    = dto.CampaignName.Trim();
        entity.CampaignType    = dto.CampaignType;
        entity.Status          = dto.Status;
        entity.StartDate       = dto.StartDate == default ? entity.StartDate : dto.StartDate;
        entity.EndDate         = dto.EndDate;
        entity.Budget          = dto.Budget;
        entity.ActualCost      = dto.ActualCost;
        entity.ExpectedRevenue = dto.ExpectedRevenue;
        entity.Owner           = dto.Owner;
        entity.Remarks         = dto.Remarks;
        entity.UpdatedAt       = DateTime.UtcNow;

        // Add newly-listed targets only — never drop an existing target (and
        // its response history) just because a later edit's list left it out.
        var existingCodes = entity.Targets.Select(t => t.BPCode).ToHashSet();
        AddTargets(entity, dto.Targets.Where(t => !existingCodes.Contains(t.BPCode)).ToList());

        _repository.Commit();
        return Ok(_mapper.Map<CampaignResponse>(ReloadWithTargets(entity.Id)));
    }

    [HttpPut("{id:int}/Targets/{targetId:int}/Status")]
    public IActionResult SetTargetStatus(int id, int targetId, [FromBody] CampaignTargetStatusRequest dto)
    {
        var target = _db.Set<CampaignTarget>().FirstOrDefault(t => t.Id == targetId && t.CampaignId == id);
        if (target == null) return NotFound();
        if (!CampaignOptions.TargetStatuses.Contains(dto.Status))
            return BadRequest($"Status must be one of: {string.Join(", ", CampaignOptions.TargetStatuses)}.");

        target.Status        = dto.Status;
        target.ResponseNotes = dto.ResponseNotes;
        target.ResponseDate  = dto.Status == "NotContacted" ? null : DateTime.UtcNow;
        target.UpdatedAt     = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(_mapper.Map<CampaignTargetResponse>(target));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private string? Validate(CampaignRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CampaignName)) return "Campaign name is required.";
        if (!CampaignOptions.Types.Contains(dto.CampaignType))
            return $"Campaign type must be one of: {string.Join(", ", CampaignOptions.Types)}.";
        if (!CampaignOptions.Statuses.Contains(dto.Status))
            return $"Status must be one of: {string.Join(", ", CampaignOptions.Statuses)}.";
        if (dto.EndDate.HasValue && dto.StartDate != default && dto.EndDate < dto.StartDate)
            return "End date cannot be before the start date.";
        if (dto.Targets.Select(t => t.BPCode).Distinct().Count() != dto.Targets.Count)
            return "The same business partner is targeted more than once.";
        if (dto.Targets.Count > 0)
        {
            var codes = dto.Targets.Select(t => t.BPCode).ToList();
            var found = _db.Set<BPEntity>().Where(b => codes.Contains(b.Code)).Select(b => b.Code).ToHashSet();
            var missing = codes.Where(c => !found.Contains(c)).ToList();
            if (missing.Count > 0) return $"Business partner(s) not found: {string.Join(", ", missing)}.";
        }
        return null;
    }

    private void AddTargets(Campaign entity, List<CampaignTargetRequest> targets)
    {
        if (targets.Count == 0) return;
        var codes = targets.Select(t => t.BPCode).ToList();
        var bps = _db.Set<BPEntity>().Where(b => codes.Contains(b.Code)).ToDictionary(b => b.Code, b => b.CardName);

        foreach (var t in targets)
        {
            entity.Targets.Add(new CampaignTarget
            {
                BPCode = t.BPCode, BPName = bps[t.BPCode], Status = "NotContacted",
                CreatedAt = DateTime.UtcNow, InActive = false,
            });
        }
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CAMP-{year}-";
        var highest = _repository.GetAll().Where(c => c.CampaignNo.StartsWith(prefix))
            .Select(c => c.CampaignNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }

    private Campaign ReloadWithTargets(int id) =>
        _repository.GetAll().Include(x => x.Targets).First(x => x.Id == id);
}

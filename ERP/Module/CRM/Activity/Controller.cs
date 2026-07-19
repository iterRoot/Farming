using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.CRM.Activity;

[ApiController]
[Route("[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityRepository _repo;
    private readonly IMapper _mapper;
    private readonly MyDbContext _db;

    public ActivityController(IActivityRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /Activity ──────────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? status         = null,   // Open/Closed/Inactive
        [FromQuery] string? activityKind   = null,
        [FromQuery] string? assignedToCode = null,
        [FromQuery] string? bpCode         = null)
    {
        var list = _repo.GetAll()
            .Include(x => x.Attachments)
            .Where(x => activityKind   == null || x.ActivityKind   == activityKind)
            .Where(x => assignedToCode == null || x.AssignedToCode == assignedToCode)
            .Where(x => bpCode         == null || x.BPCode         == bpCode)
            .OrderByDescending(x => x.StartDateTime)
            .ToList();

        var mapped = _mapper.Map<List<ActivityResponse>>(list);
        if (status != null) mapped = mapped.Where(x => x.Status == status).ToList();
        return Ok(mapped);
    }

    // ── GET /Activity/{id} ─────────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll().Include(x => x.Attachments).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ActivityResponse>(e));
    }

    // ── GET /Activity/Summary ──────────────────────────────────
    [AllowAnonymous][HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        var now = DateTime.UtcNow;
        return Ok(new ActivitySummary
        {
            Total   = all.Count,
            Open    = all.Count(x => !x.IsClosed && !x.IsInactive),
            Closed  = all.Count(x => x.IsClosed),
            Overdue = all.Count(x => !x.IsClosed && !x.IsInactive && x.EndDateTime < now),
            Today   = all.Count(x => x.StartDateTime.Date == now.Date),
            ByKind  = all.GroupBy(x => x.ActivityKind).ToDictionary(g => g.Key, g => g.Count()),
        });
    }

    // ── POST /Activity ─────────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ActivityRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (dto.EndDateTime < dto.StartDateTime)
            return BadRequest("End time cannot be before start time");

        var entity = _mapper.Map<Activity>(dto);
        entity.ActivityNo      = GenerateActivityNo();
        entity.DurationMinutes = (int)(dto.EndDateTime - dto.StartDateTime).TotalMinutes;
        entity.AssignedBy      = "current-user"; // replace with auth context when available
        entity.CreatedAt       = DateTime.UtcNow;
        entity.InActive         = false;

        int i = 1;
        foreach (var a in entity.Attachments) a.LineNum = i++;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ActivityResponse>(entity));
    }

    // ── PUT /Activity/{id} ─────────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ActivityRequest dto)
    {
        var entity = _repo.GetAll().Include(x => x.Attachments).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (dto.EndDateTime < dto.StartDateTime)
            return BadRequest("End time cannot be before start time");

        _db.RemoveRange(entity.Attachments);
        _mapper.Map(dto, entity);
        entity.DurationMinutes = (int)(dto.EndDateTime - dto.StartDateTime).TotalMinutes;
        entity.UpdatedAt       = DateTime.UtcNow;

        int i = 1;
        foreach (var a in entity.Attachments) a.LineNum = i++;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ActivityResponse>(entity));
    }

    // ── PUT /Activity/{id}/Close ───────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}/Close")]
    public IActionResult SetClosed(int id, [FromBody] ActivityCloseRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsClosed = dto.Closed;
        entity.ClosedAt = dto.Closed ? DateTime.UtcNow : null;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { id, isClosed = entity.IsClosed });
    }

    // ── POST /Activity/{id}/FollowUp ───────────────────────────
    // Creates a new Activity pre-filled from this one (BP, contact, etc.)
    [AllowAnonymous][HttpPost("{id:int}/FollowUp")]
    public IActionResult FollowUp(int id, [FromBody] FollowUpRequest dto)
    {
        var source = _repo.GetSingle(x => x.Id == id);
        if (source == null) return NotFound();

        var followUp = new Activity
        {
            ActivityNo          = GenerateActivityNo(),
            ActivityKind        = dto.ActivityKind,
            Category            = source.Category,
            Subject             = dto.Subject ?? source.Subject,
            BPCode              = source.BPCode,
            BPName              = source.BPName,
            ContactPerson       = source.ContactPerson,
            TelephoneNo         = source.TelephoneNo,
            AssignedToType      = source.AssignedToType,
            AssignedToCode      = source.AssignedToCode,
            AssignedBy          = "current-user",
            StartDateTime       = dto.StartDateTime,
            EndDateTime         = dto.EndDateTime,
            DurationMinutes     = (int)(dto.EndDateTime - dto.StartDateTime).TotalMinutes,
            Priority            = source.Priority,
            Remarks             = dto.Remarks,
            PreviousActivityNo  = source.ActivityNo,
            CreatedAt           = DateTime.UtcNow,
            InActive             = false,
        };

        _repo.Add(followUp);
        _repo.Commit();
        return Ok(_mapper.Map<ActivityResponse>(followUp));
    }

    // ── DELETE /Activity/{id} ──────────────────────────────────
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
    private int GenerateActivityNo() => _repo.GetAll().Count() + 1;
}
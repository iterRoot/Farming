using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using CandidateEntity = FarmingApi.Modules.HumanResources.Candidate.Candidate;

namespace FarmingApi.Modules.HumanResources.JobPosting;

// ═══════════════════════════════════════════════════════════════════
// JOB POSTING  (route: /JobPosting)
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class JobPostingController : ControllerBase
{
    private readonly IJobPostingRepository _repository;
    private readonly IMapper               _mapper;
    private readonly MyDbContext           _db;

    public JobPostingController(IJobPostingRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? status = null)
    {
        var list = _repository.GetAll()
            .Where(x => status == null || x.Status == status)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(Attach(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(Attach(new List<JobPosting> { e })[0]);
    }

    [HttpPost]
    public IActionResult Create([FromBody] JobPostingRequest dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new JobPosting
        {
            PostingNo        = NextNumber(),
            Title            = dto.Title.Trim(),
            Department       = dto.Department,
            EmploymentType   = dto.EmploymentType,
            Status           = dto.Status,
            NumberOfOpenings = dto.NumberOfOpenings,
            OpenDate         = DateTime.UtcNow.Date,
            TargetCloseDate  = dto.TargetCloseDate,
            HiringManager    = dto.HiringManager,
            Description      = dto.Description,
            Requirements     = dto.Requirements,
            Remarks          = dto.Remarks,
            CreatedAt        = DateTime.UtcNow,
            InActive         = false,
        };

        _repository.Add(entity);
        _repository.Commit();
        return Ok(Attach(new List<JobPosting> { entity })[0]);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] JobPostingRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.Title            = dto.Title.Trim();
        entity.Department       = dto.Department;
        entity.EmploymentType   = dto.EmploymentType;
        entity.Status           = dto.Status;
        entity.NumberOfOpenings = dto.NumberOfOpenings;
        entity.TargetCloseDate  = dto.TargetCloseDate;
        entity.HiringManager    = dto.HiringManager;
        entity.Description      = dto.Description;
        entity.Requirements     = dto.Requirements;
        entity.Remarks          = dto.Remarks;
        entity.UpdatedAt        = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return Ok(Attach(new List<JobPosting> { entity })[0]);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var hasCandidates = _db.Set<CandidateEntity>().Any(c => c.JobPostingId == id);
        if (hasCandidates) return BadRequest("This posting has candidates against it and cannot be deleted — close it instead.");

        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private List<JobPostingResponse> Attach(List<JobPosting> postings)
    {
        var ids = postings.Select(p => p.Id).ToList();
        var candidates = _db.Set<CandidateEntity>().Where(c => ids.Contains(c.JobPostingId)).ToList();
        return postings.Select(p =>
        {
            var r = _mapper.Map<JobPostingResponse>(p);
            var mine = candidates.Where(c => c.JobPostingId == p.Id).ToList();
            r.CandidateCount = mine.Count;
            r.HiredCount     = mine.Count(c => c.Stage == "Hired");
            return r;
        }).ToList();
    }

    private static string? Validate(JobPostingRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return "Title is required.";
        if (!JobPostingOptions.EmploymentTypes.Contains(dto.EmploymentType))
            return $"Employment type must be one of: {string.Join(", ", JobPostingOptions.EmploymentTypes)}.";
        if (!JobPostingOptions.Statuses.Contains(dto.Status))
            return $"Status must be one of: {string.Join(", ", JobPostingOptions.Statuses)}.";
        if (dto.NumberOfOpenings <= 0) return "Number of openings must be greater than zero.";
        return null;
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"JOB-{year}-";
        var highest = _repository.GetAll().Where(j => j.PostingNo.StartsWith(prefix))
            .Select(j => j.PostingNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }
}

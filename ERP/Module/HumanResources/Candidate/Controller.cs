using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using JobPostingEntity = FarmingApi.Modules.HumanResources.JobPosting.JobPosting;
using EmployeeEntity   = FarmingApi.Modules.HumanResources.EmployeeMaster.EmployeeMaster;

namespace FarmingApi.Modules.HumanResources.Candidate;

// ═══════════════════════════════════════════════════════════════════
// CANDIDATE  (route: /Candidate)
// GET    /Candidate?jobPostingId=&stage=
// GET    /Candidate/{id}
// POST   /Candidate
// PUT    /Candidate/{id}
// PUT    /Candidate/{id}/Stage    → Applied/Screening/Interviewing/
//                                    Offered/Rejected (not Hired — see below)
// POST   /Candidate/{id}/Hire     → Offered only. Creates a real
//                                    Employee Master record and moves
//                                    the candidate to Hired.
// DELETE /Candidate/{id}          → not once Hired
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class CandidateController : ControllerBase
{
    private readonly ICandidateRepository _repository;
    private readonly IMapper              _mapper;
    private readonly MyDbContext          _db;

    public CandidateController(ICandidateRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] int? jobPostingId = null, [FromQuery] string? stage = null)
    {
        var list = _repository.GetAll()
            .Where(x => jobPostingId == null || x.JobPostingId == jobPostingId)
            .Where(x => stage == null || x.Stage == stage)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(Attach(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(Attach(new List<Candidate> { e })[0]);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CandidateRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("First and last name are required.");
        var posting = _db.Set<JobPostingEntity>().FirstOrDefault(p => p.Id == dto.JobPostingId);
        if (posting == null) return BadRequest($"Job posting {dto.JobPostingId} not found.");
        if (posting.Status != "Open") return BadRequest($"'{posting.Title}' is {posting.Status}, not accepting candidates.");

        var entity = new Candidate
        {
            CandidateNo    = NextNumber(),
            JobPostingId   = dto.JobPostingId,
            FirstName      = dto.FirstName.Trim(),
            LastName       = dto.LastName.Trim(),
            Email          = dto.Email,
            Phone          = dto.Phone,
            Stage          = "Applied",
            AppliedDate    = DateTime.UtcNow,
            ExpectedSalary = dto.ExpectedSalary,
            Notes          = dto.Notes,
            CreatedAt      = DateTime.UtcNow,
            InActive       = false,
        };

        _repository.Add(entity);
        _repository.Commit();
        return Ok(Attach(new List<Candidate> { entity })[0]);
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CandidateRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Stage is "Hired" or "Rejected") return BadRequest($"A {entity.Stage} candidate cannot be edited.");
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("First and last name are required.");

        entity.FirstName      = dto.FirstName.Trim();
        entity.LastName       = dto.LastName.Trim();
        entity.Email          = dto.Email;
        entity.Phone          = dto.Phone;
        entity.ExpectedSalary = dto.ExpectedSalary;
        entity.Notes          = dto.Notes;
        entity.UpdatedAt      = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return Ok(Attach(new List<Candidate> { entity })[0]);
    }

    private static readonly string[] StageOrder = { "Applied", "Screening", "Interviewing", "Offered" };

    [HttpPut("{id:int}/Stage")]
    public IActionResult SetStage(int id, [FromBody] CandidateStageRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Stage is "Hired" or "Rejected")
            return BadRequest($"A {entity.Stage} candidate's stage cannot be changed here.");
        if (dto.Stage == "Hired")
            return BadRequest("Use POST /Candidate/{id}/Hire to hire a candidate — it creates their Employee Master record too.");
        if (!StageOrder.Contains(dto.Stage) && dto.Stage != "Rejected")
            return BadRequest($"Stage must be one of: {string.Join(", ", StageOrder)}, Rejected.");
        if (dto.Stage == "Rejected" && string.IsNullOrWhiteSpace(dto.RejectionReason))
            return BadRequest("A rejection reason is required.");

        entity.Stage           = dto.Stage;
        entity.RejectionReason = dto.Stage == "Rejected" ? dto.RejectionReason : entity.RejectionReason;
        entity.UpdatedAt       = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(Attach(new List<Candidate> { entity })[0]);
    }

    [HttpPost("{id:int}/Hire")]
    public IActionResult Hire(int id, [FromBody] HireRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Stage != "Offered")
            return BadRequest($"Only a candidate at Offered can be hired (this one is {entity.Stage}).");
        if (string.IsNullOrWhiteSpace(dto.EmployeeCode)) return BadRequest("An employee code is required.");

        var code = dto.EmployeeCode.Trim().ToUpper();
        if (_db.Set<EmployeeEntity>().Any(e => e.Code == code))
            return BadRequest($"Employee code '{code}' already exists.");

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var employee = new EmployeeEntity
            {
                Code       = code,
                FirstName  = entity.FirstName,
                LastName   = entity.LastName,
                Email      = entity.Email,
                Mobile     = entity.Phone,
                Salary     = dto.Salary ?? entity.ExpectedSalary,
                HireDate   = dto.HireDate ?? DateTime.UtcNow.Date,
                JobTitle   = dto.JobTitle,
                Department = dto.Department,
                EmployeeType = dto.EmployeeType,
                IsActive   = true,
                CreatedAt  = DateTime.UtcNow,
                InActive   = false,
            };
            _db.Add(employee);
            _db.SaveChanges();

            entity.Stage        = "Hired";
            entity.EmployeeId   = employee.Id;
            entity.EmployeeCode = employee.Code;
            entity.UpdatedAt    = DateTime.UtcNow;
            _db.SaveChanges();

            tx.Commit();
            return Ok(new { message = $"{entity.FirstName} {entity.LastName} hired as {employee.Code}.", employeeId = employee.Id, employeeCode = employee.Code });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Stage == "Hired") return BadRequest("A Hired candidate is linked to a real employee record and cannot be deleted.");
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private List<CandidateResponse> Attach(List<Candidate> candidates)
    {
        var postingIds = candidates.Select(c => c.JobPostingId).Distinct().ToList();
        var postings = _db.Set<JobPostingEntity>().Where(p => postingIds.Contains(p.Id))
            .ToDictionary(p => p.Id, p => p.Title);
        return candidates.Select(c =>
        {
            var r = _mapper.Map<CandidateResponse>(c);
            r.JobPostingTitle = postings.GetValueOrDefault(c.JobPostingId);
            return r;
        }).ToList();
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CAND-{year}-";
        var highest = _repository.GetAll().Where(c => c.CandidateNo.StartsWith(prefix))
            .Select(c => c.CandidateNo).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }
}

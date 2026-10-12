using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;
using ApprovalRequestEntity = FarmingApi.Modules.Administration.ApprovalRequest.ApprovalRequest;

namespace FarmingApi.Modules.Administration.ApprovalProcess;

[ApiController]
[Route("[controller]")]
public class ApprovalProcessController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ApprovalProcessController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /ApprovalProcess/DocumentTypes ── dropdown options for Create/Update
    [HttpGet("DocumentTypes")]
    public IActionResult GetDocumentTypes() => Ok(ApprovalDocumentTypes.All);

    // ── GET /ApprovalProcess ─────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<ApprovalTemplate>()
            .Include(x => x.Approvers).ThenInclude(a => a.User)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ApprovalTemplateResponse>>(list));
    }

    // ── GET /ApprovalProcess/{id} ────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var template = await _db.Set<ApprovalTemplate>()
            .Include(x => x.Approvers).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (template == null) return NotFound();
        return Ok(_mapper.Map<ApprovalTemplateResponse>(template));
    }

    // ── POST /ApprovalProcess ────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApprovalTemplateRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var validation = await ValidateAsync(dto);
        if (validation != null) return validation;

        var template = _mapper.Map<ApprovalTemplate>(dto);
        template.Approvers = dto.ApproverUserIds.Distinct()
            .Select(uid => new ApprovalTemplateApprover { UserId = uid })
            .ToList();

        _db.Set<ApprovalTemplate>().Add(template);
        await _db.SaveChangesAsync();

        var created = await _db.Set<ApprovalTemplate>()
            .Include(x => x.Approvers).ThenInclude(a => a.User)
            .FirstAsync(x => x.Id == template.Id);

        return CreatedAtAction(nameof(GetById), new { id = template.Id },
            _mapper.Map<ApprovalTemplateResponse>(created));
    }

    // ── PUT /ApprovalProcess/{id} ────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ApprovalTemplateUpdateRequest dto)
    {
        var template = await _db.Set<ApprovalTemplate>()
            .Include(x => x.Approvers)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (template == null) return NotFound();

        var validation = await ValidateAsync(dto);
        if (validation != null) return validation;

        _db.RemoveRange(template.Approvers);
        _mapper.Map(dto, template);
        template.Approvers = dto.ApproverUserIds.Distinct()
            .Select(uid => new ApprovalTemplateApprover { UserId = uid })
            .ToList();
        template.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /ApprovalProcess/{id} ─────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _db.Set<ApprovalTemplate>().FindAsync(id);
        if (template == null) return NotFound();

        // Every approval request keeps a permanent FK to the template it was
        // decided under (an audit trail, like a Journal Entry's link to its
        // source document) — the database itself will refuse the delete once
        // any request, decided or not, references this template. Check first
        // so the caller gets a clear 400 instead of a raw DB constraint error.
        var hasAnyRequest = await _db.Set<ApprovalRequestEntity>()
            .AnyAsync(r => r.ApprovalTemplateId == id);
        if (hasAnyRequest)
            return BadRequest("Cannot delete a template that has approval history — deactivate it instead.");

        _db.Set<ApprovalTemplate>().Remove(template);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<IActionResult?> ValidateAsync(ApprovalTemplateRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required.");

        if (!ApprovalDocumentTypes.All.Contains(dto.DocumentType))
            return BadRequest($"Unknown document type '{dto.DocumentType}'.");

        if (dto.Operator != null && !ApprovalOperators.All.Contains(dto.Operator))
            return BadRequest($"Operator must be one of: {string.Join(", ", ApprovalOperators.All)}");

        if ((dto.Operator == null) != (dto.Amount == null))
            return BadRequest("Operator and Amount must both be set, or both left blank for an unconditional rule.");

        if (dto.ApproverUserIds == null || dto.ApproverUserIds.Count == 0)
            return BadRequest("At least one approver is required.");

        var distinctIds = dto.ApproverUserIds.Distinct().ToList();
        var foundCount = await _db.Set<UserEntity>().CountAsync(u => distinctIds.Contains(u.Id));
        if (foundCount != distinctIds.Count)
            return BadRequest("One or more approver users were not found.");

        return null;
    }
}

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Branch;

public class BranchController : MyController
{
    private readonly IMapper _mapper;
    private readonly IBranchRepository _repository;

    public BranchController(IBranchRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    // ── GET /Branch ────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var iQueryable = _repository.GetAll()
            .OrderByDescending(b => b.MainBranch).ThenBy(b => b.BranchCode);
        var results = _mapper.ProjectTo<BranchListResponse>(iQueryable).ToList();
        return Ok(results);
    }

    // ── GET /Branch/{id} ───────────────────────────────────────
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var branch = _repository.GetSingle(e => e.Id == id);
        if (branch == null) return NotFound($"Branch not found: {id}");
        return Ok(_mapper.Map<BranchListResponse>(branch));
    }

    // ── POST /Branch ───────────────────────────────────────────
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] BranchListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var error = Save(request, out var entity);
        if (error != null) return BadRequest(error);

        _repository.Commit();
        EnsureMainBranch();
        return Ok(new { message = "Branch saved successfully", id = entity!.Id, branchCode = entity.BranchCode });
    }

    // ── POST /Branch/Bulk ──────────────────────────────────────
    // Saves the whole "Branches - Setup" matrix in one call.
    [AllowAnonymous]
    [HttpPost("Bulk")]
    public IActionResult CreateBulk([FromBody] List<BranchListRequest> requests)
    {
        if (requests == null || requests.Count == 0)
            return BadRequest("Send at least one branch.");

        // Duplicate names inside the submitted batch
        var dupName = requests
            .GroupBy(r => (r.BranchName ?? "").Trim().ToLowerInvariant())
            .FirstOrDefault(g => g.Key.Length > 0 && g.Count() > 1);
        if (dupName != null)
            return BadRequest($"Branch name '{dupName.First().BranchName}' appears more than once.");

        if (requests.Count(r => r.MainBranch) > 1)
            return BadRequest("Only one branch can be marked as the main branch.");

        var saved = new List<Branch>();
        foreach (var request in requests)
        {
            var error = Save(request, out var entity);
            if (error != null) return BadRequest(error);
            saved.Add(entity!);
        }

        // Ids are only assigned once the insert is committed
        _repository.Commit();
        EnsureMainBranch();

        return Ok(new
        {
            message  = $"{saved.Count} branch(es) saved successfully",
            branches = saved.Select(b => new { id = b.Id, branchCode = b.BranchCode, branchName = b.BranchName }),
        });
    }

    // ── PUT /Branch/{id} ───────────────────────────────────────
    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BranchUpdateRequest request)
    {
        var branch = _repository.GetSingle(e => e.Id == id);
        if (branch == null) return NotFound($"Branch not found: {id}");

        if (string.IsNullOrWhiteSpace(request.BranchName))
            return BadRequest("Branch Name is required");

        // One branch must always be the main branch — clearing the flag on the
        // only main branch would leave the company with none, so reject it and
        // let the user promote a different branch instead.
        if (branch.MainBranch && !request.MainBranch)
            return BadRequest(
                "At least one branch must be the main branch. "
                + "Mark another branch as the main branch instead of clearing this one.");

        var code = branch.BranchCode;          // immutable
        _mapper.Map(request, branch);
        branch.BranchCode = code;
        branch.UpdatedAt  = DateTime.UtcNow;

        if (request.MainBranch) ClearOtherMainBranches(branch.Id);

        _repository.Update(branch);
        _repository.Commit();
        EnsureMainBranch();
        return NoContent();
    }

    // ── DELETE /Branch/{id} ────────────────────────────────────
    // (was [HttpDelete] with no template, so /Branch/{id} returned 405)
    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var branch = _repository.GetSingle(e => e.Id == id);
        if (branch == null) return NotFound($"Branch not found: {id}");

        branch.DeletedAt = DateTime.UtcNow;
        _repository.Remove(branch);
        _repository.Commit();

        // Deleting the main branch would leave none — promote another.
        EnsureMainBranch();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════

    /// <summary>Maps + validates one request. Returns an error string, or null on success.</summary>
    private string? Save(BranchListRequest request, out Branch? entity)
    {
        entity = null;

        if (string.IsNullOrWhiteSpace(request.BranchName))
            return "Branch Name is required";

        var code = string.IsNullOrWhiteSpace(request.BranchCode)
            ? GenerateCode(request.BranchName)
            : request.BranchCode.Trim().ToUpperInvariant();

        if (_repository.GetSingle(b => b.BranchCode == code) != null)
            return $"Branch code '{code}' already exists.";

        entity = _mapper.Map<Branch>(request);
        entity.BranchCode = code;
        entity.CreatedAt  = DateTime.UtcNow;
        entity.InActive   = false;

        if (entity.MainBranch) ClearOtherMainBranches(null);

        _repository.Add(entity);
        return null;
    }

    /// <summary>
    /// Guarantees that exactly one branch is the main branch. Runs after every
    /// write: when branches exist but none is flagged (first branch created,
    /// or the main branch was deleted), the oldest is promoted. Callers handle
    /// the "at most one" side via <see cref="ClearOtherMainBranches"/>.
    /// </summary>
    private void EnsureMainBranch()
    {
        var branches = _repository.GetAll().Where(b => !b.Disabled).ToList();
        if (branches.Count == 0 || branches.Any(b => b.MainBranch)) return;

        var promote = branches.OrderBy(b => b.Id).First();
        promote.MainBranch = true;
        promote.UpdatedAt  = DateTime.UtcNow;
        _repository.Update(promote);
        _repository.Commit();
    }

    /// <summary>Only one branch may be the main branch.</summary>
    private void ClearOtherMainBranches(int? keepId)
    {
        var others = _repository.GetAll()
            .Where(b => b.MainBranch && (keepId == null || b.Id != keepId))
            .ToList();

        foreach (var b in others)
        {
            b.MainBranch = false;
            _repository.Update(b);
        }
    }

    /// <summary>"Phnom Penh HQ" → "PHNOMPENHHQ", de-duplicated with a numeric suffix.</summary>
    private string GenerateCode(string name)
    {
        var baseCode = new string(name.Where(char.IsLetterOrDigit).ToArray())
            .ToUpperInvariant();
        if (baseCode.Length == 0) baseCode = "BRANCH";
        if (baseCode.Length > 20) baseCode = baseCode[..20];

        var code = baseCode;
        var n = 1;
        while (_repository.GetSingle(b => b.BranchCode == code) != null)
            code = $"{baseCode}{++n}";

        return code;
    }
}

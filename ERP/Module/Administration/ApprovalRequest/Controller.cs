using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ApprovalRequest;

// Read-only queue: rows here are created/decided exclusively through
// IApprovalGateService (invoked from each document's own Post/Approve/Reject
// actions), never through a CRUD body on this controller.
[ApiController]
[Route("[controller]")]
public class ApprovalRequestController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public ApprovalRequestController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /ApprovalRequest?status=Pending ── admin-wide view ──────────────
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        var query = _db.Set<ApprovalRequest>()
            .Include(x => x.ApprovalTemplate)
            .Include(x => x.RequestedByUser)
            .Include(x => x.DecidedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        var list = await query.OrderByDescending(x => x.RequestedAt).ToListAsync();
        return Ok(_mapper.Map<List<ApprovalRequestResponse>>(list));
    }

    // ── GET /ApprovalRequest/Mine ── pending requests the caller can decide ──
    [HttpGet("Mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var list = await _db.Set<ApprovalRequest>()
            .Include(x => x.ApprovalTemplate).ThenInclude(t => t.Approvers)
            .Include(x => x.RequestedByUser)
            .Where(x => x.Status == "Pending" && x.ApprovalTemplate.Approvers.Any(a => a.UserId == userId))
            .OrderBy(x => x.RequestedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ApprovalRequestResponse>>(list));
    }

    // ── GET /ApprovalRequest/{id} ────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var request = await _db.Set<ApprovalRequest>()
            .Include(x => x.ApprovalTemplate)
            .Include(x => x.RequestedByUser)
            .Include(x => x.DecidedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null) return NotFound();
        return Ok(_mapper.Map<ApprovalRequestResponse>(request));
    }
}

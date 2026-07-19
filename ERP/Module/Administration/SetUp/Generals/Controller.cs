using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.UserGroups;

// ⚠️ Route was "[controller]" (/UserGroup), which collided with
// FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroupController
// (also /UserGroup) and threw AmbiguousMatchException on every call.
// This is the richer RBAC groups feature (members/permissions); it now
// lives at /SecurityUserGroup. The SetUp controller keeps /UserGroup,
// which the User-creation dropdown and UserGroups CRUD pages use.
[ApiController]
[Route("SecurityUserGroup")]
public class UserGroupController : ControllerBase
{
    private readonly IUserGroupRepository _repo;
    private readonly IMapper              _mapper;
    private readonly MyDbContext          _db;

    public UserGroupController(IUserGroupRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /UserGroup ─────────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .Include(x => x.Members)
            .Include(x => x.Permissions)
            .OrderBy(x => x.GroupType == "System" ? 0 : 1)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<UserGroupResponse>>(list));
    }

    // ── GET /UserGroup/{id} ────────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Members)
            .Include(x => x.Permissions)
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<UserGroupResponse>(e));
    }

    // ── GET /UserGroup/ByUser/{userCode} ──────────────────────
    // Returns all groups a user belongs to
    [AllowAnonymous][HttpGet("ByUser/{userCode}")]
    public IActionResult GetByUser(string userCode)
    {
        var groups = _repo.GetAll()
            .Include(x => x.Members)
            .Include(x => x.Permissions)
            .Where(x => x.Members.Any(m => m.UserCode == userCode.Trim().ToUpper()))
            .ToList();
        return Ok(_mapper.Map<List<UserGroupResponse>>(groups));
    }

    // ── GET /UserGroup/{id}/CheckAccess?userCode=U001&module=Inventory&subModule=ItemMaster
    [AllowAnonymous][HttpGet("{id:int}/CheckAccess")]
    public IActionResult CheckAccess(int id,
        [FromQuery] string module, [FromQuery] string subModule)
    {
        var group = _repo.GetAll()
            .Include(x => x.Permissions)
            .FirstOrDefault(x => x.Id == id);
        if (group == null) return NotFound();

        var perm = group.Permissions
            .FirstOrDefault(p => p.Module == module && p.SubModule == subModule);

        if (perm == null)
            return Ok(new AccessCheckResponse
            {
                Module = module, SubModule = subModule,
                CanRead = false, CanCreate = false,
                CanUpdate = false, CanDelete = false,
            });

        return Ok(_mapper.Map<AccessCheckResponse>(perm));
    }

    // ── POST /UserGroup ────────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] UserGroupRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (_repo.GetAll().Any(x => x.Code == dto.Code.Trim()))
            return BadRequest($"Group code '{dto.Code}' already exists");

        var entity       = _mapper.Map<UserGroup>(dto);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<UserGroupResponse>(entity));
    }

    // ── PUT /UserGroup/{id} ────────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UserGroupRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Members)
            .Include(x => x.Permissions)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.GroupType == "System")
            return BadRequest("System groups cannot be modified");
        if (_repo.GetAll().Any(x => x.Code == dto.Code.Trim() && x.Id != id))
            return BadRequest($"Group code '{dto.Code}' already exists");

        _db.RemoveRange(entity.Members);
        _db.RemoveRange(entity.Permissions);
        _mapper.Map(dto, entity);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<UserGroupResponse>(entity));
    }

    // ── POST /UserGroup/{id}/CopyTo ───────────────────────────
    // Clone a group's permissions to a new group
    [AllowAnonymous][HttpPost("{id:int}/CopyTo")]
    public IActionResult CopyTo(int id, [FromBody] UserGroupRequest dto)
    {
        var source = _repo.GetAll()
            .Include(x => x.Permissions)
            .FirstOrDefault(x => x.Id == id);
        if (source == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Group code '{code}' already exists");

        var copy = new UserGroup
        {
            Code        = code,
            Name        = dto.Name,
            Description = dto.Description,
            GroupType   = "Custom",
            Color       = dto.Color ?? source.Color,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
            Permissions = source.Permissions.Select(p => new UserGroupPermission
            {
                Module    = p.Module,    SubModule = p.SubModule,
                CanCreate = p.CanCreate, CanRead   = p.CanRead,
                CanUpdate = p.CanUpdate, CanDelete = p.CanDelete,
                CanApprove= p.CanApprove,CanExport = p.CanExport,
            }).ToList(),
        };
        _repo.Add(copy);
        _repo.Commit();
        return Ok(_mapper.Map<UserGroupResponse>(copy));
    }

    // ── DELETE /UserGroup/{id} ─────────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.GroupType == "System")
            return BadRequest("System groups cannot be deleted");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FarmingApi.Core;
using UserEntity      = FarmingApi.Modules.Administration.SetUp.User.User;
using UserGroupEntity = FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroup;

namespace FarmingApi.Modules.Administration.Authorisations;

// ═══════════════════════════════════════════════════════════════
// AUTHORISATIONS  (route: /UserAuthorization)
// GET    /UserAuthorization           → every user + authorisation summary
// GET    /UserAuthorization/{userId}  → one user's permission tree
// PUT    /UserAuthorization/{userId}  → save it (creates on first save)
// POST   /UserAuthorization/{userId}/GrantFull → shortcut to full access
// DELETE /UserAuthorization/{userId}  → reset to nothing
// ═══════════════════════════════════════════════════════════════
public class UserAuthorizationController : MyController
{
    private readonly IUserAuthorizationRepository _repository;
    private readonly MyDbContext _db;

    public UserAuthorizationController(IUserAuthorizationRepository repository, MyDbContext db)
    {
        _repository = repository;
        _db = db;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static Dictionary<string, string> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOpts) ?? new(); }
        catch { return new(); }
    }

    // ── GET /UserAuthorization ─────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
        var users  = _db.Set<UserEntity>().ToList();
        var groups = _db.Set<UserGroupEntity>().ToDictionary(g => g.Id, g => g.GroupName);
        var auths  = _repository.GetAll().ToDictionary(a => a.UserId);

        var rows = users.Select(u =>
        {
            auths.TryGetValue(u.Id, out var a);
            var perms = a == null ? new() : Parse(a.Permissions);

            return new AuthorizationListResponse
            {
                UserId            = u.Id,
                UserCode          = u.UserId,
                UserName          = u.UserName,
                Email             = u.Email,
                GroupName         = groups.TryGetValue(u.UserGroupId, out var gn) ? gn : null,
                Configured        = a != null,
                FullAuthorization = a?.FullAuthorization ?? false,
                FullCount         = perms.Count(p => p.Value == "Full"),
                ReadCount         = perms.Count(p => p.Value == "Read"),
                NoneCount         = perms.Count(p => p.Value == "None"),
                UpdatedAt         = a?.UpdatedAt ?? a?.CreatedAt,
            };
        }).OrderBy(r => r.UserName).ToList();

        return Ok(rows);
    }

    // ── GET /UserAuthorization/{userId} ────────────────────────
    [AllowAnonymous]
    [HttpGet("{userId:int}")]
    public IActionResult Get(int userId)
    {
        var user = _db.Set<UserEntity>().FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound($"User not found: {userId}");

        var a = _repository.GetSingle(x => x.UserId == userId);
        return Ok(new AuthorizationDetailResponse
        {
            UserId            = user.Id,
            UserCode          = user.UserId,
            UserName          = user.UserName,
            Configured        = a != null,
            FullAuthorization = a?.FullAuthorization ?? false,
            Permissions       = a == null ? new() : Parse(a.Permissions),
        });
    }

    // ── PUT /UserAuthorization/{userId} ────────────────────────
    [AllowAnonymous]
    [HttpPut("{userId:int}")]
    public IActionResult Save(int userId, [FromBody] AuthorizationSaveRequest request)
    {
        var user = _db.Set<UserEntity>().FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound($"User not found: {userId}");
        if (request == null) return BadRequest("No authorisation data supplied.");

        var perms = request.Permissions ?? new();
        // Full toggle forces every listed subject to Full for a consistent record
        if (request.FullAuthorization)
            foreach (var key in perms.Keys.ToList()) perms[key] = "Full";

        var json = JsonSerializer.Serialize(perms);

        var a = _repository.GetSingle(x => x.UserId == userId);
        var isNew = a == null;
        a ??= new UserAuthorization { UserId = userId, CreatedAt = DateTime.UtcNow, InActive = false };

        a.FullAuthorization = request.FullAuthorization;
        a.Permissions       = json;

        if (isNew) _repository.Add(a);
        else { a.UpdatedAt = DateTime.UtcNow; _repository.Update(a); }

        _repository.Commit();
        return Ok(new { message = "Authorisations saved", userId, fullAuthorization = a.FullAuthorization });
    }

    // ── POST /UserAuthorization/{userId}/GrantFull ─────────────
    [AllowAnonymous]
    [HttpPost("{userId:int}/GrantFull")]
    public IActionResult GrantFull(int userId, [FromBody] AuthorizationSaveRequest? request)
    {
        var user = _db.Set<UserEntity>().FirstOrDefault(u => u.Id == userId);
        if (user == null) return NotFound($"User not found: {userId}");

        // Set every subject the caller knows about to Full (or {} if none sent)
        var perms = (request?.Permissions ?? new()).ToDictionary(p => p.Key, _ => "Full");
        var json  = JsonSerializer.Serialize(perms);

        var a = _repository.GetSingle(x => x.UserId == userId);
        var isNew = a == null;
        a ??= new UserAuthorization { UserId = userId, CreatedAt = DateTime.UtcNow, InActive = false };
        a.FullAuthorization = true;
        a.Permissions       = json;

        if (isNew) _repository.Add(a);
        else { a.UpdatedAt = DateTime.UtcNow; _repository.Update(a); }

        _repository.Commit();
        return Ok(new { message = $"Full authorisation granted to {user.UserName}", userId });
    }

    // ── DELETE /UserAuthorization/{userId} ─────────────────────
    [AllowAnonymous]
    [HttpDelete("{userId:int}")]
    public IActionResult Delete(int userId)
    {
        var a = _repository.GetSingle(x => x.UserId == userId);
        if (a == null) return NotFound($"No authorisations for user {userId}");
        _repository.Remove(a);
        _repository.Commit();
        return NoContent();
    }
}

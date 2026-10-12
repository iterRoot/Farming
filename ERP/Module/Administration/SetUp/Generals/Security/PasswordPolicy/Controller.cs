using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Modules.Administration.Security;

// SAP-B1-style "Password Administration" screen: one policy record that
// UserController reads on login / change-password (see GetPolicy() there),
// plus a read-only report of how old each user's password is.
public class PasswordPolicyController : MyController
{
    private readonly MyDbContext _db;

    public PasswordPolicyController(MyDbContext db)
    {
        _db = db;
    }

    // ── GET /PasswordPolicy ─────────────────────────────────────
    [HttpGet]
    public IActionResult Get()
    {
        var p = _db.Set<PasswordPolicy>().OrderBy(x => x.Id).FirstOrDefault();
        return Ok(p == null ? new PasswordPolicyDto() : ToDto(p));
    }

    // ── PUT /PasswordPolicy ─────────────────────────────────────
    [HttpPut]
    public IActionResult Save([FromBody] PasswordPolicyDto dto)
    {
        if (dto == null) return BadRequest("No policy supplied.");
        if (dto.MinLength < 4 || dto.MinLength > 64)
            return BadRequest("Minimum length must be between 4 and 64.");
        if (dto.MaxFailedAttempts < 1 || dto.MaxFailedAttempts > 20)
            return BadRequest("Max failed attempts must be between 1 and 20.");
        if (dto.ExpiryDays is < 1)
            return BadRequest("Expiry days must be at least 1, or left blank for never.");

        var p = _db.Set<PasswordPolicy>().OrderBy(x => x.Id).FirstOrDefault();
        var isNew = p == null;
        if (isNew) p = new PasswordPolicy { CreatedAt = DateTime.UtcNow };

        p!.MinLength         = dto.MinLength;
        p.RequireUppercase   = dto.RequireUppercase;
        p.RequireLowercase   = dto.RequireLowercase;
        p.RequireDigit       = dto.RequireDigit;
        p.RequireSpecial     = dto.RequireSpecial;
        p.MaxFailedAttempts  = dto.MaxFailedAttempts;
        p.ExpiryDays         = dto.ExpiryDays;

        if (isNew) _db.Set<PasswordPolicy>().Add(p);
        else p.UpdatedAt = DateTime.UtcNow;

        _db.SaveChanges();
        return Ok(ToDto(p));
    }

    // ── GET /PasswordPolicy/UserAges ────────────────────────────
    // Who is overdue for a change, per the ExpiryDays above. A user whose
    // PasswordChangedAt is still null predates this feature — shown as
    // "unknown" (AgeDays = null) rather than guessed at.
    [HttpGet("UserAges")]
    public IActionResult UserAges()
    {
        var policy = GetPolicy(_db);
        var now = DateTime.UtcNow;

        var rows = _db.Set<UserEntity>()
            .OrderBy(u => u.UserName)
            .Select(u => new PasswordAgeRow
            {
                Id = u.Id,
                UserId = u.UserId,
                UserName = u.UserName,
                PasswordChangedAt = u.PasswordChangedAt,
            })
            .ToList();

        foreach (var r in rows)
        {
            if (r.PasswordChangedAt == null) continue;
            r.AgeDays = (int)(now - r.PasswordChangedAt.Value).TotalDays;
            r.Expired = policy.ExpiryDays.HasValue && r.AgeDays >= policy.ExpiryDays.Value;
        }

        return Ok(rows);
    }

    // Shared with UserController so both read the exact same fallback
    // defaults — a fresh install with no saved policy behaves exactly as
    // this app always did.
    public static PasswordPolicyDto GetPolicy(MyDbContext db)
    {
        var p = db.Set<PasswordPolicy>().OrderBy(x => x.Id).FirstOrDefault();
        return p == null ? new PasswordPolicyDto() : ToDto(p);
    }

    private static PasswordPolicyDto ToDto(PasswordPolicy p) => new()
    {
        MinLength = p.MinLength,
        RequireUppercase = p.RequireUppercase,
        RequireLowercase = p.RequireLowercase,
        RequireDigit = p.RequireDigit,
        RequireSpecial = p.RequireSpecial,
        MaxFailedAttempts = p.MaxFailedAttempts,
        ExpiryDays = p.ExpiryDays,
    };
}

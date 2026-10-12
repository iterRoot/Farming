using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using System.Security.Cryptography;
using System.Text;
using UserGroupEntity = FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroup;
using ApprovalTemplateApproverEntity = FarmingApi.Modules.Administration.ApprovalProcess.ApprovalTemplateApprover;
using ApprovalRequestEntity = FarmingApi.Modules.Administration.ApprovalRequest.ApprovalRequest;
using FarmingApi.Modules.Administration.Security;

namespace FarmingApi.Modules.Administration.SetUp.User;

public class UserController : MyController
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly MyDbContext _db;
    private readonly ITotpService _totp;
    private readonly IJwtTokenService _jwt;

    public UserController(IUserRepository repository, IMapper mapper, MyDbContext db, ITotpService totp, IJwtTokenService jwt)
    {
        _mapper = mapper;
        _repository = repository;
        _db = db;
        _totp = totp;
        _jwt = jwt;
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }

    // Reads the saved Password Administration policy, falling back to the
    // rules this app always enforced when nothing has been saved yet — so
    // a database with no KPWDPOL row behaves exactly as before this existed.
    private PasswordPolicyDto GetPolicy() => PasswordPolicyController.GetPolicy(_db);

    private static bool ValidatePasswordStrength(string password, PasswordPolicyDto policy)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < policy.MinLength)
            return false;

        if (policy.RequireUppercase && !password.Any(char.IsUpper)) return false;
        if (policy.RequireLowercase && !password.Any(char.IsLower)) return false;
        if (policy.RequireDigit && !password.Any(char.IsDigit)) return false;
        if (policy.RequireSpecial && !password.Any(c => "@$!%*?&".Contains(c))) return false;

        return true;
    }

    private static string PolicyDescription(PasswordPolicyDto p)
    {
        var parts = new List<string> { $"at least {p.MinLength} characters" };
        if (p.RequireUppercase) parts.Add("an uppercase letter");
        if (p.RequireLowercase) parts.Add("a lowercase letter");
        if (p.RequireDigit) parts.Add("a number");
        if (p.RequireSpecial) parts.Add("a special character (@$!%*?&)");
        return "Password must have " + string.Join(", ", parts) + ".";
    }

    // ── POST /User/Login ───────────────────────────────────────
    // Accepts either UserId or Email. Locks the account after the
    // configured number of consecutive failures (Password Administration,
    // default 5). The one endpoint that must stay public — it's how a
    // client gets a token in the first place.
    [AllowAnonymous]
    [HttpPost("Login")]
    public IActionResult Login([FromBody] UserLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest("User ID or Email is required");
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required");

        var key = request.UserId.Trim();
        var user = _repository.GetSingle(u => u.UserId == key)
                ?? _repository.GetSingle(u => u.Email == key);

        // Same message whether the user is missing or the password is
        // wrong — don't reveal which accounts exist.
        if (user == null)
            return Unauthorized("Invalid user ID or password");

        if (user.IsLocked)
            return StatusCode(403, "Account is locked. Contact your administrator.");

        if (user.ActiveStatus != 'A')
            return StatusCode(403, "Account is inactive. Contact your administrator.");

        var maxAttempts = GetPolicy().MaxFailedAttempts;

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            user.LoginAttempts += 1;
            if (user.LoginAttempts >= maxAttempts) user.IsLocked = true;
            _repository.Update(user);
            _repository.Commit();

            return Unauthorized(user.IsLocked
                ? $"Account locked after {maxAttempts} failed attempts. Contact your administrator."
                : $"Invalid user ID or password ({maxAttempts - user.LoginAttempts} attempt(s) left)");
        }

        // ── Second factor ──────────────────────────────────────────
        if (user.TwoFactorEnabled)
        {
            // Password was right but no code supplied → ask for it.
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
                return StatusCode(428, new
                {
                    twoFactorRequired = true,
                    message = "Enter the 6-digit code from your authenticator app",
                });

            var code = request.TwoFactorCode.Trim();
            var okTotp = _totp.VerifyCode(user.TwoFactorSecret!, code);

            // Fall back to a single-use recovery code
            var okRecovery = false;
            if (!okTotp && code.Contains('-'))
            {
                var hashes = (user.TwoFactorRecoveryCodes ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                var hit = HashPassword(code.ToLowerInvariant());
                if (hashes.Remove(hit))
                {
                    okRecovery = true;
                    user.TwoFactorRecoveryCodes = string.Join(",", hashes); // burn it
                }
            }

            if (!okTotp && !okRecovery)
            {
                user.LoginAttempts += 1;
                if (user.LoginAttempts >= maxAttempts) user.IsLocked = true;
                _repository.Update(user);
                _repository.Commit();

                return Unauthorized(user.IsLocked
                    ? $"Account locked after {maxAttempts} failed attempts. Contact your administrator."
                    : $"Invalid authenticator code ({maxAttempts - user.LoginAttempts} attempt(s) left)");
            }
        }

        // Success — reset the counter and stamp the login time
        user.LoginAttempts = 0;
        user.LastLogin     = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();

        var group = _db.Set<UserGroupEntity>().FirstOrDefault(g => g.Id == user.UserGroupId);
        var (token, expiresAt) = _jwt.IssueToken(user, group?.GroupName);

        return Ok(new UserLoginResponse
        {
            Id           = user.Id,
            UserId       = user.UserId,
            UserName     = user.UserName,
            Email        = user.Email,
            UserGroupId  = user.UserGroupId,
            GroupName    = group?.GroupName,
            ActiveStatus = user.ActiveStatus,
            LastLogin    = user.LastLogin,
            Token          = token,
            TokenExpiresAt = expiresAt,
        });
    }

    // ═══════════════════════════════════════════════════════════
    // TWO-FACTOR (TOTP — Microsoft / Google Authenticator)
    // ═══════════════════════════════════════════════════════════

    // ── POST /User/{id}/TwoFactor/Setup ────────────────────────
    // Generates a secret + QR URI. Does NOT enable 2FA yet — the
    // user must prove they can produce a code first.
    [HttpPost("{id:int}/TwoFactor/Setup")]
    public IActionResult TwoFactorSetup(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");
        if (user.TwoFactorEnabled)
            return BadRequest("Two-factor is already enabled. Disable it first to re-enrol.");

        var secret = _totp.GenerateSecret();
        user.TwoFactorSecret = secret;   // provisional until verified
        _repository.Update(user);
        _repository.Commit();

        return Ok(new TwoFactorSetupResponse
        {
            Secret     = secret,
            OtpAuthUri = _totp.BuildOtpAuthUri(secret, user.Email ?? user.UserId, "Farm ERP"),
        });
    }

    // ── POST /User/{id}/TwoFactor/Enable ───────────────────────
    // Verifies the first code, switches 2FA on, returns recovery codes.
    [HttpPost("{id:int}/TwoFactor/Enable")]
    public IActionResult TwoFactorEnable(int id, [FromBody] TwoFactorVerifyRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");
        if (string.IsNullOrWhiteSpace(user.TwoFactorSecret))
            return BadRequest("Run Setup first to generate a secret.");
        if (user.TwoFactorEnabled)
            return BadRequest("Two-factor is already enabled.");

        if (!_totp.VerifyCode(user.TwoFactorSecret, request.Code ?? ""))
            return BadRequest("That code is not valid. Check your device's clock and try again.");

        var codes = _totp.GenerateRecoveryCodes();
        user.TwoFactorEnabled       = true;
        user.TwoFactorRecoveryCodes = string.Join(",", codes.Select(c => HashPassword(c)));
        user.UpdatedAt              = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();

        return Ok(new TwoFactorEnableResponse { Enabled = true, RecoveryCodes = codes });
    }

    // ── POST /User/{id}/TwoFactor/Disable ──────────────────────
    [HttpPost("{id:int}/TwoFactor/Disable")]
    public IActionResult TwoFactorDisable(int id, [FromBody] TwoFactorDisableRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");

        if (!VerifyPassword(request.Password ?? "", user.PasswordHash))
            return Unauthorized("Password is incorrect");

        user.TwoFactorEnabled       = false;
        user.TwoFactorSecret        = null;
        user.TwoFactorRecoveryCodes = null;
        user.UpdatedAt              = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();

        return Ok(new { enabled = false, message = "Two-factor authentication disabled" });
    }

    // ── GET /User/{id}/TwoFactor/Status ────────────────────────
    [HttpGet("{id:int}/TwoFactor/Status")]
    public IActionResult TwoFactorStatus(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");

        var remaining = (user.TwoFactorRecoveryCodes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

        return Ok(new { enabled = user.TwoFactorEnabled, recoveryCodesRemaining = remaining });
    }

    [HttpGet]
    public IActionResult Gets()
    {
        var iQueryable = _repository.GetAll();
        var results = _mapper.ProjectTo<UserListResponse>(iQueryable).ToList();
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
        {
            return BadRequest($"Item not found {id}");
        }
        var result = _mapper.Map<UserListResponse>(user);
        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create([FromBody] UserListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest("User ID is required");
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("User Name is required");
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            return BadRequest("Valid email is required");
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required");

        var policy = GetPolicy();
        if (!ValidatePasswordStrength(request.Password, policy))
            return BadRequest(PolicyDescription(policy));

        var existingUser = _repository.GetSingle(u => u.UserId == request.UserId);
        if (existingUser != null)
            return BadRequest("User ID already exists");

        var existingEmail = _repository.GetSingle(u => u.Email == request.Email);
        if (existingEmail != null)
            return BadRequest("Email already exists");

        var entity = _mapper.Map<User>(request);
        entity.PasswordHash = HashPassword(request.Password);
        entity.PasswordChangedAt = DateTime.UtcNow;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message = "User saved successfully",
            id = entity.Id
        });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromForm] UserUpdateRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
            return NotFound($"Item not found: {id}");

        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("User Name is required");
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            return BadRequest("Valid email is required");

        if (user.Email != request.Email)
        {
            var existingEmail = _repository.GetSingle(u => u.Email == request.Email && u.Id != id);
            if (existingEmail != null)
                return BadRequest("Email already exists");
        }

        _mapper.Map(request, user);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var policy = GetPolicy();
            if (!ValidatePasswordStrength(request.Password, policy))
                return BadRequest(PolicyDescription(policy));
            user.PasswordHash = HashPassword(request.Password);
            user.PasswordChangedAt = DateTime.UtcNow;
        }

        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        _repository.Commit();

        return NoContent();
    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
        {
            return BadRequest($"Item not found {id}");
        }

        // ApprovalTemplateApprover and ApprovalRequest both hold Restrict-behavior
        // FKs to User (added with the approval engine) — deleting a user who is or
        // was involved in an approval would otherwise throw an unhandled
        // DbUpdateException, the same bug class fixed in ApprovalProcessController.
        var isApprover = _db.Set<ApprovalTemplateApproverEntity>().Any(a => a.UserId == id);
        if (isApprover)
            return BadRequest("Cannot delete a user assigned as an approver on an approval template — remove them from the template first.");

        var hasApprovalHistory = _db.Set<ApprovalRequestEntity>().Any(r => r.RequestedBy == id || r.DecidedBy == id);
        if (hasApprovalHistory)
            return BadRequest("Cannot delete a user with approval history (submitted or decided an approval request) — deactivate them instead.");

        user.DeletedAt = DateTime.UtcNow;
        _repository.Remove(user);
        _repository.Commit();
        return NoContent();
    }

    [HttpPost("{id}/ChangePassword")]
    public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
            return NotFound($"User not found: {id}");

        if (!VerifyPassword(request.OldPassword, user.PasswordHash))
            return BadRequest("Current password is incorrect");

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("New passwords do not match");

        var policy = GetPolicy();
        if (!ValidatePasswordStrength(request.NewPassword, policy))
            return BadRequest(PolicyDescription(policy));

        user.PasswordHash = HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        _repository.Commit();

        return NoContent();
    }
}
using Microsoft.EntityFrameworkCore;
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
    private readonly SecurityAudit _audit;
    private readonly IConfiguration _config;

    public UserController(IUserRepository repository, IMapper mapper, MyDbContext db, ITotpService totp, IJwtTokenService jwt, SecurityAudit audit, IConfiguration config)
    {
        _audit = audit;
        _config = config;
        _mapper = mapper;
        _repository = repository;
        _db = db;
        _totp = totp;
        _jwt = jwt;
    }

    private string HashPassword(string password) => PasswordHashing.Hash(password);

    // Recovery codes are long random values, so plain SHA-256 is enough for them (see PasswordHashing).
    private string HashCode(string value) => PasswordHashing.Hash256(value);

    private bool VerifyPassword(string password, string hash) => PasswordHashing.Verify(password, hash, out _);

    // Every sign-in failure says the same thing, whether the user exists, is locked or inactive, or the password was
    // wrong, so the form can't be used to find out which accounts exist. Status is only revealed to someone who has
    // proved they know the password.
    private const string GenericLoginFailure = "Invalid user ID or password. After repeated failures an account is locked for a while.";

    // Checked against when the user doesn't exist, so an unknown ID costs the same time as a real one.
    private static readonly string DummyHash = PasswordHashing.Hash("not-a-real-password-" + Guid.NewGuid());

    // How long a lockout from failed attempts lasts before the account opens again by itself.
    // An administrator can always unlock sooner (POST /User/{id}/Unlock).
    private int LockoutMinutes => _config.GetValue<int?>("Security:LockoutMinutes") ?? 15;

    // Takes one attempt BEFORE the password is checked, with a single UPDATE that returns the new count. Parallel guesses
    // each get a different number, and any beyond the limit are refused without being evaluated — counting only
    // after a wrong answer let a burst of simultaneous guesses all run first.
    private int ReserveAttempt(User user)
    {
        var set = _db.Set<User>().Where(u => u.Id == user.Id);
        set.ExecuteUpdate(s => s.SetProperty(u => u.LoginAttempts, u => u.LoginAttempts + 1));
        return set.Select(u => u.LoginAttempts).First();
    }

    // True when this step is newer than any accepted before. The UPDATE is the check, so it's safe under parallel requests.
    private bool ClaimTotpStep(User user, long step)
    {
        var claimed = _db.Set<User>().Where(u => u.Id == user.Id && (u.TwoFactorLastStep == null || u.TwoFactorLastStep < step))
            .ExecuteUpdate(s => s.SetProperty(u => u.TwoFactorLastStep, step)) > 0;
        if (claimed) user.TwoFactorLastStep = step;
        return claimed;
    }

    private void ReleaseAttempt(User user) =>
        _db.Set<User>().Where(u => u.Id == user.Id && u.LoginAttempts > 0)
            .ExecuteUpdate(s => s.SetProperty(u => u.LoginAttempts, u => u.LoginAttempts - 1));

    private void LockNow(User user)
    {
        var now = DateTime.UtcNow; var until = now.AddMinutes(LockoutMinutes);
        _db.Set<User>().Where(u => u.Id == user.Id)
            .ExecuteUpdate(s => s.SetProperty(u => u.IsLocked, true).SetProperty(u => u.LockedUntil, until).SetProperty(u => u.SessionsValidFrom, now));
        user.IsLocked = true;
    }

    // Counts one failed attempt with a single UPDATE (read-modify-write let parallel guesses slip past
    // the limit), locks the account when the limit is reached and ends the user's sessions.
    private (int Attempts, bool Locked) RegisterFailure(User user, int maxAttempts)
    {
        var set = _db.Set<User>().Where(u => u.Id == user.Id);
        set.ExecuteUpdate(s => s.SetProperty(u => u.LoginAttempts, u => u.LoginAttempts + 1));
        var attempts = set.Select(u => u.LoginAttempts).First();
        var locked = attempts >= maxAttempts;
        if (locked)
        {
            var now = DateTime.UtcNow;
            var until = now.AddMinutes(LockoutMinutes);
            set.ExecuteUpdate(s => s.SetProperty(u => u.IsLocked, true)
                                    .SetProperty(u => u.LockedUntil, until)
                                    .SetProperty(u => u.SessionsValidFrom, now));
        }
        user.LoginAttempts = attempts;
        user.IsLocked = locked || user.IsLocked;
        return (attempts, locked);
    }

    // Reads the saved Password Administration policy, falling back to the
    // rules this app always enforced when nothing has been saved yet — so
    // a database with no KPWDPOL row behaves exactly as before this existed.
    private PasswordPolicyDto GetPolicy() => PasswordPolicyController.GetPolicy(_db);

    // null = acceptable; otherwise the reason (see Core/PasswordRules.cs)
    private static string? PasswordProblem(string? password, PasswordPolicyDto p, string? userId, string? email) =>
        PasswordRules.Check(password, new PasswordPolicyView(p.MinLength, p.RequireUppercase, p.RequireLowercase, p.RequireDigit, p.RequireSpecial), userId, email);

    // ── POST /User/Login ───────────────────────────────────────
    // Accepts either UserId or Email. Locks the account after the
    // configured number of consecutive failures (Password Administration,
    // default 5). The one endpoint that must stay public — it's how a
    // client gets a token in the first place.
    [AllowAnonymous]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("login")]
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
        {
            PasswordHashing.Verify(request.Password, DummyHash, out _);   // same work as a real account
            _audit.LoginFailed(key, "unknown-user");
            return Unauthorized(GenericLoginFailure);
        }

        if (user.IsLocked && user.LockedUntil is DateTime lockEnds && lockEnds.ToUniversalTime() <= DateTime.UtcNow)
        {
            // A timed lockout has run out: open the account again.
            user.IsLocked = false; user.LockedUntil = null; user.LoginAttempts = 0;
            _repository.Update(user);
            _repository.Commit();
        }

        if (user.IsLocked || user.ActiveStatus != 'A')
        {
            // Check the password without counting an attempt; only the real owner (right password) is told why.
            var owner = PasswordHashing.Verify(request.Password, user.PasswordHash, out _);
            _audit.LoginFailed(user.UserId, user.IsLocked ? "account-locked" : "account-inactive");
            if (!owner) return Unauthorized(GenericLoginFailure);
            return StatusCode(403, user.IsLocked
                ? (user.LockedUntil == null
                    ? "Account is locked. Contact your administrator."
                    : "Account is temporarily locked after too many failed attempts. Try again later or contact your administrator.")
                : "Account is inactive. Contact your administrator.");
        }

        var maxAttempts = GetPolicy().MaxFailedAttempts;

        var attemptNo = ReserveAttempt(user);
        if (attemptNo > maxAttempts)
        {
            // Other guesses in flight already used up the allowance: don't evaluate this one.
            LockNow(user);
            _audit.LoginFailed(user.UserId, "too-many-attempts");
            _audit.AccountLocked(user.UserId, "sign-in");
            return Unauthorized(GenericLoginFailure);
        }

        if (!PasswordHashing.Verify(request.Password, user.PasswordHash, out var upgradeHash))
        {
            var attempts = attemptNo;
            var locked = attempts >= maxAttempts;
            if (locked) LockNow(user);
            user.LoginAttempts = attempts;

            _audit.LoginFailed(user.UserId, "wrong-password", Math.Max(0, maxAttempts - attempts));
            if (locked) _audit.AccountLocked(user.UserId, "sign-in");

            return Unauthorized(GenericLoginFailure);
        }

        // ── Second factor ──────────────────────────────────────────
        if (user.TwoFactorEnabled)
        {
            // Password was right but no code supplied → ask for it.
            if (string.IsNullOrWhiteSpace(request.TwoFactorCode))
            {
                ReleaseAttempt(user);   // the password was right; asking for the code isn't a failed guess
                _audit.TwoFactorChallenge(user.UserId);
                return StatusCode(428, new
                {
                    twoFactorRequired = true,
                    message = "Enter the 6-digit code from your authenticator app",
                });
            }

            var code = request.TwoFactorCode.Trim();
            // A code is good once: the step it belongs to must be newer than the last one accepted (claimed atomically,
            // so two simultaneous sign-ins can't both use it).
            var okTotp = _totp.TryVerify(user.TwoFactorSecret!, code, out var step) && ClaimTotpStep(user, step);

            // Fall back to a single-use recovery code
            var okRecovery = false;
            if (!okTotp && code.Contains('-'))
            {
                var hashes = (user.TwoFactorRecoveryCodes ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
                var hit = HashCode(code.ToLowerInvariant());
                if (hashes.Remove(hit))
                {
                    okRecovery = true;
                    user.TwoFactorRecoveryCodes = string.Join(",", hashes); // burn it
                }
            }

            if (!okTotp && !okRecovery)
            {
                var attempts2 = attemptNo;
                var locked2 = attempts2 >= maxAttempts;
                if (locked2) LockNow(user);
                user.LoginAttempts = attempts2;

                _audit.LoginFailed(user.UserId, "wrong-2fa-code", Math.Max(0, maxAttempts - attempts2));
                if (locked2) _audit.AccountLocked(user.UserId, "2fa code");

                return Unauthorized("Invalid authenticator code.");
            }
        }

        // Success — reset the counter and stamp the login time
        user.LoginAttempts = 0;
        user.LockedUntil   = null;
        user.LastLogin     = DateTime.UtcNow;
        if (upgradeHash) user.PasswordHash = HashPassword(request.Password);   // old SHA-256 → salted PBKDF2, no reset needed
        _repository.Update(user);
        _repository.Commit();

        var group = _db.Set<UserGroupEntity>().FirstOrDefault(g => g.Id == user.UserGroupId);
        var (token, expiresAt) = _jwt.IssueToken(user, group?.GroupName);
        _audit.LoginSucceeded(user.UserId, usedRecoveryCode: user.TwoFactorEnabled && !string.IsNullOrWhiteSpace(request.TwoFactorCode) && request.TwoFactorCode.Contains('-'));

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
        _audit.Changed("2fa.setup", user.UserId);

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

        if (!(_totp.TryVerify(user.TwoFactorSecret, request.Code ?? "", out var enableStep) && ClaimTotpStep(user, enableStep)))
        {
            _audit.ChangeRefused("2fa.enable", user.UserId, "wrong code");
            return BadRequest("That code is not valid. Check your device's clock and try again.");
        }

        var codes = _totp.GenerateRecoveryCodes();
        user.TwoFactorEnabled       = true;
        user.TwoFactorRecoveryCodes = string.Join(",", codes.Select(c => HashCode(c)));
        user.UpdatedAt              = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();
        _audit.Changed("2fa.enable", user.UserId);

        return Ok(new TwoFactorEnableResponse { Enabled = true, RecoveryCodes = codes });
    }

    // ── POST /User/{id}/TwoFactor/Disable ──────────────────────
    [HttpPost("{id:int}/TwoFactor/Disable")]
    public IActionResult TwoFactorDisable(int id, [FromBody] TwoFactorDisableRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");

        if (!VerifyPassword(request.Password ?? "", user.PasswordHash))
        {
            _audit.ChangeRefused("2fa.disable", user.UserId, "wrong password");
            return Unauthorized("Password is incorrect");
        }

        user.TwoFactorEnabled       = false;
        user.TwoFactorSecret        = null;
        user.TwoFactorRecoveryCodes = null;
        user.UpdatedAt              = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();
        _audit.Changed("2fa.disable", user.UserId);

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
        var pwProblem = PasswordProblem(request.Password, policy, request.UserId, request.Email);
        if (pwProblem != null) return BadRequest(pwProblem);

        if (!_db.Set<UserGroupEntity>().Any(g => g.Id == request.UserGroupId))
            return BadRequest("The selected user group doesn't exist");

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

        // Start with no access. Without a record the permission filter denies everything, but an explicit
        // record shows up (as "None") on the Authorisations screen for an administrator to configure.
        _db.Set<FarmingApi.Modules.Administration.Authorisations.UserAuthorization>().Add(new()
        {
            UserId = entity.Id, FullAuthorization = false, Permissions = "{}", CreatedAt = DateTime.UtcNow, InActive = false,
        });
        _db.SaveChanges();
        _audit.Changed("user.create", entity.UserId, $"group {entity.UserGroupId}; no access until Authorisations are set");

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

        if (!_db.Set<UserGroupEntity>().Any(g => g.Id == request.UserGroupId))
            return BadRequest("The selected user group doesn't exist");

        var before = (user.UserGroupId, user.ActiveStatus, user.Email);
        _mapper.Map(request, user);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var policy = GetPolicy();
            var pwProblem = PasswordProblem(request.Password, policy, user.UserId, request.Email);
            if (pwProblem != null) return BadRequest(pwProblem);
            user.PasswordHash = HashPassword(request.Password);
            user.PasswordChangedAt = DateTime.UtcNow;
        }

        user.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Password) || user.ActiveStatus != 'A')
            user.SessionsValidFrom = DateTime.UtcNow;   // a reset password or a deactivation ends any open session

        _repository.Update(user);
        _repository.Commit();

        var changes = new List<string>();
        if (before.UserGroupId != user.UserGroupId) changes.Add($"group {before.UserGroupId}->{user.UserGroupId}");
        if (before.ActiveStatus != user.ActiveStatus) changes.Add($"status {before.ActiveStatus}->{user.ActiveStatus}");
        if (before.Email != user.Email) changes.Add("email changed");
        if (!string.IsNullOrWhiteSpace(request.Password)) changes.Add("password set by administrator");
        _audit.Changed("user.update", user.UserId, changes.Count > 0 ? string.Join(", ", changes) : "profile");

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
        _audit.Changed("user.delete", user.UserId);
        return NoContent();
    }

    // ── POST /User/{id}/Unlock ─────────────────────────────────
    // An administrator opens a locked account (lockouts from failed attempts also end by themselves).
    [HttpPost("{id:int}/Unlock")]
    public IActionResult Unlock(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");

        user.IsLocked = false;
        user.LockedUntil = null;
        user.LoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();
        _audit.Changed("user.unlock", user.UserId);

        return Ok(new { message = $"{user.UserId} unlocked", id });
    }

    // ── POST /User/Logout ──────────────────────────────────────
    // Ends every token issued to the caller so far; a copied token stops working too.
    [HttpPost("Logout")]
    public IActionResult Logout()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id)) return Unauthorized();
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return Unauthorized();

        user.SessionsValidFrom = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();
        _audit.Changed("logout", user.UserId);
        return NoContent();
    }

    [HttpPost("{id}/ChangePassword")]
    public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
            return NotFound($"User not found: {id}");

        if (user.IsLocked && user.LockedUntil is DateTime lockEnds2 && lockEnds2.ToUniversalTime() <= DateTime.UtcNow)
        { user.IsLocked = false; user.LockedUntil = null; user.LoginAttempts = 0; }
        if (user.IsLocked)
            return StatusCode(403, "Account is locked. Contact your administrator.");

        // A wrong current password counts toward the same lockout as a failed login, so this
        // endpoint (open to every user for their own account) can't be used to guess it.
        if (!VerifyPassword(request.OldPassword ?? "", user.PasswordHash))
        {
            var maxAttempts = GetPolicy().MaxFailedAttempts;
            var (attempts3, locked3) = RegisterFailure(user, maxAttempts);

            _audit.ChangeRefused("password.change", user.UserId, "wrong current password");
            if (locked3) _audit.AccountLocked(user.UserId, "change password");

            return BadRequest(user.IsLocked
                ? $"Account locked after {maxAttempts} failed attempts. Contact your administrator."
                : $"Current password is incorrect ({maxAttempts - user.LoginAttempts} attempt(s) left)");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest("New password is required");

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("New passwords do not match");

        var policy = GetPolicy();
        var pwProblem = PasswordProblem(request.NewPassword, policy, user.UserId, user.Email);
        if (pwProblem != null) return BadRequest(pwProblem);
        if (request.NewPassword == request.OldPassword) return BadRequest("The new password must be different from the current one.");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        user.LoginAttempts = 0;
        user.SessionsValidFrom = DateTime.UtcNow;   // every token issued before the change stops working

        _repository.Update(user);
        _repository.Commit();
        _audit.Changed("password.change", user.UserId);

        return NoContent();
    }
}
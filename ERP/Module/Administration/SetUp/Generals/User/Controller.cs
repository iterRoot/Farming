using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using System.Security.Cryptography;
using System.Text;
using UserGroupEntity = FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroup;

namespace FarmingApi.Modules.Administration.SetUp.User;

public class UserController : MyController
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;
    private readonly MyDbContext _db;
    private readonly ITotpService _totp;

    public UserController(IUserRepository repository, IMapper mapper, MyDbContext db, ITotpService totp)
    {
        _mapper = mapper;
        _repository = repository;
        _db = db;
        _totp = totp;
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

    private bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(c => char.IsUpper(c));
        bool hasLower = password.Any(c => char.IsLower(c));
        bool hasDigit = password.Any(c => char.IsDigit(c));
        bool hasSpecial = password.Any(c => "@$!%*?&".Contains(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    // ── POST /User/Login ───────────────────────────────────────
    // Accepts either UserId or Email. Locks the account after 5
    // consecutive failures.
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

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            user.LoginAttempts += 1;
            if (user.LoginAttempts >= 5) user.IsLocked = true;
            _repository.Update(user);
            _repository.Commit();

            return Unauthorized(user.IsLocked
                ? "Account locked after 5 failed attempts. Contact your administrator."
                : $"Invalid user ID or password ({5 - user.LoginAttempts} attempt(s) left)");
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
                if (user.LoginAttempts >= 5) user.IsLocked = true;
                _repository.Update(user);
                _repository.Commit();

                return Unauthorized(user.IsLocked
                    ? "Account locked after 5 failed attempts. Contact your administrator."
                    : $"Invalid authenticator code ({5 - user.LoginAttempts} attempt(s) left)");
            }
        }

        // Success — reset the counter and stamp the login time
        user.LoginAttempts = 0;
        user.LastLogin     = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();

        var group = _db.Set<UserGroupEntity>().FirstOrDefault(g => g.Id == user.UserGroupId);

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
        });
    }

    // ═══════════════════════════════════════════════════════════
    // TWO-FACTOR (TOTP — Microsoft / Google Authenticator)
    // ═══════════════════════════════════════════════════════════

    // ── POST /User/{id}/TwoFactor/Setup ────────────────────────
    // Generates a secret + QR URI. Does NOT enable 2FA yet — the
    // user must prove they can produce a code first.
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    [AllowAnonymous]
    [HttpGet("{id:int}/TwoFactor/Status")]
    public IActionResult TwoFactorStatus(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound($"User not found: {id}");

        var remaining = (user.TwoFactorRecoveryCodes ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

        return Ok(new { enabled = user.TwoFactorEnabled, recoveryCodesRemaining = remaining });
    }

    [AllowAnonymous]
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

        if (!ValidatePasswordStrength(request.Password))
            return BadRequest("Password must be at least 8 characters with uppercase, lowercase, number, and special character (@$!%*?&)");

        var existingUser = _repository.GetSingle(u => u.UserId == request.UserId);
        if (existingUser != null)
            return BadRequest("User ID already exists");

        var existingEmail = _repository.GetSingle(u => u.Email == request.Email);
        if (existingEmail != null)
            return BadRequest("Email already exists");

        var entity = _mapper.Map<User>(request);
        entity.PasswordHash = HashPassword(request.Password);
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
            if (!ValidatePasswordStrength(request.Password))
                return BadRequest("Password does not meet complexity requirements");
            user.PasswordHash = HashPassword(request.Password);
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

        if (!ValidatePasswordStrength(request.NewPassword))
            return BadRequest("New password does not meet complexity requirements");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        _repository.Commit();

        return NoContent();
    }
}
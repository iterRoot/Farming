using System;

namespace FarmingApi.Modules.Administration.SetUp.User;

public class UserListResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } =null!;
    public string UserName { get; set; } =null!;
    public string? Email { get; set; }
    public int UserGroupId { get; set; }
    public string? Phone { get; set; }
    public char ActiveStatus { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool InActive { get; set; }
}

public class UserListRequest
{
    public string UserId { get; set; } =null!;
    public string UserName { get; set; } =null!;
    public string? Email { get; set; }
    public string Password { get; set; } =null!;
    public int UserGroupId { get; set; }
    public string? Phone { get; set; }
    public char ActiveStatus { get; set; } = 'A';
}

public class UserUpdateRequest
{
    public string UserName { get; set; } =null!;
    public string? Email { get; set; }
    public int UserGroupId { get; set; }
    public string? Phone { get; set; }
    public char ActiveStatus { get; set; } = 'A';
    public string? Password { get; set; }
}

public class UserLoginRequest
{
    /// <summary>User ID or Email.</summary>
    public string UserId { get; set; } = null!;
    public string Password { get; set; } = null!;
    /// <summary>6-digit authenticator code, or a recovery code. Required only when 2FA is enabled.</summary>
    public string? TwoFactorCode { get; set; }
}

// ── Two-factor DTOs ────────────────────────────────────────────────
public class TwoFactorSetupResponse
{
    /// <summary>Base32 secret — show for manual entry.</summary>
    public string Secret     { get; set; } = null!;
    /// <summary>otpauth:// URI to render as a QR code.</summary>
    public string OtpAuthUri { get; set; } = null!;
}

public class TwoFactorVerifyRequest
{
    public string Code { get; set; } = null!;
}

public class TwoFactorEnableResponse
{
    public bool         Enabled       { get; set; }
    /// <summary>Shown ONCE — store securely, each is single-use.</summary>
    public List<string> RecoveryCodes { get; set; } = new();
}

public class TwoFactorDisableRequest
{
    /// <summary>Current password, required to turn 2FA off.</summary>
    public string Password { get; set; } = null!;
}

public class UserLoginResponse
{
    public int      Id          { get; set; }
    public string   UserId      { get; set; } = null!;
    public string   UserName    { get; set; } = null!;
    public string?  Email       { get; set; }
    public int      UserGroupId { get; set; }
    public string?  GroupName   { get; set; }
    public char     ActiveStatus{ get; set; }
    public DateTime? LastLogin  { get; set; }
}

public class ChangePasswordRequest
{
    public string? OldPassword { get; set; }
    public string ?NewPassword { get; set; }
    public string ?ConfirmPassword { get; set; }
}
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

public class ChangePasswordRequest
{
    public string? OldPassword { get; set; }
    public string ?NewPassword { get; set; }
    public string ?ConfirmPassword { get; set; }
}
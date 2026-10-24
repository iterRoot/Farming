namespace FarmingApi.Modules.Administration.UserGroups;

// ── Responses ─────────────────────────────────────────────────
public class UserGroupResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Description { get; set; }
    public string   GroupType   { get; set; } = null!;
    public string?  Color       { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
    public List<UserGroupMemberResponse>     Members     { get; set; } = new();
    public List<UserGroupPermissionResponse> Permissions { get; set; } = new();
}

public class UserGroupMemberResponse
{
    public int     Id           { get; set; }
    public string  UserCode     { get; set; } = null!;
    public string  FullName     { get; set; } = null!;
    public string? Email        { get; set; }
    public bool    IsGroupAdmin { get; set; }
}

public class UserGroupPermissionResponse
{
    public int     Id         { get; set; }
    public string  Module     { get; set; } = null!;
    public string  SubModule  { get; set; } = null!;
    public bool    CanCreate  { get; set; }
    public bool    CanRead    { get; set; }
    public bool    CanUpdate  { get; set; }
    public bool    CanDelete  { get; set; }
    public bool    CanApprove { get; set; }
    public bool    CanExport  { get; set; }
}

// ── Requests ──────────────────────────────────────────────────
public class UserGroupRequest
{
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Description { get; set; }
    public string   GroupType   { get; set; } = "Custom";
    public string?  Color       { get; set; }
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
    public List<UserGroupMemberRequest>     Members     { get; set; } = new();
    public List<UserGroupPermissionRequest> Permissions { get; set; } = new();
}

public class UserGroupMemberRequest
{
    public string  UserCode     { get; set; } = null!;
    public string  FullName     { get; set; } = null!;
    public string? Email        { get; set; }
    public bool    IsGroupAdmin { get; set; } = false;
}

public class UserGroupPermissionRequest
{
    public string  Module     { get; set; } = null!;
    public string  SubModule  { get; set; } = null!;
    public bool    CanCreate  { get; set; } = false;
    public bool    CanRead    { get; set; } = true;
    public bool    CanUpdate  { get; set; } = false;
    public bool    CanDelete  { get; set; } = false;
    public bool    CanApprove { get; set; } = false;
    public bool    CanExport  { get; set; } = false;
}

// Module access check response
public class AccessCheckResponse
{
    public string Module    { get; set; } = null!;
    public string SubModule { get; set; } = null!;
    public bool   CanCreate  { get; set; }
    public bool   CanRead    { get; set; }
    public bool   CanUpdate  { get; set; }
    public bool   CanDelete  { get; set; }
    public bool   CanApprove { get; set; }
    public bool   CanExport  { get; set; }
}
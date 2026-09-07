namespace FarmingApi.Modules.Administration.Authorisations;

// Summary row for the List
public class AuthorizationListResponse
{
    public int     UserId            { get; set; }
    public string  UserCode          { get; set; } = null!;
    public string  UserName          { get; set; } = null!;
    public string? Email             { get; set; }
    public string? GroupName         { get; set; }
    public bool    Configured        { get; set; }   // has an authorisation record
    public bool    FullAuthorization { get; set; }
    public int     FullCount         { get; set; }
    public int     ReadCount         { get; set; }
    public int     NoneCount         { get; set; }
    public DateTime? UpdatedAt       { get; set; }
}

// Full detail for one user (Create prefill / Update load)
public class AuthorizationDetailResponse
{
    public int    UserId            { get; set; }
    public string UserCode          { get; set; } = null!;
    public string UserName          { get; set; } = null!;
    public bool   Configured        { get; set; }
    public bool   FullAuthorization { get; set; }
    /// <summary>{ subjectKey: "Full" | "Read" | "None" }</summary>
    public Dictionary<string, string> Permissions { get; set; } = new();
}

public class AuthorizationSaveRequest
{
    public bool FullAuthorization { get; set; }
    public Dictionary<string, string> Permissions { get; set; } = new();
}

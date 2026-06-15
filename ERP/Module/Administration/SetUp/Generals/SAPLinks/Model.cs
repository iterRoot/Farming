namespace FarmingApi.Modules.Administration.SAPLinks;

// ── Response ──────────────────────────────────────────────────
public class SAPLinkResponse
{
    public int      Id              { get; set; }
    public string   Code            { get; set; } = null!;
    public string   Title           { get; set; } = null!;
    public string   LinkType        { get; set; } = null!;
    public string   Category        { get; set; } = null!;
    public string?  SubCategory     { get; set; }
    public string   Url             { get; set; } = null!;
    public string?  IconName        { get; set; }
    public string?  IconUrl         { get; set; }
    public string?  BadgeText       { get; set; }
    public string   Target          { get; set; } = null!;
    public bool     IsEmbeddable    { get; set; }
    public string?  HttpMethod      { get; set; }
    public string?  AuthType        { get; set; }
    public string?  ApiKeyHeader    { get; set; }
    public string?  SamplePayload   { get; set; }
    public string?  ContentType     { get; set; }
    public string?  LinkedModule    { get; set; }
    public string?  LinkedDocType   { get; set; }
    public string?  Description     { get; set; }
    public string?  Tags            { get; set; }
    public string?  Notes           { get; set; }
    public bool     IsPublic        { get; set; }
    public string?  AllowedRoles    { get; set; }
    public bool     RequiresVPN     { get; set; }
    public bool     RequiresLogin   { get; set; }
    public string?  LoginHint       { get; set; }
    public bool     IsPinned        { get; set; }
    public bool     IsOnDashboard   { get; set; }
    public int      SortOrder       { get; set; }
    public int      ClickCount      { get; set; }
    public DateTime? LastClickedAt  { get; set; }
    public string?  LastStatusCode  { get; set; }
    public DateTime? LastCheckedAt  { get; set; }
    public bool     IsHealthy       { get; set; }
    public bool     IsActive        { get; set; }
    public DateTime? ExpiresAt      { get; set; }
    public string?  Remarks         { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
    // Computed
    public bool     IsExpired       { get; set; }
}

public class SAPLinkUsageResponse
{
    public int      Id           { get; set; }
    public DateTime ClickedAt    { get; set; }
    public string?  UserCode     { get; set; }
    public string?  UserName     { get; set; }
    public string?  IpAddress    { get; set; }
    public string?  ResponseCode { get; set; }
}

// ── Request ───────────────────────────────────────────────────
public class SAPLinkRequest
{
    public string   Code            { get; set; } = null!;
    public string   Title           { get; set; } = null!;
    public string   LinkType        { get; set; } = "URL";
    public string   Category        { get; set; } = "Custom";
    public string?  SubCategory     { get; set; }
    public string   Url             { get; set; } = null!;
    public string?  IconName        { get; set; }
    public string?  IconUrl         { get; set; }
    public string?  BadgeText       { get; set; }
    public string   Target          { get; set; } = "_blank";
    public bool     IsEmbeddable    { get; set; } = false;
    public string?  HttpMethod      { get; set; }
    public string?  AuthType        { get; set; }
    public string?  ApiKeyHeader    { get; set; }
    public string?  SamplePayload   { get; set; }
    public string?  ContentType     { get; set; }
    public string?  LinkedModule    { get; set; }
    public string?  LinkedDocType   { get; set; }
    public string?  Description     { get; set; }
    public string?  Tags            { get; set; }
    public string?  Notes           { get; set; }
    public bool     IsPublic        { get; set; } = true;
    public string?  AllowedRoles    { get; set; }
    public bool     RequiresVPN     { get; set; } = false;
    public bool     RequiresLogin   { get; set; } = false;
    public string?  LoginHint       { get; set; }
    public bool     IsPinned        { get; set; } = false;
    public bool     IsOnDashboard   { get; set; } = false;
    public int      SortOrder       { get; set; } = 0;
    public bool     IsActive        { get; set; } = true;
    public DateTime? ExpiresAt      { get; set; }
    public string?  Remarks         { get; set; }
}

// Click tracking
public class RecordClickRequest
{
    public string?  UserCode  { get; set; }
    public string?  UserName  { get; set; }
    public string?  IpAddress { get; set; }
    public string?  UserAgent { get; set; }
    public string?  Referrer  { get; set; }
}

// Health check result
public class LinkHealthResult
{
    public int     Id          { get; set; }
    public string  Code        { get; set; } = null!;
    public string  Title       { get; set; } = null!;
    public string  Url         { get; set; } = null!;
    public bool    IsReachable { get; set; }
    public int?    StatusCode  { get; set; }
    public int?    ResponseMs  { get; set; }
    public string? Error       { get; set; }
}

// Summary / stats
public class SAPLinkSummary
{
    public int     Total           { get; set; }
    public int     Active          { get; set; }
    public int     Pinned          { get; set; }
    public int     OnDashboard     { get; set; }
    public int     Unhealthy       { get; set; }
    public long    TotalClicks     { get; set; }
    public Dictionary<string, int>  ByCategory { get; set; } = new();
    public Dictionary<string, int>  ByType     { get; set; } = new();
    public List<SAPLinkResponse>    TopLinks   { get; set; } = new();
}
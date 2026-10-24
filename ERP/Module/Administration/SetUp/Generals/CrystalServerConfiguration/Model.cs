namespace FarmingApi.Modules.Administration.CrystalServer;

public class CrystalServerConfigResponse
{
    public int     Id             { get; set; }
    public string  ConfigName     { get; set; } = null!;
    // Server
    public string? Host           { get; set; }
    public int     Port           { get; set; }
    public string  Protocol       { get; set; } = null!;
    public string? VirtualDir     { get; set; }
    // Auth
    public string? Username       { get; set; }
    public string? Password       { get; set; }   // masked on read
    public string  AuthType       { get; set; } = null!;
    // DB
    public string? DbHost         { get; set; }
    public int?    DbPort         { get; set; }
    public string? DbName         { get; set; }
    public string? DbUser         { get; set; }
    public string? DbPassword     { get; set; }   // masked on read
    public string  DbProvider     { get; set; } = null!;
    // Runtime
    public int     TimeoutSeconds { get; set; }
    public string? ReportFolder   { get; set; }
    public string? OutputFolder   { get; set; }
    public string  DefaultFormat  { get; set; } = null!;
    public int     MaxRows        { get; set; }
    public bool    CacheEnabled   { get; set; }
    public int     CacheMinutes   { get; set; }
    // SMTP
    public string? SmtpHost       { get; set; }
    public int?    SmtpPort       { get; set; }
    public string? SmtpUser       { get; set; }
    public string? SmtpPassword   { get; set; }   // masked on read
    public bool    SmtpUseSsl     { get; set; }
    public string? SmtpFromEmail  { get; set; }
    public string? SmtpFromName   { get; set; }
    // Status
    public bool    IsEnabled      { get; set; }
    public string? Remarks        { get; set; }
    public DateTime UpdatedAt     { get; set; }
}

public class CrystalServerConfigRequest
{
    public string  ConfigName     { get; set; } = "Default";
    // Server
    public string? Host           { get; set; }
    public int     Port           { get; set; } = 6405;
    public string  Protocol       { get; set; } = "http";
    public string? VirtualDir     { get; set; }
    // Auth
    public string? Username       { get; set; }
    public string? Password       { get; set; }
    public string  AuthType       { get; set; } = "Enterprise";
    // DB
    public string? DbHost         { get; set; }
    public int?    DbPort         { get; set; }
    public string? DbName         { get; set; }
    public string? DbUser         { get; set; }
    public string? DbPassword     { get; set; }
    public string  DbProvider     { get; set; } = "PostgreSQL";
    // Runtime
    public int     TimeoutSeconds { get; set; } = 60;
    public string? ReportFolder   { get; set; }
    public string? OutputFolder   { get; set; }
    public string  DefaultFormat  { get; set; } = "PDF";
    public int     MaxRows        { get; set; } = 50000;
    public bool    CacheEnabled   { get; set; } = true;
    public int     CacheMinutes   { get; set; } = 15;
    // SMTP
    public string? SmtpHost       { get; set; }
    public int?    SmtpPort       { get; set; }
    public string? SmtpUser       { get; set; }
    public string? SmtpPassword   { get; set; }
    public bool    SmtpUseSsl     { get; set; } = true;
    public string? SmtpFromEmail  { get; set; }
    public string? SmtpFromName   { get; set; }
    // Status
    public bool    IsEnabled      { get; set; } = true;
    public string? Remarks        { get; set; }
}

// Used by ping/test-connection endpoint
public class ConnectionTestResult
{
    public bool    Success  { get; set; }
    public string  Message  { get; set; } = null!;
    public int?    PingMs   { get; set; }
    public string? Detail   { get; set; }
}
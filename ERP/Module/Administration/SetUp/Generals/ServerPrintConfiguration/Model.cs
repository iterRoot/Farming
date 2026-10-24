namespace FarmingApi.Modules.Administration.ServerPrintConfig;

public class ServerPrintConfigResponse
{
    public int      Id              { get; set; }
    public string   PrinterCode     { get; set; } = null!;
    public string   PrinterName     { get; set; } = null!;
    public string   PrinterType     { get; set; } = null!;
    public string?  Location        { get; set; }
    public string?  Description     { get; set; }
    public string?  NetworkPath     { get; set; }
    public string?  IpAddress       { get; set; }
    public int?     Port            { get; set; }
    public string   Protocol        { get; set; } = null!;
    public string?  ShareName       { get; set; }
    public string?  DriverName      { get; set; }
    public string?  QueueName       { get; set; }
    public string?  PrintUsername   { get; set; }
    public string?  PrintPassword   { get; set; }   // masked "••••••••"
    public string   PaperSize       { get; set; } = null!;
    public string   Orientation     { get; set; } = null!;
    public decimal? CustomWidth     { get; set; }
    public decimal? CustomHeight    { get; set; }
    public int      DefaultCopies   { get; set; }
    public string   ColorMode       { get; set; } = null!;
    public string   DuplexMode      { get; set; } = null!;
    public bool     CanColor        { get; set; }
    public bool     CanDuplex       { get; set; }
    public bool     CanStaple       { get; set; }
    public int      MaxDpi          { get; set; }
    public string?  SupportedPapers { get; set; }
    public string?  PrintLanguage   { get; set; }
    public bool     IsDefault       { get; set; }
    public bool     IsActive        { get; set; }
    public int      SortOrder       { get; set; }
    public string?  Remarks         { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
    public List<PrintDocumentRouteResponse> Routes { get; set; } = new();
}

public class PrintDocumentRouteResponse
{
    public int     Id           { get; set; }
    public string  DocumentType { get; set; } = null!;
    public string? UserCode     { get; set; }
    public int     Copies       { get; set; }
    public string? PaperTray    { get; set; }
    public string? PaperSize    { get; set; }
    public bool    IsEnabled    { get; set; }
    public int     SortOrder    { get; set; }
}

public class ServerPrintConfigRequest
{
    public string   PrinterCode     { get; set; } = null!;
    public string   PrinterName     { get; set; } = null!;
    public string   PrinterType     { get; set; } = "Network";
    public string?  Location        { get; set; }
    public string?  Description     { get; set; }
    public string?  NetworkPath     { get; set; }
    public string?  IpAddress       { get; set; }
    public int?     Port            { get; set; }
    public string   Protocol        { get; set; } = "RAW";
    public string?  ShareName       { get; set; }
    public string?  DriverName      { get; set; }
    public string?  QueueName       { get; set; }
    public string?  PrintUsername   { get; set; }
    public string?  PrintPassword   { get; set; }
    public string   PaperSize       { get; set; } = "A4";
    public string   Orientation     { get; set; } = "Portrait";
    public decimal? CustomWidth     { get; set; }
    public decimal? CustomHeight    { get; set; }
    public int      DefaultCopies   { get; set; } = 1;
    public string   ColorMode       { get; set; } = "Mono";
    public string   DuplexMode      { get; set; } = "None";
    public bool     CanColor        { get; set; } = false;
    public bool     CanDuplex       { get; set; } = false;
    public bool     CanStaple       { get; set; } = false;
    public int      MaxDpi          { get; set; } = 600;
    public string?  SupportedPapers { get; set; }
    public string?  PrintLanguage   { get; set; } = "PCL";
    public bool     IsDefault       { get; set; } = false;
    public bool     IsActive        { get; set; } = true;
    public int      SortOrder       { get; set; } = 0;
    public string?  Remarks         { get; set; }
    public List<PrintDocumentRouteRequest> Routes { get; set; } = new();
}

public class PrintDocumentRouteRequest
{
    public string  DocumentType { get; set; } = null!;
    public string? UserCode     { get; set; }
    public int     Copies       { get; set; } = 1;
    public string? PaperTray    { get; set; }
    public string? PaperSize    { get; set; }
    public bool    IsEnabled    { get; set; } = true;
    public int     SortOrder    { get; set; } = 0;
}

// TCP ping result
public class PrinterTestResult
{
    public bool    Success  { get; set; }
    public string  Message  { get; set; } = null!;
    public int?    PingMs   { get; set; }
    public string? Detail   { get; set; }
}
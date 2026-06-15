namespace FarmingApi.Modules.Administration.ReportLayoutManager;

// ── Response ──────────────────────────────────────────────────
public class ReportLayoutResponse
{
    public int      Id                { get; set; }
    public string   Code              { get; set; } = null!;
    public string   Name              { get; set; } = null!;
    public string?  Description       { get; set; }
    public string   LayoutType        { get; set; } = null!;
    public string   DocumentType      { get; set; } = null!;
    public string   Module            { get; set; } = null!;
    public string   Category          { get; set; } = null!;
    public string   LayoutEngine      { get; set; } = null!;
    public string?  FilePath          { get; set; }
    public string?  FileUrl           { get; set; }
    public string?  FileName          { get; set; }
    public long?    FileSizeBytes     { get; set; }
    public string?  FileHash          { get; set; }
    public DateTime? LastUploadedAt   { get; set; }
    public string   PaperSize         { get; set; } = null!;
    public string   Orientation       { get; set; } = null!;
    public decimal? PageWidth         { get; set; }
    public decimal? PageHeight        { get; set; }
    public decimal  MarginTop         { get; set; }
    public decimal  MarginBottom      { get; set; }
    public decimal  MarginLeft        { get; set; }
    public decimal  MarginRight       { get; set; }
    public string   OutputFormat      { get; set; } = null!;
    public bool     AllowFormatChange { get; set; }
    public string   Language          { get; set; } = null!;
    public int      Copies            { get; set; }
    public bool     ShowWatermark     { get; set; }
    public string?  WatermarkText     { get; set; }
    public bool     IsDefault         { get; set; }
    public bool     IsSystemLayout    { get; set; }
    public int      SortOrder         { get; set; }
    public string?  AllowedRoles      { get; set; }
    public string   Version           { get; set; } = null!;
    public string?  ChangeLog         { get; set; }
    public bool     IsActive          { get; set; }
    public string?  Remarks           { get; set; }
    public DateTime CreatedAt         { get; set; }
    public DateTime UpdatedAt         { get; set; }
    public List<ReportParameterResponse> Parameters { get; set; } = new();
}

public class ReportParameterResponse
{
    public int     Id           { get; set; }
    public int     ParamOrder   { get; set; }
    public string  ParamKey     { get; set; } = null!;
    public string  ParamLabel   { get; set; } = null!;
    public string  DataType     { get; set; } = null!;
    public string? DefaultValue { get; set; }
    public string? Options      { get; set; }
    public bool    IsRequired   { get; set; }
    public bool    IsVisible    { get; set; }
}

// ── Request ───────────────────────────────────────────────────
public class ReportLayoutRequest
{
    public string   Code              { get; set; } = null!;
    public string   Name              { get; set; } = null!;
    public string?  Description       { get; set; }
    public string   LayoutType        { get; set; } = "PrintLayout";
    public string   DocumentType      { get; set; } = null!;
    public string   Module            { get; set; } = null!;
    public string   Category          { get; set; } = "General";
    public string   LayoutEngine      { get; set; } = "Crystal";
    public string?  FilePath          { get; set; }
    public string?  FileUrl           { get; set; }
    public string?  FileName          { get; set; }
    public string   PaperSize         { get; set; } = "A4";
    public string   Orientation       { get; set; } = "Portrait";
    public decimal? PageWidth         { get; set; }
    public decimal? PageHeight        { get; set; }
    public decimal  MarginTop         { get; set; } = 10;
    public decimal  MarginBottom      { get; set; } = 10;
    public decimal  MarginLeft        { get; set; } = 15;
    public decimal  MarginRight       { get; set; } = 10;
    public string   OutputFormat      { get; set; } = "PDF";
    public bool     AllowFormatChange { get; set; } = true;
    public string   Language          { get; set; } = "en";
    public int      Copies            { get; set; } = 1;
    public bool     ShowWatermark     { get; set; } = false;
    public string?  WatermarkText     { get; set; }
    public bool     IsDefault         { get; set; } = false;
    public int      SortOrder         { get; set; } = 0;
    public string?  AllowedRoles      { get; set; }
    public string   Version           { get; set; } = "1.0";
    public string?  ChangeLog         { get; set; }
    public bool     IsActive          { get; set; } = true;
    public string?  Remarks           { get; set; }
    public List<ReportParameterRequest> Parameters { get; set; } = new();
}

public class ReportParameterRequest
{
    public int     ParamOrder   { get; set; }
    public string  ParamKey     { get; set; } = null!;
    public string  ParamLabel   { get; set; } = null!;
    public string  DataType     { get; set; } = "String";
    public string? DefaultValue { get; set; }
    public string? Options      { get; set; }
    public bool    IsRequired   { get; set; } = false;
    public bool    IsVisible    { get; set; } = true;
}

// For the print/run dialog
public class RunReportRequest
{
    public int     ReportLayoutId { get; set; }
    public string  OutputFormat   { get; set; } = "PDF";
    public Dictionary<string, string> ParamValues { get; set; } = new();
}
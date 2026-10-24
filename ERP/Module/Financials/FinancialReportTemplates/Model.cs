namespace FarmingApi.Modules.Financials.FinancialReportTemplates;

public class FinancialReportTemplateRowResponse
{
    public int     Id            { get; set; }
    public int     LineNum       { get; set; }
    public string  RowCode       { get; set; } = null!;
    public string  Description   { get; set; } = null!;
    public string  RowType       { get; set; } = null!;
    public int     Level         { get; set; }
    public string? ParentRowCode { get; set; }
    public string? AccountFrom   { get; set; }
    public string? AccountTo     { get; set; }
    public string? Formula       { get; set; }
    public int     Sign          { get; set; }
    public bool    Bold          { get; set; }
    public bool    Underline     { get; set; }
    public bool    PrintZero     { get; set; }
    public string? Remarks       { get; set; }
}

public class FinancialReportTemplateResponse
{
    public int       Id          { get; set; }
    public string    Code        { get; set; } = null!;
    public string    Name        { get; set; } = null!;
    public string    ReportType  { get; set; } = null!;
    public string    Currency    { get; set; } = null!;
    public string?   Description { get; set; }
    public bool      IsDefault   { get; set; }
    public bool      IsActive    { get; set; }
    public string?   Remarks     { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
    public int       RowCount    { get; set; }
    public List<FinancialReportTemplateRowResponse> Rows { get; set; } = new();
}

public class FinancialReportTemplateRowRequest
{
    public string  RowCode       { get; set; } = null!;
    public string  Description   { get; set; } = null!;
    public string  RowType       { get; set; } = "Account";
    public int     Level         { get; set; } = 1;
    public string? ParentRowCode { get; set; }
    public string? AccountFrom   { get; set; }
    public string? AccountTo     { get; set; }
    public string? Formula       { get; set; }
    public int     Sign          { get; set; } = 1;
    public bool    Bold          { get; set; }
    public bool    Underline     { get; set; }
    public bool    PrintZero     { get; set; } = true;
    public string? Remarks       { get; set; }
}

public class FinancialReportTemplateRequest
{
    public string  Code        { get; set; } = null!;
    public string  Name        { get; set; } = null!;
    public string  ReportType  { get; set; } = null!;
    public string? Currency    { get; set; }
    public string? Description { get; set; }
    public bool    IsDefault   { get; set; }
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
    public List<FinancialReportTemplateRowRequest> Rows { get; set; } = new();
}

/// <summary>Body for POST /FinancialReportTemplates/{id}/Duplicate.</summary>
public class FinancialReportTemplateDuplicateRequest
{
    public string? Code { get; set; }
    public string? Name { get; set; }
}

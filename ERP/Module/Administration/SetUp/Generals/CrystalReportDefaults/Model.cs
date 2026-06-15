namespace FarmingApi.Modules.Administration.CrystalReportDefaults;

public class CrystalReportDefaultResponse
{
    public int      Id              { get; set; }
    public string   ElementKey      { get; set; } = null!;
    public string   ElementName     { get; set; } = null!;
    public string   ElementType     { get; set; } = null!;
    public string   Category        { get; set; } = null!;
    public string   DocumentType    { get; set; } = null!;
    public string   Language        { get; set; } = null!;
    public string   DataType        { get; set; } = null!;
    public string?  Value           { get; set; }
    public string?  DefaultValue    { get; set; }
    public string?  Placeholder     { get; set; }
    public string?  HelpText        { get; set; }
    public bool     IsRequired      { get; set; }
    public bool     IsSystemElement { get; set; }
    public string?  PreviewHint     { get; set; }
    public int      SortOrder       { get; set; }
    public bool     IsActive        { get; set; }
    public string?  Remarks         { get; set; }
    public DateTime UpdatedAt       { get; set; }
}

public class CrystalReportDefaultRequest
{
    public string   ElementKey      { get; set; } = null!;
    public string   ElementName     { get; set; } = null!;
    public string   ElementType     { get; set; } = "Custom";
    public string   Category        { get; set; } = "General";
    public string   DocumentType    { get; set; } = "All";
    public string   Language        { get; set; } = "en";
    public string   DataType        { get; set; } = "Text";
    public string?  Value           { get; set; }
    public string?  DefaultValue    { get; set; }
    public string?  Placeholder     { get; set; }
    public string?  HelpText        { get; set; }
    public bool     IsRequired      { get; set; } = false;
    public int      SortOrder       { get; set; } = 0;
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }
}

// Bulk value-only save (from settings page)
public class ElementValueItem
{
    public int      Id    { get; set; }
    public string?  Value { get; set; }
}

// Reset single element to default
public class ResetResult
{
    public string  ElementKey   { get; set; } = null!;
    public string? OldValue     { get; set; }
    public string? RestoredValue{ get; set; }
    public string  Message      { get; set; } = null!;
}
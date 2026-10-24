namespace FarmingApi.Modules.Administration.ReferenceFieldLink;

public class ReferenceFieldLinkResponse
{
    public int      Id                  { get; set; }
    public string   Code                { get; set; } = null!;
    public string   Name                { get; set; } = null!;
    public string   SourceModule        { get; set; } = null!;
    public string   SourceField         { get; set; } = null!;
    public string?  SourceLabel         { get; set; }
    public string   TargetTable         { get; set; } = null!;
    public string   TargetKeyField      { get; set; } = null!;
    public string   TargetDisplayField  { get; set; } = null!;
    public string?  TargetFilter        { get; set; }
    public string?  TargetOrderBy       { get; set; }
    public string?  TargetApiEndpoint   { get; set; }
    public string   LinkType            { get; set; } = null!;
    public bool     IsRequired          { get; set; }
    public bool     AllowSearch         { get; set; }
    public bool     AllowCreate         { get; set; }
    public int      SortOrder           { get; set; }
    public string?  AutoFillFields      { get; set; }
    public string?  Description         { get; set; }
    public bool     IsActive            { get; set; }
    public string?  Remarks             { get; set; }
    public DateTime CreatedAt           { get; set; }
    public DateTime UpdatedAt           { get; set; }
    // Parsed for frontend convenience
    public List<AutoFillFieldItem> AutoFillList { get; set; } = new();
}

public class ReferenceFieldLinkRequest
{
    public string   Code                { get; set; } = null!;
    public string   Name                { get; set; } = null!;
    public string   SourceModule        { get; set; } = null!;
    public string   SourceField         { get; set; } = null!;
    public string?  SourceLabel         { get; set; }
    public string   TargetTable         { get; set; } = null!;
    public string   TargetKeyField      { get; set; } = null!;
    public string   TargetDisplayField  { get; set; } = null!;
    public string?  TargetFilter        { get; set; }
    public string?  TargetOrderBy       { get; set; }
    public string?  TargetApiEndpoint   { get; set; }
    public string   LinkType            { get; set; } = "Lookup";
    public bool     IsRequired          { get; set; } = false;
    public bool     AllowSearch         { get; set; } = true;
    public bool     AllowCreate         { get; set; } = false;
    public int      SortOrder           { get; set; } = 0;
    public string?  AutoFillFields      { get; set; }
    public string?  Description         { get; set; }
    public bool     IsActive            { get; set; } = true;
    public string?  Remarks             { get; set; }
}

// Parsed auto-fill pair
public class AutoFillFieldItem
{
    public string SourceField { get; set; } = null!;  // field on the source document
    public string TargetField { get; set; } = null!;  // field on the target record
}

// Used by the /Resolve endpoint to look up a value
public class ResolveRequest
{
    public string  TargetTable        { get; set; } = null!;
    public string  TargetKeyField     { get; set; } = null!;
    public string  TargetDisplayField { get; set; } = null!;
    public string? SearchTerm         { get; set; }
    public string? Filter             { get; set; }
    public int     MaxResults         { get; set; } = 20;
}

public class ResolvedItem
{
    public string  Key     { get; set; } = null!;
    public string  Display { get; set; } = null!;
    public Dictionary<string, object?> Extra { get; set; } = new();
}
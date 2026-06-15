namespace FarmingApi.Modules.Administration.PredefinedText;

public class PredefinedTextResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Category    { get; set; }
    public string?  Module      { get; set; }
    public string   Language    { get; set; } = null!;
    public string   TextContent { get; set; } = null!;
    public string?  Tags        { get; set; }
    public int      SortOrder   { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class PredefinedTextRequest
{
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Category    { get; set; }
    public string?  Module      { get; set; }
    public string   Language    { get; set; } = "en";
    public string   TextContent { get; set; } = null!;
    public string?  Tags        { get; set; }
    public int      SortOrder   { get; set; } = 0;
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
}

// Used by clone endpoint
public class CopyTextRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
}
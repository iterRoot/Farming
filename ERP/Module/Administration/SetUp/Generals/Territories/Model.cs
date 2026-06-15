namespace FarmingApi.Modules.Sales.Territories;

public class TerritoryResponse
{
    public int      Id            { get; set; }
    public string   Code          { get; set; } = null!;
    public string   Name          { get; set; } = null!;
    public int?     ParentId      { get; set; }
    public string?  ParentCode    { get; set; }
    public string?  ParentName    { get; set; }
    public int      Level         { get; set; }
    public string?  LevelName     { get; set; }
    public string?  CountryCode   { get; set; }
    public string?  Region        { get; set; }
    public string?  Manager       { get; set; }
    public string?  ManagerEmail  { get; set; }
    public decimal? TargetRevenue { get; set; }
    public string?  Currency      { get; set; }
    public string?  Description   { get; set; }
    public bool     IsActive      { get; set; }
    public string?  Remarks       { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
    public int      ChildCount    { get; set; }          // how many direct children
    public List<TerritoryResponse> Children { get; set; } = new();
}

public class TerritoryRequest
{
    public string   Code          { get; set; } = null!;
    public string   Name          { get; set; } = null!;
    public int?     ParentId      { get; set; }
    public int      Level         { get; set; } = 1;
    public string?  LevelName     { get; set; }
    public string?  CountryCode   { get; set; }
    public string?  Region        { get; set; }
    public string?  Manager       { get; set; }
    public string?  ManagerEmail  { get; set; }
    public decimal? TargetRevenue { get; set; }
    public string?  Currency      { get; set; } = "KHR";
    public string?  Description   { get; set; }
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }
}

// Flat list item (for dropdowns / tables)
public class TerritoryFlatResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string   FullPath    { get; set; } = null!;   // e.g. "KH > Central > Phnom Penh"
    public int?     ParentId    { get; set; }
    public int      Level       { get; set; }
    public string?  LevelName   { get; set; }
    public string?  Manager     { get; set; }
    public bool     IsActive    { get; set; }
    public int      ChildCount  { get; set; }
}
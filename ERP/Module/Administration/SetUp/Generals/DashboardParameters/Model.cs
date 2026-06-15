namespace FarmingApi.Modules.Administration.DashboardParameters;

public class DashboardParameterResponse
{
    public int      Id           { get; set; }
    public string   Code         { get; set; } = null!;
    public string   Name         { get; set; } = null!;
    public string   Category     { get; set; } = null!;
    public string   DataType     { get; set; } = null!;
    public string?  Value        { get; set; }
    public string?  DefaultValue { get; set; }
    public string?  MinValue     { get; set; }
    public string?  MaxValue     { get; set; }
    public string?  Options      { get; set; }
    public string?  Unit         { get; set; }
    public string?  Description  { get; set; }
    public bool     IsSystem     { get; set; }
    public bool     IsActive     { get; set; }
    public string?  Remarks      { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}

public class DashboardParameterRequest
{
    public string   Code         { get; set; } = null!;
    public string   Name         { get; set; } = null!;
    public string   Category     { get; set; } = "General";
    public string   DataType     { get; set; } = "String";
    public string?  Value        { get; set; }
    public string?  DefaultValue { get; set; }
    public string?  MinValue     { get; set; }
    public string?  MaxValue     { get; set; }
    public string?  Options      { get; set; }
    public string?  Unit         { get; set; }
    public string?  Description  { get; set; }
    public bool     IsSystem     { get; set; } = false;
    public bool     IsActive     { get; set; } = true;
    public string?  Remarks      { get; set; }
}

// Lightweight value-only update (for bulk save from settings page)
public class ParameterValueRequest
{
    public string Code  { get; set; } = null!;
    public string? Value { get; set; }
}

// Reset response
public class ResetResult
{
    public string Code         { get; set; } = null!;
    public string? OldValue    { get; set; }
    public string? NewValue    { get; set; }
    public string  Message     { get; set; } = null!;
}
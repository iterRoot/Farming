namespace FarmingApi.Modules.BusinessPartners.BPProperties;

public class BPPropertyResponse
{
    public int      Id          { get; set; }
    public int      PropertyNo  { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string   AppliesTo   { get; set; } = null!;
    public string?  Description { get; set; }
    public string?  Color       { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class BPPropertyRequest
{
    public int      PropertyNo  { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string   AppliesTo   { get; set; } = "Both";
    public string?  Description { get; set; }
    public string?  Color       { get; set; }
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
}
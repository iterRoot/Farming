// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.PackageTypes;

public class PackageTypeResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Material    { get; set; }
    public decimal? Width       { get; set; }
    public decimal? Height      { get; set; }
    public decimal? Length      { get; set; }
    public decimal? MaxWeight   { get; set; }
    public decimal? Tare        { get; set; }
    public string?  DimUnit     { get; set; }
    public string?  WeightUnit  { get; set; }
    public string?  Description { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class PackageTypeRequest
{
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Material    { get; set; }
    public decimal? Width       { get; set; }
    public decimal? Height      { get; set; }
    public decimal? Length      { get; set; }
    public decimal? MaxWeight   { get; set; }
    public decimal? Tare        { get; set; }
    public string?  DimUnit     { get; set; } = "CM";
    public string?  WeightUnit  { get; set; } = "KG";
    public string?  Description { get; set; }
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
}

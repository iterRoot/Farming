// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.Weight;

public class WeightUomResponse
{
    public int      Id             { get; set; }
    public string   Code           { get; set; } = null!;
    public string   Name           { get; set; } = null!;
    public string?  Symbol         { get; set; }
    public decimal  ConversionToKg { get; set; }
    public bool     IsActive       { get; set; }
    public string?  Remarks        { get; set; }
    public DateTime CreatedAt      { get; set; }
    public DateTime UpdatedAt      { get; set; }
}

public class WeightUomRequest
{
    public string   Code           { get; set; } = null!;
    public string   Name           { get; set; } = null!;
    public string?  Symbol         { get; set; }
    public decimal  ConversionToKg { get; set; } = 1;
    public bool     IsActive       { get; set; } = true;
    public string?  Remarks        { get; set; }
}


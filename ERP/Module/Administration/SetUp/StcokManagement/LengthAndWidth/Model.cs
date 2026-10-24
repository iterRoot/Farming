// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.LengthWidth;

public class LengthWidthUomResponse
{
    public int      Id                { get; set; }
    public string   Code              { get; set; } = null!;
    public string   Name              { get; set; } = null!;
    public string?  Symbol            { get; set; }
    public decimal  ConversionToMeter { get; set; }
    public bool     IsActive          { get; set; }
    public string?  Remarks           { get; set; }
    public DateTime CreatedAt         { get; set; }
    public DateTime UpdatedAt         { get; set; }
}

public class LengthWidthUomRequest
{
    public string   Code              { get; set; } = null!;
    public string   Name              { get; set; } = null!;
    public string?  Symbol            { get; set; }
    public decimal  ConversionToMeter { get; set; } = 1;
    public bool     IsActive          { get; set; } = true;
    public string?  Remarks           { get; set; }
}


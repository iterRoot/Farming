// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.UnitOfMeasure;

public class UnitOfMeasureResponse
{
    public int      Id            { get; set; }
    public string   Code          { get; set; } = null!;
    public string   Name          { get; set; } = null!;
    public string?  Description   { get; set; }
    public string?  Category      { get; set; }
    public decimal? DecimalPlaces { get; set; }
    public bool     IsActive      { get; set; }
    public string?  Remarks       { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
}

public class UnitOfMeasureRequest
{
    public string   Code          { get; set; } = null!;
    public string   Name          { get; set; } = null!;
    public string?  Description   { get; set; }
    public string?  Category      { get; set; }
    public decimal? DecimalPlaces { get; set; }
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }
}

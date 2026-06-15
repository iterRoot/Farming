// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.Locations;

public class LocationResponse
{
    public int      Id            { get; set; }
    public string   Code          { get; set; } = null!;
    public string   Name          { get; set; } = null!;
    public string?  WarehouseCode { get; set; }
    public string?  Aisle         { get; set; }
    public string?  Row           { get; set; }
    public string?  Shelf         { get; set; }
    public string?  Bin           { get; set; }
    public string?  Zone          { get; set; }
    public decimal? MaxWeight     { get; set; }
    public bool     IsActive      { get; set; }
    public string?  Remarks       { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
}

public class LocationRequest
{
    public string   Code          { get; set; } = null!;
    public string   Name          { get; set; } = null!;
    public string?  WarehouseCode { get; set; }
    public string?  Aisle         { get; set; }
    public string?  Row           { get; set; }
    public string?  Shelf         { get; set; }
    public string?  Bin           { get; set; }
    public string?  Zone          { get; set; }
    public decimal? MaxWeight     { get; set; }
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }
}

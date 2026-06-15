// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.CycleCount;

public class CycleCountResponse
{
    public int      Id             { get; set; }
    public string   Code           { get; set; } = null!;
    public string   Name           { get; set; } = null!;
    public string   ItemClass      { get; set; } = null!;
    public string   AlertType      { get; set; } = null!;
    public int      CountFrequency { get; set; }
    public string?  WarehouseCode  { get; set; }
    public string?  ItemGroupCode  { get; set; }
    public string?  Description    { get; set; }
    public bool     IsActive       { get; set; }
    public string?  Remarks        { get; set; }
    public DateTime CreatedAt      { get; set; }
    public DateTime UpdatedAt      { get; set; }
}

public class CycleCountRequest
{
    public string   Code           { get; set; } = null!;
    public string   Name           { get; set; } = null!;
    public string   ItemClass      { get; set; } = "A";
    public string   AlertType      { get; set; } = "ByDate";
    public int      CountFrequency { get; set; }
    public string?  WarehouseCode  { get; set; }
    public string?  ItemGroupCode  { get; set; }
    public string?  Description    { get; set; }
    public bool     IsActive       { get; set; } = true;
    public string?  Remarks        { get; set; }
}

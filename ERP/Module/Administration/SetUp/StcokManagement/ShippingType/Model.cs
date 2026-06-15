// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.ShippingTypes;

public class ShippingTypeResponse
{
    public int      Id           { get; set; }
    public string   Code         { get; set; } = null!;
    public string   Name         { get; set; } = null!;
    public string?  Carrier      { get; set; }
    public string?  Website      { get; set; }
    public string?  Phone        { get; set; }
    public string?  TrackingUrl  { get; set; }
    public int?     LeadTimeDays { get; set; }
    public bool     IsActive     { get; set; }
    public string?  Remarks      { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}

public class ShippingTypeRequest
{
    public string   Code         { get; set; } = null!;
    public string   Name         { get; set; } = null!;
    public string?  Carrier      { get; set; }
    public string?  Website      { get; set; }
    public string?  Phone        { get; set; }
    public string?  TrackingUrl  { get; set; }
    public int?     LeadTimeDays { get; set; }
    public bool     IsActive     { get; set; } = true;
    public string?  Remarks      { get; set; }
}



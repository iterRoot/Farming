// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.Manufacturers;

public class ManufacturerResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Country     { get; set; }
    public string?  Website     { get; set; }
    public string?  Phone       { get; set; }
    public string?  Email       { get; set; }
    public string?  ContactName { get; set; }
    public string?  Description { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class ManufacturerRequest
{
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  Country     { get; set; }
    public string?  Website     { get; set; }
    public string?  Phone       { get; set; }
    public string?  Email       { get; set; }
    public string?  ContactName { get; set; }
    public string?  Description { get; set; }
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
}


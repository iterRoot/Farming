// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.DutyGroups;

public class DutyGroupResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public decimal  DutyRate    { get; set; }
    public string?  Description { get; set; }
    public string?  HsCode      { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
}

public class DutyGroupRequest
{
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public decimal  DutyRate    { get; set; }
    public string?  Description { get; set; }
    public string?  HsCode      { get; set; }
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
}

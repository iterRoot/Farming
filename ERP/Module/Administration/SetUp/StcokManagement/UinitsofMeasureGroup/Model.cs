// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.UomGroup;

public class UomGroupResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string   BaseUom     { get; set; } = null!;
    public string?  Description { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
    public List<UomGroupLineResponse> Lines { get; set; } = new();
}

public class UomGroupLineResponse
{
    public int     Id      { get; set; }
    public string  AltUom  { get; set; } = null!;
    public decimal AltQty  { get; set; }
    public decimal BaseQty { get; set; }
}

public class UomGroupRequest
{
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string   BaseUom     { get; set; } = null!;
    public string?  Description { get; set; }
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
    public List<UomGroupLineRequest> Lines { get; set; } = new();
}

public class UomGroupLineRequest
{
    public string  AltUom  { get; set; } = null!;
    public decimal AltQty  { get; set; } = 1;
    public decimal BaseQty { get; set; }
}


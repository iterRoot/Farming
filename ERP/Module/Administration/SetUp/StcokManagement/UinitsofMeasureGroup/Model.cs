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

// ── Conversion ─────────────────────────────────────────────────
// One unit available in a group, with how many base units it equals.
public class UomUnitInfo
{
    public string  Uom             { get; set; } = null!;
    public bool    IsBase          { get; set; }
    public decimal BaseUnitsPerOne { get; set; }   // base units in 1 of this UoM
}

public class UomConvertRequest
{
    public int?    UomGroupId { get; set; }   // identify the group by id…
    public string? GroupCode  { get; set; }   // …or by code
    public string  FromUom    { get; set; } = null!;
    public string  ToUom      { get; set; } = null!;
    public decimal Quantity   { get; set; }
}

public class UomConvertResponse
{
    public string  GroupCode { get; set; } = null!;
    public string  BaseUom   { get; set; } = null!;
    public string  FromUom   { get; set; } = null!;
    public string  ToUom     { get; set; } = null!;
    public decimal Quantity  { get; set; }
    public decimal Result    { get; set; }
    public decimal BaseQty   { get; set; }   // the input expressed in the base unit
    public decimal Rate      { get; set; }   // ToUom units per 1 FromUom
}


namespace FarmingApi.Modules.Sale.Warehouse;

public class WarehouseListResponse
{
    public int      Id       { get; set; }
    public string   Code     { get; set; } = null!;
    public string   Name     { get; set; } = null!;
    public string?  Location { get; set; }
    public string?  Address  { get; set; }
    public string?  Manager  { get; set; }
    public string?  Phone    { get; set; }
    public string?  Email    { get; set; }
    public string?  Remarks  { get; set; }
    public string?  Branch   { get; set; }
    public string?  Street   { get; set; }
    public string?  City     { get; set; }
    public string?  State    { get; set; }
    public string?  ZipCode  { get; set; }
    public string?  Country  { get; set; }
    public bool     IsDefault { get; set; }
    public bool     Nettable  { get; set; }
    public bool     DropShip  { get; set; }
    public bool     IsActive  { get; set; }
}

// The Create form posts these field names (code, name, location, …, isActive).
public class WarehouseListRequest
{
    public string   Code     { get; set; } = null!;
    public string   Name     { get; set; } = null!;
    public string?  Location { get; set; }
    public string?  Address  { get; set; }
    public string?  Manager  { get; set; }
    public string?  Phone    { get; set; }
    public string?  Email    { get; set; }
    public string?  Remarks  { get; set; }
    public string?  Branch   { get; set; }
    public string?  Street   { get; set; }
    public string?  City     { get; set; }
    public string?  State    { get; set; }
    public string?  ZipCode  { get; set; }
    public string?  Country  { get; set; }
    public bool     IsDefault { get; set; }
    public bool     Nettable  { get; set; } = true;
    public bool     DropShip  { get; set; }
    public bool     IsActive  { get; set; } = true;
}

public class WarehouseUpdateRequest : WarehouseListRequest { }

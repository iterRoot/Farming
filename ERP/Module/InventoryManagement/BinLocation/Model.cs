namespace FarmingApi.Modules.InventoryManagement.BinLocation;

public class BinLocationRequest
{
    public string  Code           { get; set; } = null!;
    public string  WarehouseCode  { get; set; } = null!;
    public string? Description    { get; set; }
    public string? Sublevel1      { get; set; }
    public string? Sublevel2      { get; set; }
    public bool    IsActive       { get; set; } = true;
    public bool    IsReceivingBin { get; set; }
    public decimal? MaxWeight     { get; set; }
}

public class BinLocationResponse
{
    public int      Id             { get; set; }
    public string   Code           { get; set; } = null!;
    public string   WarehouseCode  { get; set; } = null!;
    public string?  WarehouseName  { get; set; }
    public string?  Description    { get; set; }
    public string?  Sublevel1      { get; set; }
    public string?  Sublevel2      { get; set; }
    public bool     IsActive       { get; set; }
    public bool     IsReceivingBin { get; set; }
    public decimal? MaxWeight      { get; set; }
    public DateTime CreatedAt      { get; set; }
}

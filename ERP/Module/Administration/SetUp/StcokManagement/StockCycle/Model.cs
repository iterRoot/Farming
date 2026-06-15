// ═══════════════════════════════════════════════════════════════
// Model.cs
// ═══════════════════════════════════════════════════════════════
namespace FarmingApi.Modules.Inventory.StockCycles;

public class StockCycleResponse
{
    public int       Id            { get; set; }
    public string    Code          { get; set; } = null!;
    public string    Name          { get; set; } = null!;
    public string    CycleType     { get; set; } = null!;
    public string    Status        { get; set; } = null!;
    public DateTime  StartDate     { get; set; }
    public DateTime  EndDate       { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string?   WarehouseCode { get; set; }
    public string?   ResponsibleBy { get; set; }
    public string?   Description   { get; set; }
    public bool      IsActive      { get; set; }
    public string?   Remarks       { get; set; }
    public DateTime  CreatedAt     { get; set; }
    public DateTime  UpdatedAt     { get; set; }
}

public class StockCycleRequest
{
    public string    Code          { get; set; } = null!;
    public string    Name          { get; set; } = null!;
    public string    CycleType     { get; set; } = "Monthly";
    public string    Status        { get; set; } = "Open";
    public DateTime  StartDate     { get; set; }
    public DateTime  EndDate       { get; set; }
    public string?   WarehouseCode { get; set; }
    public string?   ResponsibleBy { get; set; }
    public string?   Description   { get; set; }
    public bool      IsActive      { get; set; } = true;
    public string?   Remarks       { get; set; }
}

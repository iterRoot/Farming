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
    public int?      JournalEntryId { get; set; }
    public int       LineCount     { get; set; }
    public int       CountedCount  { get; set; }
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

public class StockCycleLineResponse
{
    public int      Id          { get; set; }
    public string   ItemCode    { get; set; } = null!;
    public string   ItemName    { get; set; } = null!;
    public string?  Uom         { get; set; }
    public string   WhsCode     { get; set; } = null!;
    public decimal  SystemQty   { get; set; }
    public decimal? CountedQty  { get; set; }
    public decimal? Variance    { get; set; }   // counted − system at the start of the cycle
    public decimal? CurrentQty  { get; set; }   // live book quantity while the cycle is running
    public decimal? PostedQty   { get; set; }
    public decimal? PostedUnitCost { get; set; }
}

public class StockCycleCountUpdate
{
    public int      LineId     { get; set; }
    public decimal? CountedQty { get; set; }    // null clears a count
}

public class StockCycleAddLineRequest
{
    public string ItemCode { get; set; } = null!;
    public string WhsCode  { get; set; } = null!;
}

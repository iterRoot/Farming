namespace FarmingApi.Modules.InventoryManagement.AlternativeItem;

public class AlternativeItemRequest
{
    public string  ItemCode    { get; set; } = null!;
    public string  AltItemCode { get; set; } = null!;
    public decimal Factor      { get; set; } = 1m;
    public int     Ranking     { get; set; }
    public string? Criteria    { get; set; }
}

public class AlternativeItemResponse
{
    public int      Id          { get; set; }
    public string   ItemCode    { get; set; } = null!;
    public string?  ItemName    { get; set; }
    public string   AltItemCode { get; set; } = null!;
    public string?  AltItemName { get; set; }
    public decimal  Factor      { get; set; }
    public int      Ranking     { get; set; }
    public string?  Criteria    { get; set; }
    public DateTime CreatedAt   { get; set; }
}

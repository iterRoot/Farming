namespace FarmingApi.Modules.InventoryManagement.OrderRecommendation;

public class OrderRecommendationLine
{
    public int      ItemId          { get; set; }
    public string   ItemCode        { get; set; } = null!;
    public string   ItemName        { get; set; } = null!;
    public string   RecommendedType { get; set; } = "Purchase"; // "Production" | "Purchase"
    public decimal  SalesDemand     { get; set; }   // open sales order quantity
    public decimal  OnHand          { get; set; }
    public decimal  MinStock        { get; set; }
    public decimal  RecommendedQty  { get; set; }   // quantity to make or buy
    public int?     BillOfMaterialId { get; set; }
    public string?  Warehouse       { get; set; }
}

public class OrderRecommendationResponse
{
    public DateTime GeneratedAt { get; set; }
    public int      Count       { get; set; }
    public int      MakeCount   { get; set; }
    public int      BuyCount    { get; set; }
    public List<OrderRecommendationLine> Lines { get; set; } = new();
}

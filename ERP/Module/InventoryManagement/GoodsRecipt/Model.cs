namespace FarmingApi.Modules.Inventory.GoodsReceipt;

public class GoodsReceiptListResponse
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public int DocumentNumber { get; set; }
    public DateTime PostingDate { get; set; }
    public decimal GrandTotal { get; set; }
}
public class GoodsReceiptInsertListRequest
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public int DocumentNumber { get; set; }
    public DateTime PostingDate { get; set; }
    public decimal GrandTotal { get; set; }
}
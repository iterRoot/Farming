namespace FarmingApi.Modules.Sale.Inventory;

public class InventoryListResponse
{
    public string ItemsCode { get; set; } = null!;
    public string WhsCode { get; set; }  = null!;
    public decimal InQty { get; set; }
    public decimal OutQty { get; set; }
    public decimal Balance { get; set; }

}

public class InventoryListRequest
{
     public string ItemsCode { get; set; } = null!;
    public string WhsCode { get; set; }  = null!;
    public decimal InQty { get; set; }
    public decimal OutQty { get; set; }
    public decimal Balance { get; set; }
}
public class InventoryUpdateRequest
{
    public string ItemsCode { get; set; } = null!;
    public string WhsCode { get; set; }  = null!;
    public decimal InQty { get; set; }
    public decimal OutQty { get; set; }
    public decimal Balance { get; set; }
}
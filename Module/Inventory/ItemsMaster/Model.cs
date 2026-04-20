namespace FarmingApi.Modules.Master.ItemsMaster;

public class ItemsMasterResponse
{
    public int Id { get; set; }
    public string ItemCode { get; set; }
    public string ItemName { get; set; }
    public decimal Price { get; set; }
    public ItemStatus Status { get; set; }
}

public class ItemsMasterRequest
{
    public string ItemCode { get; set; }
    public string ItemName { get; set; }

    public string UomName { get; set; }
    public string UomCode { get; set; }

    public decimal Price { get; set; }
    public string PriceList { get; set; }

    public string Types { get; set; }

    public ItemStatus Status { get; set; }
}

public class ItemsMasterUpdateRequest
{
    public string ItemName { get; set; }

    public string UomName { get; set; }
    public string UomCode { get; set; }

    public decimal Price { get; set; }
    public string PriceList { get; set; }

    public string Types { get; set; }

    public ItemStatus Status { get; set; }
}
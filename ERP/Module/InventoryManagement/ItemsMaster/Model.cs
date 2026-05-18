namespace FarmingApi.Modules.Inventory.ItemsMaster;

public class ItemsMasterResponse
{
    public int Id { get; set; }

    public string ItemCode { get; set; } = null!;
    public string ItemName { get; set; } = null!;

    public string? ItemGroup { get; set; }
    public string ItemType { get; set; } = null!;

    public string? UomName { get; set; }

    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }

    public string? Warehouse { get; set; }

    public ItemStatus Status { get; set; }
}


// 🔥 CREATE
public class ItemsMasterRequest
{
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;

    public string ItemGroup { get; set; } = string.Empty;
    public string ItemType { get; set; } = "Inventory";
    public string Barcode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;

    public string UomName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;

    public string ManageBy { get; set; } = "None";

    public bool SalesItem { get; set; } = true;
    public bool PurchaseItem { get; set; } = true;

    public int Status { get; set; } = 0;

    // Inventory
    public string Warehouse { get; set; } = string.Empty;
    public decimal MinStock { get; set; }
    public decimal MaxStock { get; set; }
    public string ValuationMethod { get; set; } = "MovingAverage";

    // Sales
    public decimal SalePrice { get; set; }
    public string PriceList { get; set; } = string.Empty;
    public string SaleTaxGroup { get; set; } = string.Empty;
    public decimal SaleDiscount { get; set; }
    public string SaleUom { get; set; } = string.Empty;
    public string SaleDescription { get; set; } = string.Empty;

    // Purchasing
    public decimal PurchasePrice { get; set; }
    public string PreferredVendor { get; set; } = string.Empty;
    public string VendorItemNo { get; set; } = string.Empty;
    public string PurchaseUom { get; set; } = string.Empty;
    public int LeadTime { get; set; }
    public string PurchaseTaxGroup { get; set; } = string.Empty;
}


// 🔥 UPDATE
public class ItemsMasterUpdateRequest
{
    public string ItemName { get; set; } = string.Empty;

    public string ItemGroup { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;

    public string UomName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;

    public string Warehouse { get; set; } = string.Empty;

    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }

    public ItemStatus Status { get; set; }
}
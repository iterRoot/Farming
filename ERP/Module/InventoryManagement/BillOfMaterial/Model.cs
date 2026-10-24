namespace FarmingApi.Modules.InventoryManagement.BillOfMaterial;

public class BillOfMaterialLineResponse
{
    public int     Id       { get; set; }
    public int     LineNum  { get; set; }
    public string  ItemCode { get; set; } = null!;
    public string  ItemName { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class BillOfMaterialResponse
{
    public int      Id             { get; set; }
    public string   Code           { get; set; } = null!;
    public string   ParentItemCode { get; set; } = null!;
    public string   ParentItemName { get; set; } = null!;
    public decimal  Quantity       { get; set; }
    public string   Warehouse      { get; set; } = null!;
    public bool     IsActive       { get; set; }
    public string?  Remarks        { get; set; }
    public DateTime CreatedAt      { get; set; }
    public DateTime? UpdatedAt     { get; set; }
    public List<BillOfMaterialLineResponse> Lines { get; set; } = new();
}

public class BillOfMaterialLineRequest
{
    public string  ItemCode { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class BillOfMaterialRequest
{
    public string   ParentItemCode { get; set; } = null!;
    public decimal  Quantity       { get; set; } = 1m;
    public string   Warehouse      { get; set; } = null!;
    public bool     IsActive       { get; set; } = true;
    public string?  Remarks        { get; set; }
    public List<BillOfMaterialLineRequest> Lines { get; set; } = new();
}

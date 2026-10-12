namespace FarmingApi.Modules.InventoryManagement.ProductionOrder;

public class ProductionOrderLineResponse
{
    public int     Id         { get; set; }
    public int     LineNum    { get; set; }
    public string  ItemCode   { get; set; } = null!;
    public string  ItemName   { get; set; } = null!;
    public decimal PlannedQty { get; set; }
    public string  Warehouse  { get; set; } = null!;
}

public class ProductionOrderResponse
{
    public int      Id               { get; set; }
    public string   DocNum           { get; set; } = null!;
    public string   ItemCode         { get; set; } = null!;
    public string   ItemName         { get; set; } = null!;
    public int      BillOfMaterialId { get; set; }
    public string   BomCode          { get; set; } = null!;
    public decimal  PlannedQty       { get; set; }
    public string   Warehouse        { get; set; } = null!;
    public DateTime DueDate          { get; set; }
    public string   Status           { get; set; } = null!;
    public DateTime? ClosedDate      { get; set; }
    public int?      JournalEntryId  { get; set; }
    public string?   Remarks         { get; set; }
    public DateTime CreatedAt        { get; set; }
    public DateTime? UpdatedAt       { get; set; }
    public List<ProductionOrderLineResponse> Lines { get; set; } = new();
}

public class ProductionOrderRequest
{
    public string   ItemCode         { get; set; } = null!;
    public int      BillOfMaterialId { get; set; }
    public decimal  PlannedQty       { get; set; }
    public string?  Warehouse        { get; set; }   // defaults to the BOM's own warehouse
    public DateTime DueDate          { get; set; }
    public string?  Remarks          { get; set; }
}

// One line's shortfall when Close is refused for insufficient stock.
public class ShortageLine
{
    public string  ItemCode { get; set; } = null!;
    public string  ItemName { get; set; } = null!;
    public decimal Needed   { get; set; }
    public decimal OnHand   { get; set; }
    public decimal Short    { get; set; }
}


// Optional body for Close: batch / serial numbers for tracked items. Components are picked
// from existing stock; the finished good's numbers are recorded as it comes in.
public class ProductionCloseRequest
{
    public List<ProductionComponentBatches>? Components { get; set; }
    public List<FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchLine>? Finished { get; set; }
}

public class ProductionComponentBatches
{
    public string ItemCode { get; set; } = null!;
    public List<FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchLine> Batches { get; set; } = new();
}

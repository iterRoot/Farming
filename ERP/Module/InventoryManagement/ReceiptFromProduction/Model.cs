namespace FarmingApi.Modules.InventoryManagement.ReceiptFromProduction;

public class ReceiptFromProductionLineRequest
{
    public string  ItemCode { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class ReceiptFromProductionRequest
{
    public DateTime  PostingDate       { get; set; }
    public string?   Warehouse         { get; set; }
    public string?   ProductionOrderNo { get; set; }
    public string?   Remarks           { get; set; }
    public List<ReceiptFromProductionLineRequest> Lines { get; set; } = new();
}

public class ReceiptFromProductionLineResponse
{
    public int     Id       { get; set; }
    public int     LineNum  { get; set; }
    public string  ItemCode { get; set; } = null!;
    public string  ItemName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Total    { get; set; }
}

public class ReceiptFromProductionResponse
{
    public int       Id                { get; set; }
    public string    DocNum            { get; set; } = null!;
    public DateTime  PostingDate       { get; set; }
    public string    Warehouse         { get; set; } = null!;
    public string?   ProductionOrderNo { get; set; }
    public string    Status            { get; set; } = "C";
    public int?      JournalEntryId    { get; set; }
    public decimal   TotalCost         { get; set; }
    public string?   Remarks           { get; set; }
    public DateTime  CreatedAt         { get; set; }
    public List<ReceiptFromProductionLineResponse> Lines { get; set; } = new();
}

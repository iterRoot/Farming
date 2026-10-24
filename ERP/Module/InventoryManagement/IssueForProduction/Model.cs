namespace FarmingApi.Modules.InventoryManagement.IssueForProduction;

public class IssueForProductionLineRequest
{
    public string  ItemCode { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class IssueForProductionRequest
{
    public DateTime  PostingDate       { get; set; }
    public string?   Warehouse         { get; set; }   // defaults to the default warehouse
    public string?   ProductionOrderNo { get; set; }
    public string?   Remarks           { get; set; }
    public List<IssueForProductionLineRequest> Lines { get; set; } = new();
}

public class IssueForProductionLineResponse
{
    public int     Id       { get; set; }
    public int     LineNum  { get; set; }
    public string  ItemCode { get; set; } = null!;
    public string  ItemName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Cost     { get; set; }
}

public class IssueForProductionResponse
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
    public List<IssueForProductionLineResponse> Lines { get; set; } = new();
}

namespace FarmingApi.Modules.InventoryManagement.PickAndPack;

public class PickPackManageDto
{
    public bool SalesOrders          { get; set; } = true;
    public bool ReserveInvoices      { get; set; }
    public bool ProductionOrders     { get; set; }
    public bool StockTransferRequests{ get; set; }
}

public class PickPackFilterDto
{
    public string? Field { get; set; }
    public string? From  { get; set; }
    public string? To    { get; set; }
}

public class PickPackCriteriaRequest
{
    public string  CriteriaName { get; set; } = "";
    public string? Status       { get; set; }
    public string? GroupBy      { get; set; }
    public string? SortBy       { get; set; }
    public bool    ReleaseManually { get; set; }
    public PickPackManageDto Manage { get; set; } = new();
    public List<string> Warehouses { get; set; } = new();
    public List<PickPackFilterDto> Filters { get; set; } = new();
}

public class PickPackLineDto
{
    public int      Id           { get; set; }
    public int      LineNum      { get; set; }
    public string   BaseDocType  { get; set; } = "";
    public int      BaseDocEntry { get; set; }
    public string?  BaseDocNum   { get; set; }
    public string?  CustomerName { get; set; }
    public DateTime? DueDate     { get; set; }
    public string   ItemCode     { get; set; } = "";
    public string?  ItemName     { get; set; }
    public string?  WhsCode      { get; set; }
    public decimal  RequiredQty  { get; set; }
    public decimal  PickedQty    { get; set; }
    public string   Status       { get; set; } = "";
    public int?     BaseLineId   { get; set; }
    public string?  GroupKey     { get; set; }   // what the list is grouped by, when it is
}

public class PickPackCriteriaResponse
{
    public int      Id           { get; set; }
    public string   CriteriaName { get; set; } = "";
    public string   Status       { get; set; } = "";
    public string?  GroupBy      { get; set; }
    public string?  SortBy       { get; set; }
    public bool     ReleaseManually { get; set; }
    public PickPackManageDto Manage { get; set; } = new();
    public List<string> Warehouses { get; set; } = new();
    public List<PickPackFilterDto> Filters { get; set; } = new();
    public int      LineCount    { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime? UpdatedAt   { get; set; }
    public List<PickPackLineDto> Lines { get; set; } = new();
}

public class PickPackLineUpdate
{
    public int     Id        { get; set; }
    public decimal PickedQty { get; set; }
}

public class PickPackReleaseRequest
{
    public List<int>? LineIds { get; set; }   // empty or missing = every line not yet released
}

public static class PickPackOptions
{
    public static readonly string[] GroupBy = { "None", "Customer", "Item", "Warehouse" };
    public static readonly string[] SortBy  = { "Delivery/Due Date", "Posting Date", "Sales Order No.", "Customer" };

    // Filter fields that are applied to the source documents.
    public static readonly string[] DateFields = { "Posting Date", "Delivery/Due Date", "Required Date" };
    public static readonly string[] TextFields = { "Sales Order No.", "Customer", "Item" };
}

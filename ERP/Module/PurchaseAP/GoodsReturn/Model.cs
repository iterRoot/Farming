namespace FarmingApi.Modules.PurchaseAP.GoodsReturn;

public class GoodsReturnRequest
{
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    public List<GoodsReturnLineRequest> Items { get; set; } = new();
}

public class GoodsReturnLineRequest
{
    public int     ItemId    { get; set; }
    /// <summary>Optional — falls back to the default warehouse when omitted.</summary>
    public string? WhsCode   { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}

public class GoodsReturnUpdateRequest : GoodsReturnRequest { }

public class GoodsReturnResponse
{
    public int       Id          { get; set; }
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public DateTime  CreatedAt   { get; set; }

    // Auto-created Journal Entry (reverse of Goods Receipt PO).
    public int?      JournalEntryId { get; set; }
    public string?   JournalNo      { get; set; }

    public int    VendorId   { get; set; }
    public string VendorCode { get; set; } = "";
    public string VendorName { get; set; } = "";

    public List<GoodsReturnLineResponse> Items { get; set; } = new();
}

public class GoodsReturnLineResponse
{
    public int     Id        { get; set; }
    public int     ItemId    { get; set; }
    public string  ItemCode  { get; set; } = "";
    public string? ItemName  { get; set; }
    public string? WhsCode   { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}
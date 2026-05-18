namespace FarmingApi.Modules.PurchaseAP.GoodsReturnRequest;

public class GoodsReturnRequestRequest
{
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    public List<GoodsReturnRequestLineRequest> Items { get; set; } = new();
}

public class GoodsReturnRequestLineRequest
{
    public int     ItemId    { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}

public class GoodsReturnRequestUpdateRequest : GoodsReturnRequestRequest { }

public class GoodsReturnRequestResponse
{
    public int       Id          { get; set; }
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public DateTime  CreatedAt   { get; set; }

    public int    VendorId   { get; set; }
    public string VendorCode { get; set; } = "";
    public string VendorName { get; set; } = "";

    public List<GoodsReturnRequestLineResponse> Items { get; set; } = new();
}

public class GoodsReturnRequestLineResponse
{
    public int     Id        { get; set; }
    public int     ItemId    { get; set; }
    public string  ItemCode  { get; set; } = "";
    public string? ItemName  { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}
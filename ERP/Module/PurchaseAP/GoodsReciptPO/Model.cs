namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST
// ══════════════════════════════════════════════════════════════════════════════
public class GoodsReceiptPORequest
{
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public List<GoodsReceiptPOLineRequest> Items { get; set; } = new();
}

public class GoodsReceiptPOLineRequest
{
    public int     ItemId    { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }   // ✅ FK to PurchaseOrder
    public string? BaseType  { get; set; }   // ✅ "PurchaseOrder"
}

public class GoodsReceiptPOUpdateRequest : GoodsReceiptPORequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════════════════
public class GoodsReceiptPOResponse
{
    public int       Id         { get; set; }
    public string    DocNum     { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public string    Status     { get; set; } = "O";
    public decimal   Total      { get; set; }
    public DateTime  CreatedAt  { get; set; }

    public int    VendorId   { get; set; }
    public string VendorCode { get; set; } = "";
    public string VendorName { get; set; } = "";

    public List<GoodsReceiptPOLineResponse> Items { get; set; } = new();
}

public class GoodsReceiptPOLineResponse
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
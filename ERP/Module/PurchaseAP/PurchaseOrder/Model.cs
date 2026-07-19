namespace FarmingApi.Modules.PurchaseAP.PurchaseOrder;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseOrderRequest
{
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    public List<PurchaseOrderLineRequest> Items { get; set; } = new();
}

public class PurchaseOrderLineRequest
{
    public int     ItemId    { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }   // ✅ FK to PurchaseQuotation
    public string? BaseType  { get; set; }   // ✅ "PurchaseQuotation"
}

public class PurchaseOrderUpdateRequest : PurchaseOrderRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseOrderResponse
{
    public int       Id         { get; set; }
    public string    DocNum     { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate    { get; set; }
    public string    Status     { get; set; } = "O";
    public decimal   Discount   { get; set; }
    public decimal   Tax        { get; set; }
    public decimal   TaxAmount  { get; set; }
    public decimal   Total      { get; set; }
    public string?   Remarks    { get; set; }
    public DateTime  CreatedAt  { get; set; }

    public int    VendorId   { get; set; }
    public string VendorCode { get; set; } = "";
    public string VendorName { get; set; } = "";

    public List<PurchaseOrderLineResponse> Items { get; set; } = new();
}

public class PurchaseOrderLineResponse
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
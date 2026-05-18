namespace FarmingApi.Modules.PurchaseAP.PurchaseQuotation;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseQuotationRequest
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

    public List<PurchaseQuotationLineRequest> Items { get; set; } = new();
}

public class PurchaseQuotationLineRequest
{
    public int     ItemId    { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }   // ✅ optional base document reference
    public string? BaseType  { get; set; }   // ✅ "PurchaseBlanketAgreement"
}

public class PurchaseQuotationUpdateRequest : PurchaseQuotationRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseQuotationResponse
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

    public List<PurchaseQuotationLineResponse> Items { get; set; } = new();
}

public class PurchaseQuotationLineResponse
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
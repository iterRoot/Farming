namespace FarmingApi.Modules.PurchaseAP.APDownPaymentRequest;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentRequestRequest
{
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public int?      BaseEntry   { get; set; }   // ✅ FK to PurchaseOrder
    public string?   BaseType    { get; set; }   // ✅ "PurchaseOrder"
}

public class APDownPaymentRequestUpdateRequest : APDownPaymentRequestRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentRequestResponse
{
    public int       Id         { get; set; }
    public string    DocNum     { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate    { get; set; }
    public string    Status     { get; set; } = "O";
    public decimal   Total      { get; set; }
    public string?   Remarks    { get; set; }
    public DateTime  CreatedAt  { get; set; }

    public int    VendorId   { get; set; }
    public string VendorCode { get; set; } = "";
    public string VendorName { get; set; } = "";

    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}
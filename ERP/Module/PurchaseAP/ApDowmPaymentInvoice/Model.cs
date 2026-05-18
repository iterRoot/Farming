namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentInvoiceRequest
{
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public int?      BaseEntry   { get; set; }   // ✅ FK to APDownPaymentRequest
    public string?   BaseType    { get; set; }   // ✅ "APDownPaymentRequest"
}

public class APDownPaymentInvoiceUpdateRequest : APDownPaymentInvoiceRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════════════════
public class APDownPaymentInvoiceResponse
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
namespace FarmingApi.Modules.SaleAR.ARInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)
// ══════════════════════════════════════════════════════════════════════════════
public class ARInvoiceListRequest
{
    public string    DocNum      { get; set; } = "";
    public int       CustomerId  { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public string    Type        { get; set; } = "Item";
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    // ── Logistics tab ────────────────────────────────────────────────────
    public string?   ShipToAddress          { get; set; }
    public string?   BillToAddress          { get; set; }
    public string?   ShippingType           { get; set; }
    public string?   PickAndPackRemarks     { get; set; }
    public bool      Approved               { get; set; }

    // ── Accounting tab ───────────────────────────────────────────────────
    public string?   PaymentTerms           { get; set; }
    public string?   PaymentMethod          { get; set; }
    public int?      CashDiscountDateOffset { get; set; }
    public string?   BPProject              { get; set; }
    public string?   ControlAccount         { get; set; }
    public string?   JournalRemark          { get; set; }

    public List<ARInvoiceLineRequest> Items { get; set; } = new();
}

public class ARInvoiceLineRequest
{
    public int     ItemId   { get; set; }
    /// <summary>Optional — falls back to the default warehouse when omitted.</summary>
    public string? WhsCode  { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }

    /// <summary>Set when copied from another document. "Delivery" suppresses stock posting.</summary>
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}

public class ARInvoiceUpdateRequest : ARInvoiceListRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI)
// ══════════════════════════════════════════════════════════════════════════════
public class ARInvoiceListResponse
{
    public int       Id          { get; set; }
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public string    Type        { get; set; } = "Item";
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public DateTime  CreatedAt   { get; set; }

    // ── Logistics tab ────────────────────────────────────────────────────
    public string?   ShipToAddress          { get; set; }
    public string?   BillToAddress          { get; set; }
    public string?   ShippingType           { get; set; }
    public string?   PickAndPackRemarks     { get; set; }
    public bool      Approved               { get; set; }

    // ── Accounting tab ───────────────────────────────────────────────────
    public string?   PaymentTerms           { get; set; }
    public string?   PaymentMethod          { get; set; }
    public int?      CashDiscountDateOffset { get; set; }
    public string?   BPProject              { get; set; }
    public string?   ControlAccount         { get; set; }
    public string?   JournalRemark          { get; set; }

    public int     CustomerId   { get; set; }
    public string  CustomerCode { get; set; } = "";
    public string  CustomerName { get; set; } = "";

    public int?    JournalEntryId { get; set; }

    public List<ARInvoiceLineResponse>       Items       { get; set; } = new();
    public List<ARInvoiceAttachmentResponse> Attachments { get; set; } = new();
}

public class ARInvoiceLineResponse
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public string? WhsCode  { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }

    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}

public class ARInvoiceAttachmentResponse
{
    public int      Id           { get; set; }
    public string   FileName     { get; set; } = "";
    public long     FileSize     { get; set; }
    public string?  ContentType  { get; set; }
    public DateTime UploadedDate { get; set; }
}
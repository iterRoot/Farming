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

    public List<ARInvoiceLineRequest> Items { get; set; } = new();
}

public class ARInvoiceLineRequest
{
    public int     ItemId   { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
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

    public int     CustomerId   { get; set; }
    public string  CustomerCode { get; set; } = "";
    public string  CustomerName { get; set; } = "";

    public int?    JournalEntryId { get; set; }

    public List<ARInvoiceLineResponse> Items { get; set; } = new();
}

public class ARInvoiceLineResponse
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
namespace FarmingApi.Modules.SaleAR.ARDownPaymentInvoice;

public class ARDownPaymentListRequest  // ✅ renamed
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

    public List<ARDownPaymentLineRequest> Items { get; set; } = new();
}

public class ARDownPaymentLineRequest  // ✅ renamed
{
    public int     ItemId   { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

public class ARDownPaymentUpdateRequest : ARDownPaymentListRequest { }

public class ARDownPaymentListResponse  // ✅ renamed
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

    // Auto-created Journal Entry (DR AR / CR Customer Deposit).
    public int?      JournalEntryId { get; set; }
    public string?   JournalNo      { get; set; }

    public int     CustomerId   { get; set; }
    public string  CustomerCode { get; set; } = "";
    public string  CustomerName { get; set; } = "";
    public List<ARDownPaymentLineResponse> Items { get; set; } = new();
}

public class ARDownPaymentLineResponse  // ✅ renamed
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
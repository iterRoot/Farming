namespace FarmingApi.Modules.SaleAR.ARCreditNote;

public class ARCreditNoteListRequest  // ✅ renamed
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

    public List<ARCreditNoteLineRequest> Items { get; set; } = new();
}

public class ARCreditNoteLineRequest  // ✅ renamed
{
    public int     ItemId   { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

public class ARCreditNoteUpdateRequest : ARCreditNoteListRequest { }  // ✅ renamed

public class ARCreditNoteListResponse  // ✅ renamed
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
    public List<ARCreditNoteLineResponse> Items { get; set; } = new();
}

public class ARCreditNoteLineResponse  // ✅ renamed
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
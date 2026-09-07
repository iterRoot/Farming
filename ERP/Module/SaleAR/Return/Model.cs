namespace FarmingApi.Modules.SaleAR.Return;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)  ✅ renamed from SaleOrder → Return
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnListRequest
{
    public string    DocNum       { get; set; } = "";
    public int       CustomerId   { get; set; }
    public DateTime? PostingDate  { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string    Status       { get; set; } = "O";
    public string    Type         { get; set; } = "Item";
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }

    public List<ReturnLineRequest> Items { get; set; } = new();
}

public class ReturnLineRequest
{
    public int     ItemId   { get; set; }
    /// <summary>Optional — falls back to the default warehouse when omitted.</summary>
    public string? WhsCode  { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

public class ReturnUpdateRequest : ReturnListRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI)
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnListResponse
{
    public int       Id           { get; set; }
    public string    DocNum       { get; set; } = "";
    public DateTime? PostingDate  { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string    Status       { get; set; } = "O";
    public string    Type         { get; set; } = "Item";
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }
    public DateTime  CreatedAt    { get; set; }

    public int     CustomerId   { get; set; }
    public string  CustomerCode { get; set; } = "";
    public string  CustomerName { get; set; } = "";

    public List<ReturnLineResponse> Items { get; set; } = new();
}

public class ReturnLineResponse
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public string? WhsCode  { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
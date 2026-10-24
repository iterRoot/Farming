namespace FarmingApi.Modules.SaleAR.ReturnRequest;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnRequestListRequest
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

    public List<ReturnRequestLineRequest> Items { get; set; } = new();
}

public class ReturnRequestLineRequest
{
    public int     ItemId   { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
    public string? WhsCode     { get; set; }
    public string? Branch      { get; set; }
    public decimal Discount    { get; set; }
    public decimal DiscountSum { get; set; }
    public decimal Vat         { get; set; }
    public decimal VatSum      { get; set; }
    public decimal ItemCost    { get; set; }
    public string? UomName     { get; set; }
    public string? UomCode     { get; set; }
    public string? DocCurrency { get; set; }
    public decimal GrossProfit { get; set; }
    public string? Dimension1  { get; set; }
    public string? Dimension2  { get; set; }
    public string? Dimension3  { get; set; }
    public string? Dimension4  { get; set; }
    public string? Dimension5  { get; set; }
    public string? FreeText    { get; set; }
    public string? Remark      { get; set; }
    public int?    BaseEntry   { get; set; }
    public string? BaseType    { get; set; }
}

public class ReturnRequestUpdateRequest : ReturnRequestListRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI)
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnRequestListResponse
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

    public List<ReturnRequestLineResponse> Items { get; set; } = new();
}

public class ReturnRequestLineResponse
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
    public string? WhsCode     { get; set; }
    public string? Branch      { get; set; }
    public decimal Discount    { get; set; }
    public decimal DiscountSum { get; set; }
    public decimal Vat         { get; set; }
    public decimal VatSum      { get; set; }
    public decimal ItemCost    { get; set; }
    public string? UomName     { get; set; }
    public string? UomCode     { get; set; }
    public string? DocCurrency { get; set; }
    public decimal GrossProfit { get; set; }
    public string? Dimension1  { get; set; }
    public string? Dimension2  { get; set; }
    public string? Dimension3  { get; set; }
    public string? Dimension4  { get; set; }
    public string? Dimension5  { get; set; }
    public string? FreeText    { get; set; }
    public string? Remark      { get; set; }
    public int?    BaseEntry   { get; set; }
    public string? BaseType    { get; set; }
}
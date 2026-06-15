namespace FarmingApi.Modules.Inventory.GoodsIssue;

// ── LIST RESPONSE ─────────────────────────────────────────────
public class GoodsIssueListResponse
{
    public int      Id           { get; set; }
    public string?  DocNo        { get; set; }
    public string?  Status       { get; set; }
    public string?  CustomerCode { get; set; }
    public string?  CustomerName { get; set; }
    public string?  Remarks      { get; set; }
    public int?     BaseDocId    { get; set; }
    public string?  BaseDocType  { get; set; }
    public DateTime PostingDate  { get; set; }
    public DateTime DocumentDate { get; set; }
    public decimal  GrandTotal   { get; set; }
    public DateTime CreatedAt    { get; set; }
    public List<GoodsIssueLineResponse> Lines { get; set; } = new();
}

public class GoodsIssueLineResponse
{
    public int     Id       { get; set; }
    public string  ItemCode { get; set; } = null!;
    public string? ItemName { get; set; }
    public decimal Qty      { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

// ── INSERT REQUEST ────────────────────────────────────────────
public class GoodsIssueInsertRequest
{
    public int?      VendorId     { get; set; }
    public string?   CustomerCode { get; set; }
    public string?   CustomerName { get; set; }
    public DateTime? PostingDate  { get; set; }
    public DateTime? DocDate      { get; set; }
    public string?   Status       { get; set; }
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   TotalAmount  { get; set; }
    public string?   Remarks      { get; set; }
    public int?      BaseDocId    { get; set; }
    public string?   BaseDocType  { get; set; }
    public List<GoodsIssueLineRequest> Lines { get; set; } = new();
}

public class GoodsIssueLineRequest
{
    public int     ItemId   { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

// ── UPDATE REQUEST ────────────────────────────────────────────
public class GoodsIssueUpdateRequest : GoodsIssueInsertRequest { }
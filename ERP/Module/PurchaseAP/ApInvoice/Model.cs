namespace FarmingApi.Modules.PurchaseAP.APInvoice;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST
// ══════════════════════════════════════════════════════════════════════════════
public class APInvoiceRequest : FarmingApi.Core.IHasBuyer
{
    public string? BuyerCode { get; set; }
    public string    DocNum      { get; set; } = "";
    public int       VendorId    { get; set; }
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";
    public string    Type        { get; set; } = "Item";
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    public List<APInvoiceLineRequest> Items { get; set; } = new();
}

public class APInvoiceLineRequest
{
    /// <summary>Batch / serial numbers for this line — required when the item is managed by them.</summary>
    public List<FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchLine>? Batches { get; set; }

    public int     ItemId    { get; set; }
    /// <summary>Optional — falls back to the default warehouse when omitted.</summary>
    public string? WhsCode   { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }   // ✅ FK to GoodsReceiptPO
    public string? BaseType  { get; set; }   // ✅ "GoodsReceiptPO"
}

public class APInvoiceUpdateRequest : APInvoiceRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════════════════
public class APInvoiceResponse
{
    public string? BuyerCode { get; set; }
    public int       Id         { get; set; }
    public string    DocNum     { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate    { get; set; }
    public string    Status     { get; set; } = "O";
    public string    Type        { get; set; } = "Item";
    public decimal   Discount   { get; set; }
    public decimal   Tax        { get; set; }
    public decimal   TaxAmount  { get; set; }
    public decimal   Total      { get; set; }
    public string?   Remarks    { get; set; }
    public DateTime  CreatedAt  { get; set; }

    public int    VendorId   { get; set; }
    public string VendorCode { get; set; } = "";
    public string VendorName { get; set; } = "";

    public List<APInvoiceLineResponse> Items { get; set; } = new();
}

public class APInvoiceLineResponse
{
    public int     Id        { get; set; }
    public int     ItemId    { get; set; }
    public string  ItemCode  { get; set; } = "";
    public string? ItemName  { get; set; }
    public string? WhsCode   { get; set; }
    public decimal Quantity  { get; set; }
    public decimal Price     { get; set; }
    public decimal Total     { get; set; }
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
}
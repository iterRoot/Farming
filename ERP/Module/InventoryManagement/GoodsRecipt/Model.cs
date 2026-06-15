// ═══════════════════════════════════════════════════════════════
// FILE 1: Model.cs  — updated request/response matching frontend
// ═══════════════════════════════════════════════════════════════

namespace FarmingApi.Modules.Inventory.GoodsReceipt;

// ── LIST RESPONSE (GET) ──────────────────────────────────────────
public class GoodsReceiptListResponse
{
    public int      Id             { get; set; }
    public string   CustomerCode   { get; set; } = null!;
    public string   CustomerName   { get; set; } = null!;
    public int      DocumentNumber { get; set; }
    public string?  DocNo          { get; set; }   // ✅ "GRC-2026-00001"
    public string?  Status         { get; set; }
    public DateTime PostingDate    { get; set; }
    public DateTime DocumentDate   { get; set; }
    public decimal  GrandTotal     { get; set; }
    public string?  Remarks        { get; set; }
    public List<GoodsReceiptLineResponse> Lines { get; set; } = new();
}

public class GoodsReceiptLineResponse
{
    public int     Id       { get; set; }
    public string  ItemCode { get; set; } = null!;
    public string? ItemName { get; set; }
    public decimal Qty      { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

// ── INSERT REQUEST (POST) ────────────────────────────────────────
// ✅ Matches exactly what the frontend sends
public class GoodsReceiptInsertListRequest
{
    // Vendor info — frontend sends vendorId; we look up code/name in controller
    public int?     VendorId       { get; set; }
    public string?  CustomerCode   { get; set; }  // optional override
    public string?  CustomerName   { get; set; }  // optional override

    public DateTime? PostingDate   { get; set; }
    public DateTime? DocDate       { get; set; }  // → DocumentDate
    public string?   Status        { get; set; }
    public decimal   Discount      { get; set; }
    public decimal   Tax           { get; set; }
    public decimal   TaxAmount     { get; set; }
    public decimal   TotalAmount   { get; set; }  // → GrandTotal
    public string?   Remarks       { get; set; }

    // Lines from frontend: { itemId, quantity, price, total }
    public List<GoodsReceiptLineRequest> Lines { get; set; } = new();
}

public class GoodsReceiptLineRequest
{
    public int     ItemId   { get; set; }
    public string? ItemCode { get; set; }   // auto-filled in controller
    public string? ItemName { get; set; }   // auto-filled in controller
    public decimal Quantity { get; set; }   // → Qty
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
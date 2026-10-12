namespace FarmingApi.Modules.PurchaseAP.LandingCost;

public static class LandedCostTypes
{
    public static readonly string[] All = { "Freight", "CustomsDuty", "Insurance", "Handling", "Other" };
}

// ── Response ─────────────────────────────────────────────────────────────
public class LandedCostResponse
{
    public int       Id          { get; set; }
    public string    DocNum      { get; set; } = null!;
    public DateTime? PostingDate { get; set; }
    public DateTime? DocDate     { get; set; }
    public string    Status      { get; set; } = null!;
    public int       VendorId    { get; set; }
    public string?   VendorName  { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }

    public List<LandedCostBaseDocResponse> BaseDocs  { get; set; } = new();
    public List<LandedCostLineResponse>    CostLines { get; set; } = new();
}

public class LandedCostBaseDocResponse
{
    public int    Id               { get; set; }
    public int    GoodsReceiptPOId { get; set; }
    public string? DocNum          { get; set; }
    public decimal? Total          { get; set; }
}

public class LandedCostLineResponse
{
    public int     Id       { get; set; }
    public string  CostType { get; set; } = null!;
    public decimal Amount   { get; set; }
    public string? Remarks  { get; set; }
}

// One row per Goods Receipt PO line the allocation touched — returned by
// Post so the caller can see exactly how the total was split, not just trust it.
public class LandedCostAllocationResponse
{
    public string  ItemCode        { get; set; } = null!;
    public string? ItemName        { get; set; }
    public decimal Quantity        { get; set; }
    public decimal LineValue       { get; set; }
    public decimal AllocatedCost   { get; set; }
    public decimal UnitCostAdded   { get; set; }
    public decimal NewItemCost     { get; set; }
}

// ── Request ──────────────────────────────────────────────────────────────
public class LandedCostRequest
{
    public DateTime?             PostingDate      { get; set; }
    public DateTime?             DocDate          { get; set; }
    public int                   VendorId         { get; set; }
    public string?                Remarks         { get; set; }
    public List<int>              GoodsReceiptPOIds { get; set; } = new();
    public List<LandedCostLineRequest> CostLines  { get; set; } = new();
}

public class LandedCostLineRequest
{
    public string  CostType { get; set; } = "Freight";
    public decimal Amount   { get; set; }
    public string? Remarks  { get; set; }
}

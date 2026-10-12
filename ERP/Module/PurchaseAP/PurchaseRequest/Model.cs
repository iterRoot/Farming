namespace FarmingApi.Modules.PurchaseAP.PurchaseRequest;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseRequestRequest
{
    public string    DocNum       { get; set; } = "";
    public int?      VendorId     { get; set; }
    public string?   RequestedBy  { get; set; }
    public string?   Department   { get; set; }
    public DateTime? PostingDate  { get; set; }
    public DateTime? NeededByDate { get; set; }
    public string    Status       { get; set; } = "O";   // "D" saves as Draft — see Controller.Create
    public string    Type         { get; set; } = "Item";
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }

    public List<PurchaseRequestLineRequest> Items { get; set; } = new();
}

public class PurchaseRequestLineRequest
{
    public int     ItemId   { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
    public string? Remark   { get; set; }
}

public class PurchaseRequestUpdateRequest : PurchaseRequestRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI)
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseRequestResponse
{
    public int       Id           { get; set; }
    public string    DocNum       { get; set; } = "";
    public DateTime? PostingDate  { get; set; }
    public DateTime? NeededByDate { get; set; }
    public string    Status       { get; set; } = "O";
    public string    Type         { get; set; } = "Item";
    public string?   RequestedBy  { get; set; }
    public string?   Department   { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }
    public DateTime  CreatedAt    { get; set; }

    // From BP join — null when no vendor has been chosen yet
    public int?    VendorId   { get; set; }
    public string? VendorCode { get; set; }
    public string? VendorName { get; set; }

    public List<PurchaseRequestLineResponse> Items { get; set; } = new();
}

public class PurchaseRequestLineResponse
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
    public string? Remark   { get; set; }
}

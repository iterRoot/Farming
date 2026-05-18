namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)
// ══════════════════════════════════════════════════════════════════════════════
public class SaleBlanketAgreementListRequest
{
    public string    DocNum       { get; set; } = "";
    public int       CustomerId   { get; set; }   // FK → BusinessPartner
    public DateTime? PostingDate  { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string    Status       { get; set; } = "O";  // O=Open, C=Closed, D=Draft
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }

    public List<SaleBlanketAgreementLineRequest> Items { get; set; } = new();
}

public class SaleBlanketAgreementLineRequest
{
    public int     ItemId   { get; set; }   // FK → ItemsMaster
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

// ── Update reuses same shape ──────────────────────────────────────────────────
public class SaleBlanketAgreementUpdateRequest : SaleBlanketAgreementListRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI)
// ══════════════════════════════════════════════════════════════════════════════
public class SaleBlanketAgreementListResponse
{
    public int       Id           { get; set; }
    public string    DocNum       { get; set; } = "";
    public DateTime? PostingDate  { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string    Status       { get; set; } = "O";
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }
    public DateTime  CreatedAt    { get; set; }

    // From BP join
    public int     CustomerId   { get; set; }
    public string  CustomerCode { get; set; } = "";
    public string  CustomerName { get; set; } = "";

    public List<SaleBlanketAgreementLineResponse> Items { get; set; } = new();
}

public class SaleBlanketAgreementLineResponse
{
    public int     Id       { get; set; }
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
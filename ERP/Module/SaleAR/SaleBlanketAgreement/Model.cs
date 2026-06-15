namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)  — YOUR EXISTING CODE (unchanged)
// ══════════════════════════════════════════════════════════════════════════════
public class SaleBlanketAgreementListRequest
{
    public string    DocNum       { get; set; } = "";
    public int       CustomerId   { get; set; }
    public DateTime? PostingDate  { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string    Status       { get; set; } = "O";
    public decimal   Discount     { get; set; }
    public decimal   Tax          { get; set; }
    public decimal   TaxAmount    { get; set; }
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }
    public List<SaleBlanketAgreementLineRequest> Items { get; set; } = new();
}

public class SaleBlanketAgreementLineRequest
{
    public int     ItemId   { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}

public class SaleBlanketAgreementUpdateRequest : SaleBlanketAgreementListRequest { }

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI) — YOUR EXISTING CODE (unchanged)
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
    public int       CustomerId   { get; set; }
    public string    CustomerCode { get; set; } = "";
    public string    CustomerName { get; set; } = "";
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

// ══════════════════════════════════════════════════════════════════════════════
// ✅ NEW — COPY FROM RESPONSE
// Returned to Sale Order form to pre-fill lines
// ══════════════════════════════════════════════════════════════════════════════
public class CopyFromBlanketAgreementResponse
{
    public int      AgreementId     { get; set; }
    public string   AgreementDocNum { get; set; } = "";
    public int      CustomerId      { get; set; }
    public string   CustomerCode    { get; set; } = "";
    public string   CustomerName    { get; set; } = "";
    public string?  Remarks         { get; set; }
    public decimal  Discount        { get; set; }
    public decimal  Tax             { get; set; }
    public List<CopyFromBlanketAgreementLine> Lines { get; set; } = new();
}

public class CopyFromBlanketAgreementLine
{
    public int     LineId   { get; set; }   // SaleBlanketAgreementLine.Id
    public int     ItemId   { get; set; }
    public string  ItemCode { get; set; } = "";
    public string? ItemName { get; set; }
    public decimal Quantity { get; set; }   // full planned quantity
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
}
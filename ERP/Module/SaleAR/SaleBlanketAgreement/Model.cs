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
    /// <summary>Optional — the warehouse this line's stock will be issued from.</summary>
    public string? WhsCode  { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
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
    public string? WhsCode  { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price    { get; set; }
    public decimal Total    { get; set; }
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

    // Draw-down tracking — set once this line has actually been invoiced.
    public int?    ARInvoiceId    { get; set; }
    public string? ARInvoiceDocNum { get; set; }

    // Source document this line was copied from (e.g. a Sale Quotation).
    public int?    BaseEntry { get; set; }
    public string? BaseType  { get; set; }
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
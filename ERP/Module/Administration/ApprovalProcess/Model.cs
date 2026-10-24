namespace FarmingApi.Modules.Administration.ApprovalProcess;

// ═══════════════════════════════════════════════════════════════════
// DOCUMENT TYPE CATALOG — the 23 Sale-AR / Purchase-AP / Purchase
// Request document types an Approval Template can target. Values match
// the literal string each document's own controller passes into
// IApprovalGateService, not necessarily its route or folder name.
// ═══════════════════════════════════════════════════════════════════
public static class ApprovalDocumentTypes
{
    public static readonly string[] All =
    {
        "SaleQuotion", "SaleBlanketAgreement", "SaleOrder", "ArDownPaymentRequest", "ReturnRequest",
        "Delivery", "ARInvoice", "ARReserveInvoice", "ARCreditNote", "ARDownPaymentInvoice", "Return",
        "PurchaseQuotation", "PurchaseBlanketAgreement", "PurchaseOrder", "APDownPaymentRequest", "GoodsReturnRequest",
        "GoodsReceiptPO", "APInvoice", "APCreditNote", "APReserveInvoice", "APDownPaymentInvoice", "GoodsReturn",
        "PurchaseRequest",
    };
}

public static class ApprovalOperators
{
    public static readonly string[] All = { ">", ">=", "<", "<=", "==" };
}

// ═══════════════════════════════════════════════════════════════════
// CREATE / UPDATE
// ═══════════════════════════════════════════════════════════════════
public class ApprovalTemplateRequest
{
    public string      Name            { get; set; } = "";
    public string      DocumentType    { get; set; } = "";
    public bool        Active          { get; set; } = true;
    public string?      Operator        { get; set; }
    public decimal?     Amount          { get; set; }
    public string?      Remarks         { get; set; }
    public List<int>    ApproverUserIds { get; set; } = new();
}

public class ApprovalTemplateUpdateRequest : ApprovalTemplateRequest
{
}

// ═══════════════════════════════════════════════════════════════════
// READ
// ═══════════════════════════════════════════════════════════════════
public class ApprovalTemplateResponse
{
    public int       Id           { get; set; }
    public string    Name         { get; set; } = "";
    public string    DocumentType { get; set; } = "";
    public bool      Active       { get; set; }
    public string?   Operator     { get; set; }
    public decimal?  Amount       { get; set; }
    public string?   Remarks      { get; set; }
    public List<ApprovalTemplateApproverResponse> Approvers { get; set; } = new();
    public DateTime  CreatedAt    { get; set; }
    public DateTime? UpdatedAt    { get; set; }
}

public class ApprovalTemplateApproverResponse
{
    public int    UserId   { get; set; }
    public string UserName { get; set; } = "";
}

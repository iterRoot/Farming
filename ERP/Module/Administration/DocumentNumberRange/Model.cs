namespace FarmingApi.Modules.Administration.DocumentNumberRange;

// ═══════════════════════════════════════════════════════════════════
// DOCUMENT TYPE CATALOG
// All supported document types in the system
// ═══════════════════════════════════════════════════════════════════
public static class DocTypes
{
    public static readonly List<DocTypeInfo> All = new()
    {
        // ── Sale-AR ──────────────────────────────────────────────
        new("SaleBlanketAgreement", "SBA", "Sale Blanket Agreement", "🛒 Sale-AR"), // ✅ ADD
        new("SaleQuotation",         "SQ",  "Sale Quotation",          "🛒 Sale-AR"),
        new("SaleOrder",             "SO",  "Sale Order",              "🛒 Sale-AR"),
        new("Delivery",              "DN",  "Delivery Note",           "🛒 Sale-AR"),
        new("Return",                "RTN", "Return",                  "🛒 Sale-AR"),
        new("ArInvoice",             "ARI", "AR Invoice",              "🛒 Sale-AR"),
        new("ArCreditNote",          "ARCN","AR Credit Note",          "🛒 Sale-AR"),
        new("ArDownPaymentInvoice",  "ARDP","AR Down Payment",         "🛒 Sale-AR"),
        new("ArDownPaymentRequest",  "ARDR","AR Down Payment Request", "🛒 Sale-AR"),
        new("ArReserveInvoice",      "ARRI","AR Reserve Invoice",      "🛒 Sale-AR"),
        // ── Purchase-AP ───────────────────────────────────────────
        new("PurchaseBlanketAgreement", "PBA", "Purchase Blanket Agreement", "🏭 Purchase-AP"), // ✅ ADD
        new("PurchaseQuotation",     "PQ",  "Purchase Quotation",      "🏭 Purchase-AP"),
        new("PurchaseOrder",         "PO",  "Purchase Order",          "🏭 Purchase-AP"),
        new("GoodsReceiptPO",        "GR",  "Goods Receipt PO",        "🏭 Purchase-AP"),
        new("ApInvoice",             "API", "AP Invoice",              "🏭 Purchase-AP"),
        new("ApCreditNote",          "APCN","AP Credit Note",          "🏭 Purchase-AP"),
        new("ApDownPaymentInvoice",  "APDP","AP Down Payment",         "🏭 Purchase-AP"),
        new("ApDownPaymentRequest",  "APDR","AP Down Payment Request", "🏭 Purchase-AP"),
        new("ApReserveInvoice",  "APRI","AP Reserve Invoice",         "🏭 Purchase-AP"),
        new("GoodsReturn",           "GRTN","Goods Return",            "🏭 Purchase-AP"),

        // ── Inventory ─────────────────────────────────────────────
        new("GoodsIssue",            "GI",  "Goods Issue",             "📦 Inventory"),
        new("GoodsReceipt",          "GRC", "Goods Receipt (Non-PO)",  "📦 Inventory"),
        new("InventoryCounting",     "IC",  "Inventory Counting",      "📦 Inventory"),
        new("StockTransferRequest",  "STR", "Stock Transfer Request",  "📦 Inventory"),
        new("StockTransfer",         "STO", "Stock Transfer",          "📦 Inventory"),

        // ── Financials ────────────────────────────────────────────
        new("JournalEntry",          "JE",  "Journal Entry",           "💰 Financials"),
        new("IncomingPayment",       "IP",  "Incoming Payment",        "💰 Financials"),
        new("OutgoingPayment",       "OP",  "Outgoing Payment",        "💰 Financials"),
        new("BankReconciliation",    "BR",  "Bank Reconciliation",     "💰 Financials"),
        new("Budget",                "BGT", "Budget",                  "💰 Financials"),
    };

    public static DocTypeInfo? Find(string docType) =>
        All.FirstOrDefault(d => d.Key == docType);
}

public record DocTypeInfo(string Key, string DefaultPrefix, string Label, string Module);

// ═══════════════════════════════════════════════════════════════════
// RESPONSE
// ═══════════════════════════════════════════════════════════════════
public class DocumentNumberRangeResponse
{
    public int      Id               { get; set; }
    public string   DocumentType     { get; set; } = null!;
    public string   DocumentTypeCode { get; set; } = null!;
    public string   DocumentLabel    { get; set; } = null!;
    public string   Module           { get; set; } = null!;
    public string   SeriesName       { get; set; } = null!;
    public bool     IsDefault        { get; set; }
    public bool     IsLocked         { get; set; }
    public string   Prefix           { get; set; } = null!;
    public bool     IncludeYear      { get; set; }
    public int      PadLength        { get; set; }
    public int      FirstNum         { get; set; }
    public int      NextNum          { get; set; }
    public int?     LastNum          { get; set; }
    public string?  Remarks          { get; set; }
    public string   PreviewFormat    { get; set; } = null!;  // e.g. "SO-2026-00042"
    public int      UsedCount        { get; set; }           // NextNum - FirstNum
    public DateTime CreatedAt        { get; set; }
    public DateTime? UpdatedAt       { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class DocumentNumberRangeCreateRequest
{
    public string   DocumentType  { get; set; } = null!;
    public string   SeriesName    { get; set; } = "Primary";
    public bool     IsDefault     { get; set; } = true;
    public string   Prefix        { get; set; } = null!;
    public bool     IncludeYear   { get; set; } = true;
    public int      PadLength     { get; set; } = 5;
    public int      FirstNum      { get; set; } = 1;
    public int?     LastNum       { get; set; }
    public string?  Remarks       { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class DocumentNumberRangeUpdateRequest
{
    public string   SeriesName    { get; set; } = "Primary";
    public bool     IsDefault     { get; set; } = true;
    public bool     IsLocked      { get; set; } = false;
    public string   Prefix        { get; set; } = null!;
    public bool     IncludeYear   { get; set; } = true;
    public int      PadLength     { get; set; } = 5;
    public int      FirstNum      { get; set; } = 1;
    public int      NextNum       { get; set; } = 1;
    public int?     LastNum       { get; set; }
    public string?  Remarks       { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// NEXT NUMBER RESPONSE — used by other modules when creating a doc
// ═══════════════════════════════════════════════════════════════════
public class NextDocNumberResponse
{
    public string DocNo      { get; set; } = null!;
    public int    Number     { get; set; }
    public string SeriesName { get; set; } = null!;
}
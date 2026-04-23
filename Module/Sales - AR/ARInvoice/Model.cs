namespace FarmingApi.Modules.SaleAR.ARInvoice;

// ─────────────────────────────────────────────────────
// LIST RESPONSE  (table/grid view)
// ─────────────────────────────────────────────────────
public class ARInvoiceListResponse
{
    public int Id { get; set; }
    public string DocSeries { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string BPCode { get; set; } = string.Empty;
    public string BPName { get; set; } = string.Empty;
    public string? CustomerRefNo { get; set; }

    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }

    public string Currency { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public decimal BalanceDue { get; set; }

    public string? SalesEmployee { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ─────────────────────────────────────────────────────
// DETAIL RESPONSE  (single invoice full view)
// ─────────────────────────────────────────────────────
public class ARInvoiceDetailResponse
{
    public int Id { get; set; }

    // Document
    public string DocSeries { get; set; } = string.Empty;
    public string DocNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Business Partner
    public int BusinessPartnerId { get; set; }
    public string BPCode { get; set; } = string.Empty;
    public string BPName { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? CustomerRefNo { get; set; }

    // Dates
    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    // Currency & Branch
    public string Currency { get; set; } = string.Empty;
    public string? BranchCode { get; set; }

    // Logistics
    public string? ShipTo { get; set; }
    public string? ShipFrom { get; set; }
    public string? ShippingType { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentMethod { get; set; }

    // Accounting
    public string? ArAccount { get; set; }
    public string? RevenueAccount { get; set; }
    public string? TaxAccount { get; set; }
    public string? CostCenter { get; set; }
    public string? ProjectCode { get; set; }

    // Totals
    public decimal TotalBeforeDiscount { get; set; }
    public decimal DiscountPct { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Rounding { get; set; }
    public decimal DownPayment { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal BalanceDue { get; set; }

    // Misc
    public string? SalesEmployee { get; set; }
    public string? Owner { get; set; }
    public string? Remarks { get; set; }
    public bool PaymentOrderRun { get; set; }

    // Lines
    public List<ARInvoiceLineResponse> Lines { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ─────────────────────────────────────────────────────
// LINE RESPONSE
// ─────────────────────────────────────────────────────
public class ARInvoiceLineResponse
{
    public int Id { get; set; }
    public int LineNum { get; set; }
    public string ItemType { get; set; } = string.Empty;

    public string? ItemCode { get; set; }
    public string? BPCatalogueNo { get; set; }
    public string ItemDescription { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public string? StockUoM { get; set; }
    public string? UoMCode { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPct { get; set; }

    public string? VatCode { get; set; }
    public decimal VatRate { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TaxAmount { get; set; }

    public string? RevenueAccount { get; set; }
    public string? CogsDepartment { get; set; }
    public string? WarehouseCode { get; set; }
    public string? CountryRegion { get; set; }
}

// ─────────────────────────────────────────────────────
// CREATE REQUEST
// ─────────────────────────────────────────────────────
public class ARInvoiceCreateRequest
{
    // Business Partner
    public int BusinessPartnerId { get; set; }
    public string? ContactPerson { get; set; }
    public string? CustomerRefNo { get; set; }

    // Dates
    public DateTime PostingDate { get; set; } = DateTime.Today;
    public DateTime DocumentDate { get; set; } = DateTime.Today;
    public DateTime? DueDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    // Currency & Branch
    public string Currency { get; set; } = "USD";
    public string? BranchCode { get; set; }

    // Logistics
    public string? ShipTo { get; set; }
    public string? ShipFrom { get; set; }
    public string? ShippingType { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentMethod { get; set; }

    // Accounting
    public string? ArAccount { get; set; }
    public string? RevenueAccount { get; set; }
    public string? TaxAccount { get; set; }
    public string? CostCenter { get; set; }
    public string? ProjectCode { get; set; }

    // Header discount
    public decimal DiscountPct { get; set; } = 0;
    public bool PaymentOrderRun { get; set; } = false;

    // Misc
    public string? SalesEmployee { get; set; }
    public string? Owner { get; set; }
    public string? Remarks { get; set; }

    // Lines — must have at least 1
    public List<ARInvoiceLineRequest> Lines { get; set; } = new();
}

// ─────────────────────────────────────────────────────
// UPDATE REQUEST
// ─────────────────────────────────────────────────────
public class ARInvoiceUpdateRequest
{
    public string? ContactPerson { get; set; }
    public string? CustomerRefNo { get; set; }

    public DateTime PostingDate { get; set; }
    public DateTime DocumentDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? DeliveryDate { get; set; }

    public string Currency { get; set; } = "USD";
    public string? BranchCode { get; set; }

    public string? ShipTo { get; set; }
    public string? ShipFrom { get; set; }
    public string? ShippingType { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PaymentMethod { get; set; }

    public string? ArAccount { get; set; }
    public string? RevenueAccount { get; set; }
    public string? TaxAccount { get; set; }
    public string? CostCenter { get; set; }
    public string? ProjectCode { get; set; }

    public decimal DiscountPct { get; set; } = 0;
    public bool PaymentOrderRun { get; set; } = false;

    public string? SalesEmployee { get; set; }
    public string? Owner { get; set; }
    public string? Remarks { get; set; }

    // Full line replace on update
    public List<ARInvoiceLineRequest> Lines { get; set; } = new();
}

// ─────────────────────────────────────────────────────
// LINE REQUEST  (shared for create & update)
// ─────────────────────────────────────────────────────
public class ARInvoiceLineRequest
{
    public int LineNum { get; set; }
    public string ItemType { get; set; } = "Item";      // "Item" or "Service"

    public string? ItemCode { get; set; }
    public string? BPCatalogueNo { get; set; }
    public string ItemDescription { get; set; } = string.Empty;

    public decimal Quantity { get; set; } = 1;
    public string? StockUoM { get; set; }
    public string? UoMCode { get; set; }
    public decimal UnitPrice { get; set; } = 0;
    public decimal DiscountPct { get; set; } = 0;

    public string? VatCode { get; set; }

    public string? RevenueAccount { get; set; }
    public string? CogsDepartment { get; set; }
    public string? WarehouseCode { get; set; }
    public string? CountryRegion { get; set; }
}
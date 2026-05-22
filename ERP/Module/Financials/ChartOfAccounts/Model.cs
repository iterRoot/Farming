namespace FarmingApi.Modules.Financials.ChartOfAccounts;

// ═══════════════════════════════════════════════════════════════════
// SAP B1 Account Type — 8 types matching right-panel tabs
// ═══════════════════════════════════════════════════════════════════
public class AccountTypeResponse
{
    public string Code  { get; set; } = null!;
    public string Label { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// Level 1 = Root (no parent), Level 2+ requires parent
// ═══════════════════════════════════════════════════════════════════
public class ChartOfAccountsCreateRequest
{
    // Core
    public string  AcctCode     { get; set; } = null!;
    public string  AcctName     { get; set; } = null!;
    public string? FrgnName     { get; set; }
    public string? ExternalCode { get; set; }

    // Hierarchy
    public int     Level     { get; set; } = 1;  // 1 = Root
    public string? FatherNum { get; set; }

    // Classification
    public string AcctType { get; set; } = "A";  // A/L/E/I/C/X/N/T
    public bool   IsGroup  { get; set; }
    public bool   IsActive { get; set; } = true;

    // Currency
    public string Currency { get; set; } = "USD";

    // Posting
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant   { get; set; }
    public bool CashBox            { get; set; }

    // G/L Properties
    public bool Confidential   { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount    { get; set; }
    public bool Indexed        { get; set; }
    public bool RevalCurrency  { get; set; }

    // Tax
    public bool    TaxIncome { get; set; }
    public bool    ExmIncome { get; set; }
    public string? DfltVat   { get; set; }
    public string? TaxCode   { get; set; }

    // Validity
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo   { get; set; }

    // Dimensions
    public string? Project    { get; set; }
    public bool    PrjRelvnt  { get; set; }
    public bool    Dim1Relvnt { get; set; }
    public bool    Dim2Relvnt { get; set; }
    public bool    Dim3Relvnt { get; set; }
    public bool    Dim4Relvnt { get; set; }
    public bool    Dim5Relvnt { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }

    // Description
    public string? Description { get; set; }

    // Closing
    public string? ClosingAcc { get; set; }
    public string? TransCode  { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST — AcctCode, Level, FatherNum are immutable
// ═══════════════════════════════════════════════════════════════════
public class ChartOfAccountsUpdateRequest
{
    // Editable core
    public string  AcctName     { get; set; } = null!;
    public string? FrgnName     { get; set; }
    public string? ExternalCode { get; set; }

    // Classification — type can change
    public string AcctType { get; set; } = "A";
    public bool   IsGroup  { get; set; }
    public bool   IsActive { get; set; } = true;

    // Currency
    public string Currency { get; set; } = "USD";

    // Posting
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant   { get; set; }
    public bool CashBox            { get; set; }

    // G/L Properties
    public bool Confidential   { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount    { get; set; }
    public bool Indexed        { get; set; }
    public bool RevalCurrency  { get; set; }

    // Tax
    public bool    TaxIncome { get; set; }
    public bool    ExmIncome { get; set; }
    public string? DfltVat   { get; set; }
    public string? TaxCode   { get; set; }

    // Freeze
    public bool      Frozen     { get; set; }
    public DateTime? FrozenFrom { get; set; }
    public DateTime? FrozenTo   { get; set; }

    // Validity
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo   { get; set; }

    // Dimensions
    public string? Project    { get; set; }
    public bool    PrjRelvnt  { get; set; }
    public bool    Dim1Relvnt { get; set; }
    public bool    Dim2Relvnt { get; set; }
    public bool    Dim3Relvnt { get; set; }
    public bool    Dim4Relvnt { get; set; }
    public bool    Dim5Relvnt { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }

    // Description
    public string? Description { get; set; }

    // Closing
    public string? ClosingAcc { get; set; }
    public string? TransCode  { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// RESPONSE DTO
// ═══════════════════════════════════════════════════════════════════
public class ChartOfAccountsResponse
{
    public int    Id      { get; set; }

    // Core
    public string  AcctCode     { get; set; } = null!;
    public string  AcctName     { get; set; } = null!;
    public string? FrgnName     { get; set; }
    public string? ExternalCode { get; set; }

    // Hierarchy
    public int     Level     { get; set; }
    public string? FatherNum { get; set; }

    // Classification
    public string AcctType { get; set; } = null!;
    public bool   IsGroup  { get; set; }
    public bool   IsActive { get; set; }

    // Currency
    public string Currency { get; set; } = "USD";

    // Balances
    public decimal Balance       { get; set; }
    public decimal DebitBalance  { get; set; }
    public decimal CreditBalance { get; set; }

    // Posting
    public bool AllowPosting       { get; set; }
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant   { get; set; }

    // G/L Properties
    public bool Confidential   { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount    { get; set; }
    public bool Indexed        { get; set; }
    public bool RevalCurrency  { get; set; }

    // Tax
    public bool    TaxIncome { get; set; }
    public bool    ExmIncome { get; set; }
    public string? DfltVat   { get; set; }
    public string? TaxCode   { get; set; }

    // Freeze
    public bool      Frozen     { get; set; }
    public DateTime? FrozenFrom { get; set; }
    public DateTime? FrozenTo   { get; set; }

    // Validity
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo   { get; set; }

    // Dimensions
    public string? Project    { get; set; }
    public bool    PrjRelvnt  { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }

    // Description
    public string? Description { get; set; }

    // Closing
    public string? ClosingAcc { get; set; }

    // System
    public int      VersionNum { get; set; }
    public DateTime CreatedAt  { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// ── Lightweight tree response ────────────────────────────────────
public class ChartOfAccountsTreeResponse
{
    public int    Id       { get; set; }
    public string AcctCode { get; set; } = null!;
    public string AcctName { get; set; } = null!;
    public int    Level    { get; set; }
    public string? FatherNum { get; set; }
    public string AcctType { get; set; } = null!;
    public bool   IsGroup  { get; set; }
    public bool   IsActive { get; set; }
    public decimal Balance { get; set; }
    public bool   AllowPosting { get; set; }
    public List<ChartOfAccountsTreeResponse> Children { get; set; } = new();
}
using System;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public class ChartOfAccountsCreateRequest
{
    public string AcctCode { get; set; } = null!;
    public string AcctName { get; set; } = null!;
    public int Level { get; set; }
    public string? FatherNum { get; set; }
    public string AcctType { get; set; } = null!; 
    public bool IsGroup { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? TaxCode { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }
    public string? Project { get; set; }
    public bool Confidential { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount { get; set; }
    public bool Indexed { get; set; }
    public bool RevalCurrency { get; set; }
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant { get; set; }
    public bool TaxIncome { get; set; }
    public bool ExmIncome { get; set; }
    public bool IsActive { get; set; }
}

public class ChartOfAccountsUpdateRequest
{
    public string AcctName { get; set; } = null!;
    public bool IsGroup { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? TaxCode { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }
    public string? Project { get; set; }
    public bool Confidential { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount { get; set; }
    public bool Indexed { get; set; }
    public bool RevalCurrency { get; set; }
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant { get; set; }
    public bool IsActive { get; set; }
}

public class ChartOfAccountsResponse
{
    public int Id { get; set; }
    public string AcctCode { get; set; } = null!;
    public string AcctName { get; set; } = null!;
    public int Level { get; set; }
    public string? FatherNum { get; set; }
    public string AcctType { get; set; } = null!;
    public bool IsGroup { get; set; }
    public bool IsActive { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Balance { get; set; }
    public bool BlockManualPosting { get; set; }
}

public class AccountTypeResponse
{
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;
}
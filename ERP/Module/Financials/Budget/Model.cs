namespace FarmingApi.Modules.Financials.Budget;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE
// ═══════════════════════════════════════════════════════════════════
public class BudgetResponse
{
    public int      Id          { get; set; }
    public string   BudgetNo    { get; set; } = null!;
    public string   BudgetName  { get; set; } = null!;
    public string?  Description { get; set; }
    public int      FiscalYear  { get; set; }
    public DateTime StartDate   { get; set; }
    public DateTime EndDate     { get; set; }
    public string   Status      { get; set; } = null!;
    public string   Currency    { get; set; } = null!;
    public string?  Remarks     { get; set; }
    public bool     IsActive    { get; set; }
    public int      VersionNum  { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime? UpdatedAt  { get; set; }
    public List<BudgetLineResponse> Lines { get; set; } = new();

    // Computed totals
    public decimal TotalIncome  { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetBudget    { get; set; }
}

public class BudgetLineResponse
{
    public int     Id         { get; set; }
    public int     LineNum    { get; set; }
    public string  AcctCode   { get; set; } = null!;
    public string  AcctName   { get; set; } = null!;
    public string  AcctType   { get; set; } = null!;
    public decimal Annual     { get; set; }
    public decimal Jan        { get; set; }
    public decimal Feb        { get; set; }
    public decimal Mar        { get; set; }
    public decimal Apr        { get; set; }
    public decimal May        { get; set; }
    public decimal Jun        { get; set; }
    public decimal Jul        { get; set; }
    public decimal Aug        { get; set; }
    public decimal Sep        { get; set; }
    public decimal Oct        { get; set; }
    public decimal Nov        { get; set; }
    public decimal Dec        { get; set; }
    public string? CostCenter { get; set; }
    public string? Project    { get; set; }
    public string? Remarks    { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BudgetCreateRequest
{
    public string   BudgetName  { get; set; } = null!;
    public string?  Description { get; set; }
    public int      FiscalYear  { get; set; }
    public DateTime StartDate   { get; set; }
    public DateTime EndDate     { get; set; }
    public string   Currency    { get; set; } = "USD";
    public string?  Remarks     { get; set; }
    public List<BudgetLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BudgetUpdateRequest
{
    public string   BudgetName  { get; set; } = null!;
    public string?  Description { get; set; }
    public DateTime StartDate   { get; set; }
    public DateTime EndDate     { get; set; }
    public string   Currency    { get; set; } = "USD";
    public string?  Remarks     { get; set; }
    public List<BudgetLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// LINE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BudgetLineRequest
{
    public int     LineNum    { get; set; }
    public string  AcctCode   { get; set; } = null!;
    public string? AcctName   { get; set; }
    public string? AcctType   { get; set; }
    public decimal Annual     { get; set; }
    public decimal Jan        { get; set; }
    public decimal Feb        { get; set; }
    public decimal Mar        { get; set; }
    public decimal Apr        { get; set; }
    public decimal May        { get; set; }
    public decimal Jun        { get; set; }
    public decimal Jul        { get; set; }
    public decimal Aug        { get; set; }
    public decimal Sep        { get; set; }
    public decimal Oct        { get; set; }
    public decimal Nov        { get; set; }
    public decimal Dec        { get; set; }
    public string? CostCenter { get; set; }
    public string? Project    { get; set; }
    public string? Remarks    { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// BUDGET VS ACTUAL (for reporting)
// ═══════════════════════════════════════════════════════════════════
public class BudgetVsActualResponse
{
    public string  AcctCode    { get; set; } = null!;
    public string  AcctName    { get; set; } = null!;
    public string  AcctType    { get; set; } = null!;
    public decimal BudgetAmt   { get; set; }   // from BGT1
    public decimal ActualAmt   { get; set; }   // from KCOA balance
    public decimal Variance    { get; set; }   // Actual - Budget
    public decimal VariancePct { get; set; }   // Variance / Budget * 100
    public bool    IsOver      { get; set; }   // true = over budget
}
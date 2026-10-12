namespace FarmingApi.Modules.Financials.FinancialProjects;

public class FinancialProjectDto
{
    public int      Id          { get; set; }
    public string   ProjectCode { get; set; } = null!;
    public string?  ProjectName { get; set; }
    public DateOnly? ValidFrom  { get; set; }
    public DateOnly? ValidTo    { get; set; }
    public bool     Active      { get; set; } = true;
}

public class FinancialProjectSaveRequest
{
    public string   ProjectCode { get; set; } = null!;
    public string?  ProjectName { get; set; }
    public DateOnly? ValidFrom  { get; set; }
    public DateOnly? ValidTo    { get; set; }
    public bool     Active      { get; set; } = true;
}

public class ProjectSummaryRow
{
    public string  AcctCode { get; set; } = null!;
    public string  AcctName { get; set; } = null!;
    public string  AcctType { get; set; } = "";
    public decimal Budget   { get; set; }
    public decimal Debit    { get; set; }
    public decimal Credit   { get; set; }
    public decimal Actual   { get; set; }
    public decimal Variance { get; set; }   // positive = favourable
}

public class ProjectSummaryResponse
{
    public string  ProjectCode  { get; set; } = null!;
    public string? ProjectName  { get; set; }
    public int?    Year         { get; set; }
    public decimal IncomeBudget { get; set; }
    public decimal IncomeActual { get; set; }
    public decimal CostBudget   { get; set; }
    public decimal CostActual   { get; set; }
    public decimal ResultBudget { get; set; }
    public decimal ResultActual { get; set; }
    public List<ProjectSummaryRow> Rows { get; set; } = new();
}

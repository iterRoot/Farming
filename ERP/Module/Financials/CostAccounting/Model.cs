// namespace FarmingApi.Modules.Financials.CostAccounting;

// // ═══════════════════════════════════════════════════════════════════
// // COST CENTER
// // ═══════════════════════════════════════════════════════════════════
// public class CostCenterResponse
// {
//     public int       Id             { get; set; }
//     public string    CostCenterCode { get; set; } = null!;
//     public string    CostCenterName { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public string    CenterType     { get; set; } = null!;
//     public string?   ParentCode     { get; set; }
//     public string?   Manager        { get; set; }
//     public string    Currency       { get; set; } = null!;
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public bool      IsActive       { get; set; }
//     public int       VersionNum     { get; set; }
//     public decimal   TotalDebit     { get; set; }
//     public decimal   TotalCredit    { get; set; }
//     public decimal   Balance        { get; set; }
//     public DateTime  CreatedAt      { get; set; }
//     public DateTime? UpdatedAt      { get; set; }
// }

// public class CostCenterRequest
// {
//     public string    CostCenterCode { get; set; } = null!;
//     public string    CostCenterName { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public string    CenterType     { get; set; } = "D";
//     public string?   ParentCode     { get; set; }
//     public string?   Manager        { get; set; }
//     public string    Currency       { get; set; } = "USD";
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public bool      IsActive       { get; set; } = true;
// }

// public class CostCenterUpdateRequest
// {
//     public string    CostCenterName { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public string    CenterType     { get; set; } = "D";
//     public string?   ParentCode     { get; set; }
//     public string?   Manager        { get; set; }
//     public string    Currency       { get; set; } = "USD";
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public bool      IsActive       { get; set; } = true;
// }

// // ═══════════════════════════════════════════════════════════════════
// // DISTRIBUTION RULE
// // ═══════════════════════════════════════════════════════════════════
// public class DistributionRuleResponse
// {
//     public int      Id          { get; set; }
//     public string   RuleCode    { get; set; } = null!;
//     public string   RuleName    { get; set; } = null!;
//     public string?  Description { get; set; }
//     public bool     IsActive    { get; set; }
//     public bool     InUse       { get; set; }
//     public int      VersionNum  { get; set; }
//     public decimal  TotalPct    { get; set; }  // computed
//     public DateTime CreatedAt   { get; set; }
//     public DateTime? UpdatedAt  { get; set; }
//     public List<DistributionRuleLineResponse> Lines { get; set; } = new();
// }

// public class DistributionRuleLineResponse
// {
//     public int     Id             { get; set; }
//     public int     LineNum        { get; set; }
//     public string  CostCenterCode { get; set; } = null!;
//     public string  CostCenterName { get; set; } = null!;
//     public decimal Percentage     { get; set; }
//     public string? Remarks        { get; set; }
// }

// public class DistributionRuleRequest
// {
//     public string  RuleCode    { get; set; } = null!;
//     public string  RuleName    { get; set; } = null!;
//     public string? Description { get; set; }
//     public bool    IsActive    { get; set; } = true;
//     public List<DistributionRuleLineRequest> Lines { get; set; } = new();
// }

// public class DistributionRuleUpdateRequest
// {
//     public string  RuleName    { get; set; } = null!;
//     public string? Description { get; set; }
//     public bool    IsActive    { get; set; } = true;
//     public List<DistributionRuleLineRequest> Lines { get; set; } = new();
// }

// public class DistributionRuleLineRequest
// {
//     public int     LineNum        { get; set; }
//     public string  CostCenterCode { get; set; } = null!;
//     public string? CostCenterName { get; set; }
//     public decimal Percentage     { get; set; }
//     public string? Remarks        { get; set; }
// }

// // ═══════════════════════════════════════════════════════════════════
// // PROJECT
// // ═══════════════════════════════════════════════════════════════════
// public class ProjectResponse
// {
//     public int       Id             { get; set; }
//     public string    ProjectCode    { get; set; } = null!;
//     public string    ProjectName    { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public string    Status         { get; set; } = null!;
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public decimal   BudgetAmount   { get; set; }
//     public string?   Manager        { get; set; }
//     public string?   CustomerCode   { get; set; }
//     public string?   CustomerName   { get; set; }
//     public string?   CostCenterCode { get; set; }
//     public string    Currency       { get; set; } = null!;
//     public bool      IsActive       { get; set; }
//     public int       VersionNum     { get; set; }
//     public decimal   TotalDebit     { get; set; }
//     public decimal   TotalCredit    { get; set; }
//     public decimal   ActualCost     { get; set; }
//     public decimal   ActualRevenue  { get; set; }
//     public decimal   Variance       { get; set; }  // BudgetAmount - ActualCost
//     public DateTime  CreatedAt      { get; set; }
//     public DateTime? UpdatedAt      { get; set; }
// }

// public class ProjectRequest
// {
//     public string    ProjectCode    { get; set; } = null!;
//     public string    ProjectName    { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public decimal   BudgetAmount   { get; set; }
//     public string?   Manager        { get; set; }
//     public string?   CustomerCode   { get; set; }
//     public string?   CustomerName   { get; set; }
//     public string?   CostCenterCode { get; set; }
//     public string    Currency       { get; set; } = "USD";
// }

// public class ProjectUpdateRequest
// {
//     public string    ProjectName    { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public decimal   BudgetAmount   { get; set; }
//     public string?   Manager        { get; set; }
//     public string?   CustomerCode   { get; set; }
//     public string?   CustomerName   { get; set; }
//     public string?   CostCenterCode { get; set; }
//     public string    Currency       { get; set; } = "USD";
//     public bool      IsActive       { get; set; } = true;
// }
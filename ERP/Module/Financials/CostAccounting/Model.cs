namespace FarmingApi.Modules.Financials.CostAccounting;

// ═══════════════════════════════════════════════════════════════════
// COST CENTRE
// ═══════════════════════════════════════════════════════════════════
public class CostCenterResponse
{
    public int       Id             { get; set; }
    public string    CostCenterCode { get; set; } = null!;
    public string    CostCenterName { get; set; } = null!;
    public string?   Description    { get; set; }
    public string    CenterType     { get; set; } = null!;
    public string?   ParentCode     { get; set; }
    public string?   Manager        { get; set; }
    public string    Currency       { get; set; } = null!;
    public DateTime? StartDate      { get; set; }
    public DateTime? EndDate        { get; set; }
    public bool      IsActive       { get; set; }
    public int       VersionNum     { get; set; }
    public decimal   TotalDebit     { get; set; }
    public decimal   TotalCredit    { get; set; }
    public decimal   Balance        { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public DateTime? UpdatedAt      { get; set; }
}

public class CostCenterRequest
{
    public string    CostCenterCode { get; set; } = null!;
    public string    CostCenterName { get; set; } = null!;
    public string?   Description    { get; set; }
    public string    CenterType     { get; set; } = "D";
    public string?   ParentCode     { get; set; }
    public string?   Manager        { get; set; }
    public string    Currency       { get; set; } = "USD";
    public DateTime? StartDate      { get; set; }
    public DateTime? EndDate        { get; set; }
    public bool      IsActive       { get; set; } = true;
}

public class CostCenterUpdateRequest
{
    public string    CostCenterName { get; set; } = null!;
    public string?   Description    { get; set; }
    public string    CenterType     { get; set; } = "D";
    public string?   ParentCode     { get; set; }
    public string?   Manager        { get; set; }
    public string    Currency       { get; set; } = "USD";
    public DateTime? StartDate      { get; set; }
    public DateTime? EndDate        { get; set; }
    public bool      IsActive       { get; set; } = true;
}

// ═══════════════════════════════════════════════════════════════════
// DISTRIBUTION RULE
// ═══════════════════════════════════════════════════════════════════
public class DistributionRuleResponse
{
    public int       Id          { get; set; }
    public string    RuleCode    { get; set; } = null!;
    public string    RuleName    { get; set; } = null!;
    public string?   Description  { get; set; }
    public bool      IsActive     { get; set; }
    public bool      InUse        { get; set; }
    public int       VersionNum   { get; set; }
    public decimal   TotalPct     { get; set; }   // computed — sum of the line percentages
    public DateTime  CreatedAt    { get; set; }
    public DateTime? UpdatedAt    { get; set; }
    public List<DistributionRuleLineResponse> Lines { get; set; } = new();
}

public class DistributionRuleLineResponse
{
    public int     Id             { get; set; }
    public int     LineNum        { get; set; }
    public string  CostCenterCode { get; set; } = null!;
    public string  CostCenterName { get; set; } = null!;
    public decimal Percentage     { get; set; }
    public string? Remarks        { get; set; }
}

public class DistributionRuleRequest
{
    public string  RuleCode    { get; set; } = null!;
    public string  RuleName    { get; set; } = null!;
    public string? Description  { get; set; }
    public bool    IsActive     { get; set; } = true;
    public List<DistributionRuleLineRequest> Lines { get; set; } = new();
}

public class DistributionRuleUpdateRequest
{
    public string  RuleName    { get; set; } = null!;
    public string? Description  { get; set; }
    public bool    IsActive     { get; set; } = true;
    public List<DistributionRuleLineRequest> Lines { get; set; } = new();
}

public class DistributionRuleLineRequest
{
    public int     LineNum        { get; set; }
    public string  CostCenterCode { get; set; } = null!;
    public string? CostCenterName { get; set; }
    public decimal Percentage     { get; set; }
    public string? Remarks        { get; set; }
}

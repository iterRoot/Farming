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

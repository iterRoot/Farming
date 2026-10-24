namespace FarmingApi.Modules.CRM.ServiceContract;

public static class ServiceContractOptions
{
    public static readonly string[] Types    = { "Standard", "Extended", "Premium", "Custom" };
    public static readonly string[] Statuses = { "Active", "Cancelled" };
}

public class ServiceContractResponse
{
    public int      Id                { get; set; }
    public string   ContractNo        { get; set; } = null!;
    public string   CustomerCode      { get; set; } = null!;
    public string   CustomerName      { get; set; } = null!;
    public string?  ItemCode          { get; set; }
    public string?  ItemName          { get; set; }
    public string?  SerialNumber      { get; set; }
    public string   ContractType      { get; set; } = null!;
    public string   Status            { get; set; } = null!;    // as stored: Active/Cancelled
    public bool     IsExpired         { get; set; }             // computed: Status==Active && EndDate < today
    public string   EffectiveStatus   { get; set; } = null!;    // Active/Expired/Cancelled — what the UI shows
    public DateTime StartDate         { get; set; }
    public DateTime EndDate           { get; set; }
    public int      ResponseTimeHours { get; set; }
    public decimal  Value             { get; set; }
    public string?  CoverageTerms     { get; set; }
    public string?  Remarks           { get; set; }
    public DateTime CreatedAt         { get; set; }
    public DateTime? UpdatedAt        { get; set; }
}

public class ServiceContractRequest
{
    public string   CustomerCode      { get; set; } = null!;
    public string?  ItemCode          { get; set; }
    public string?  SerialNumber      { get; set; }
    public string   ContractType      { get; set; } = "Standard";
    public DateTime StartDate         { get; set; }
    public DateTime EndDate           { get; set; }
    public int      ResponseTimeHours { get; set; } = 24;
    public decimal  Value             { get; set; }
    public string?  CoverageTerms     { get; set; }
    public string?  Remarks           { get; set; }
}

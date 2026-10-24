namespace FarmingApi.Modules.CRM.ServiceCall;

public static class ServiceCallOptions
{
    public static readonly string[] Statuses  = { "Open", "InProgress", "OnHold", "Closed" };
    public static readonly string[] Priorities = { "Low", "Medium", "High", "Urgent" };
}

public class ServiceCallResponse
{
    public int      Id                { get; set; }
    public string   ServiceCallNo     { get; set; } = null!;
    public string   CustomerCode      { get; set; } = null!;
    public string   CustomerName      { get; set; } = null!;
    public string?  ContactPerson     { get; set; }
    public string?  ItemCode          { get; set; }
    public string?  ItemName          { get; set; }
    public string?  SerialNumber      { get; set; }
    public string   Subject           { get; set; } = null!;
    public string?  ProblemDescription{ get; set; }
    public string   Status            { get; set; } = null!;
    public string   Priority          { get; set; } = null!;
    public string?  AssignedTo        { get; set; }
    public DateTime OpenDate          { get; set; }
    public DateTime? DueDate          { get; set; }
    public DateTime? ClosedDate       { get; set; }
    public string?  Resolution        { get; set; }
    public int?     ServiceContractId { get; set; }
    public string?  Remarks           { get; set; }
    public DateTime CreatedAt         { get; set; }
    public DateTime? UpdatedAt        { get; set; }
}

public class ServiceCallRequest
{
    public string   CustomerCode      { get; set; } = null!;
    public string?  ContactPerson     { get; set; }
    public string?  ItemCode          { get; set; }
    public string?  SerialNumber      { get; set; }
    public string   Subject           { get; set; } = null!;
    public string?  ProblemDescription{ get; set; }
    public string   Priority          { get; set; } = "Medium";
    public string?  AssignedTo        { get; set; }
    public DateTime? DueDate          { get; set; }
    public int?     ServiceContractId { get; set; }
    public string?  Remarks           { get; set; }
}

public class ServiceCallStatusRequest
{
    public string  Status     { get; set; } = null!;
    public string? Resolution { get; set; }
}

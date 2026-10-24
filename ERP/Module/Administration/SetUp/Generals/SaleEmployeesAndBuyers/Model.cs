namespace FarmingApi.Modules.Sales.SaleEmployeeBuyer;

public class SaleEmployeeBuyerResponse
{
    public int      Id                  { get; set; }
    public string   Code                { get; set; } = null!;
    public string   FirstName           { get; set; } = null!;
    public string   LastName            { get; set; } = null!;
    public string   FullName            { get; set; } = null!;  // computed
    public string   EmployeeType        { get; set; } = null!;
    public string?  Mobile              { get; set; }
    public string?  Phone               { get; set; }
    public string?  Fax                 { get; set; }
    public string?  Email               { get; set; }
    public string?  Department          { get; set; }
    public string?  Position            { get; set; }
    public string?  SalesRegion         { get; set; }
    public string?  Territory           { get; set; }
    public int?     BranchId            { get; set; }
    public int?     CommissionGroupId   { get; set; }
    public string?  CommissionGroupCode { get; set; }
    public string?  CommissionGroupName { get; set; }
    public string?  UserCode            { get; set; }
    public string?  ExternalId          { get; set; }
    public bool     IsActive            { get; set; }
    public string?  Remarks             { get; set; }
    public DateTime CreatedAt           { get; set; }
    public DateTime UpdatedAt           { get; set; }
}

public class SaleEmployeeBuyerRequest
{
    public string   Code                { get; set; } = null!;
    public string   FirstName           { get; set; } = null!;
    public string   LastName            { get; set; } = null!;
    public string   EmployeeType        { get; set; } = "Both";
    public string?  Mobile              { get; set; }
    public string?  Phone               { get; set; }
    public string?  Fax                 { get; set; }
    public string?  Email               { get; set; }
    public string?  Department          { get; set; }
    public string?  Position            { get; set; }
    public string?  SalesRegion         { get; set; }
    public string?  Territory           { get; set; }
    public int?     BranchId            { get; set; }
    public int?     CommissionGroupId   { get; set; }
    public string?  CommissionGroupCode { get; set; }
    public string?  CommissionGroupName { get; set; }
    public string?  UserCode            { get; set; }
    public string?  ExternalId          { get; set; }
    public bool     IsActive            { get; set; } = true;
    public string?  Remarks             { get; set; }
}
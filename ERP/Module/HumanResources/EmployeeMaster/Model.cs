namespace FarmingApi.Modules.HumanResources.EmployeeMaster;

public class EmployeeMasterResponse
{
    public int      Id              { get; set; }
    public string   Code            { get; set; } = null!;
    public string   FirstName       { get; set; } = null!;
    public string   LastName        { get; set; } = null!;
    public string   FullName        { get; set; } = null!;  // computed
    public string   EmployeeType    { get; set; } = null!;
    public string?  JobTitle        { get; set; }
    public string?  Department       { get; set; }
    public string?  Branch           { get; set; }
    public string?  Manager          { get; set; }
    public string?  Gender           { get; set; }
    public DateTime? DateOfBirth     { get; set; }
    public string?  Nationality      { get; set; }
    public string?  MaritalStatus    { get; set; }
    public string?  NationalId       { get; set; }
    public int      Dependents       { get; set; }
    public string?  Mobile           { get; set; }
    public string?  Phone            { get; set; }
    public string?  Email            { get; set; }
    public string?  Address          { get; set; }
    public DateTime? HireDate        { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal? Salary           { get; set; }
    public string?  BankName         { get; set; }
    public string?  BankAccountNo    { get; set; }
    public string?  UserCode         { get; set; }
    public string?  ExternalId       { get; set; }
    public bool     IsActive         { get; set; }
    public string?  Remarks          { get; set; }
    public DateTime CreatedAt        { get; set; }
    public DateTime? UpdatedAt       { get; set; }
}

public class EmployeeMasterRequest
{
    public string   Code            { get; set; } = null!;
    public string   FirstName       { get; set; } = null!;
    public string   LastName        { get; set; } = null!;
    public string   EmployeeType    { get; set; } = "Permanent";
    public string?  JobTitle        { get; set; }
    public string?  Department       { get; set; }
    public string?  Branch           { get; set; }
    public string?  Manager          { get; set; }
    public string?  Gender           { get; set; }
    public DateTime? DateOfBirth     { get; set; }
    public string?  Nationality      { get; set; }
    public string?  MaritalStatus    { get; set; }
    public string?  NationalId       { get; set; }
    public int      Dependents       { get; set; }
    public string?  Mobile           { get; set; }
    public string?  Phone            { get; set; }
    public string?  Email            { get; set; }
    public string?  Address          { get; set; }
    public DateTime? HireDate        { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal? Salary           { get; set; }
    public string?  BankName         { get; set; }
    public string?  BankAccountNo    { get; set; }
    public string?  UserCode         { get; set; }
    public string?  ExternalId       { get; set; }
    public bool     IsActive         { get; set; } = true;
    public string?  Remarks          { get; set; }
}

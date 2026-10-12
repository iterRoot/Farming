namespace FarmingApi.Modules.Administration.Licence;

public class LicensePackageResponse
{
    public int          Id           { get; set; }
    public string       Name         { get; set; } = null!;
    public string?      Code         { get; set; }
    public string?      Description   { get; set; }
    public decimal      Price         { get; set; }
    public string       BillingCycle  { get; set; } = null!;
    public int          MaxUsers      { get; set; }
    public int          TrialDays     { get; set; }
    public bool         IsActive      { get; set; }
    public bool         IsFeatured    { get; set; }
    public List<string> Modules       { get; set; } = new();
    public DateTime     CreatedAt     { get; set; }
    public DateTime?    UpdatedAt     { get; set; }
}

public class LicensePackageRequest
{
    public string        Name         { get; set; } = null!;
    public string?        Code         { get; set; }
    public string?        Description   { get; set; }
    public decimal        Price         { get; set; }
    public string?        BillingCycle  { get; set; }   // defaults to Monthly
    public int            MaxUsers      { get; set; } = 1;
    public int            TrialDays     { get; set; }
    public bool           IsActive      { get; set; } = true;
    public bool           IsFeatured    { get; set; }
    public List<string>?  Modules       { get; set; }
}

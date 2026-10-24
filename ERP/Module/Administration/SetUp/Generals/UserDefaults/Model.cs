namespace FarmingApi.Modules.Administration.UserDefaults;

public class UserDefaultResponse
{
    public int     Id                  { get; set; }
    public string  UserCode            { get; set; } = null!;
    public string  FullName            { get; set; } = null!;
    public string? Email               { get; set; }
    public string? Role                { get; set; }
    // Documents
    public string? DefaultWarehouse    { get; set; }
    public string? DefaultLocation     { get; set; }
    public string? DefaultPriceList    { get; set; }
    public string? DefaultPaymentTerms { get; set; }
    public string? DefaultShippingType { get; set; }
    // Financials
    public string? DefaultCurrency     { get; set; }
    public string? DefaultGLAccount    { get; set; }
    public string? DefaultCostCentre   { get; set; }
    public string? DefaultProject      { get; set; }
    public int?    DefaultBranch       { get; set; }
    // Sales
    public string? DefaultSalesEmployee  { get; set; }
    public string? DefaultSalesRegion    { get; set; }
    public string? DefaultARSeries       { get; set; }
    public string? DefaultCustomerGroup  { get; set; }
    // Purchasing
    public string? DefaultBuyer          { get; set; }
    public string? DefaultAPSeries       { get; set; }
    public string? DefaultVendorGroup    { get; set; }
    // Printing & UI
    public string? DefaultPrinter        { get; set; }
    public string  DateFormat            { get; set; } = null!;
    public string  DecimalSeparator      { get; set; } = null!;
    public string  ThousandSeparator     { get; set; } = null!;
    public int     DecimalPlaces         { get; set; }
    public string  Language              { get; set; } = null!;
    public string? Theme                 { get; set; }
    public bool    ShowTutorials         { get; set; }
    public bool    ConfirmOnClose        { get; set; }
    public bool    AutoSave              { get; set; }
    public int     AutoSaveMinutes       { get; set; }
    // Status
    public bool    IsActive              { get; set; }
    public string? Remarks               { get; set; }
    public DateTime CreatedAt            { get; set; }
    public DateTime UpdatedAt            { get; set; }
}

public class UserDefaultRequest
{
    public string  UserCode            { get; set; } = null!;
    public string  FullName            { get; set; } = null!;
    public string? Email               { get; set; }
    public string? Role                { get; set; }
    // Documents
    public string? DefaultWarehouse    { get; set; }
    public string? DefaultLocation     { get; set; }
    public string? DefaultPriceList    { get; set; }
    public string? DefaultPaymentTerms { get; set; }
    public string? DefaultShippingType { get; set; }
    // Financials
    public string? DefaultCurrency     { get; set; } = "KHR";
    public string? DefaultGLAccount    { get; set; }
    public string? DefaultCostCentre   { get; set; }
    public string? DefaultProject      { get; set; }
    public int?    DefaultBranch       { get; set; }
    // Sales
    public string? DefaultSalesEmployee  { get; set; }
    public string? DefaultSalesRegion    { get; set; }
    public string? DefaultARSeries       { get; set; }
    public string? DefaultCustomerGroup  { get; set; }
    // Purchasing
    public string? DefaultBuyer          { get; set; }
    public string? DefaultAPSeries       { get; set; }
    public string? DefaultVendorGroup    { get; set; }
    // Printing & UI
    public string? DefaultPrinter        { get; set; }
    public string  DateFormat            { get; set; } = "DD/MM/YYYY";
    public string  DecimalSeparator      { get; set; } = ".";
    public string  ThousandSeparator     { get; set; } = ",";
    public int     DecimalPlaces         { get; set; } = 2;
    public string  Language              { get; set; } = "en";
    public string? Theme                 { get; set; } = "light";
    public bool    ShowTutorials         { get; set; } = true;
    public bool    ConfirmOnClose        { get; set; } = true;
    public bool    AutoSave              { get; set; } = false;
    public int     AutoSaveMinutes       { get; set; } = 5;
    // Status
    public bool    IsActive              { get; set; } = true;
    public string? Remarks               { get; set; }
}
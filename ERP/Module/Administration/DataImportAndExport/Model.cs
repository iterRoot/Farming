namespace FarmingApi.Modules.Administration.DataImportAndExport;

// ═══════════════════════════════════════════════════════════════════
// COLUMN DEFINITION
// Describes one CSV column of an importable entity. Drives the
// downloadable template, the preview grid and the validation pass.
// ═══════════════════════════════════════════════════════════════════
public class TransferColumn
{
    public string  Name     { get; set; } = null!;   // CSV header, e.g. "Code"
    public string  Label    { get; set; } = null!;   // human label
    public string  Type     { get; set; } = "text";  // text | int | decimal | bool | date | enum
    public bool    Required { get; set; }
    public bool    IsKey    { get; set; }            // used to match existing rows on update
    public int?    MaxLength{ get; set; }
    public string? Hint     { get; set; }            // shown in the template's comment row
    public List<string>? Options { get; set; }       // for Type == "enum"
    public decimal? Min     { get; set; }            // inclusive, numeric types
    public decimal? Max     { get; set; }            // inclusive, numeric types

    public TransferColumn() { }

    public TransferColumn(string name, string label, string type = "text",
                          bool required = false, bool isKey = false,
                          int? maxLength = null, string? hint = null,
                          List<string>? options = null,
                          decimal? min = null, decimal? max = null)
    {
        Name = name; Label = label; Type = type; Required = required;
        IsKey = isKey; MaxLength = maxLength; Hint = hint; Options = options;
        Min = min; Max = max;
    }
}

// ═══════════════════════════════════════════════════════════════════
// ENTITY CATALOG ENTRY
// ═══════════════════════════════════════════════════════════════════
public class TransferEntityInfo
{
    public string Key        { get; set; } = null!;  // "BusinessPartner"
    public string Label      { get; set; } = null!;  // "Business Partner Master Data"
    public string Module     { get; set; } = null!;  // "Business Partners"
    public string Table      { get; set; } = null!;  // "KOCR"
    public bool   CanImport  { get; set; }
    public bool   CanExport  { get; set; } = true;
    public List<TransferColumn> Columns { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// CATALOG — the entities this module knows how to move.
//
// CanExport is generic (driven by the EF model), so adding an export
// is one entry here. CanImport additionally needs a handler registered
// in DataTransferService.Importers.
// ═══════════════════════════════════════════════════════════════════
public static class TransferCatalog
{
    public static readonly List<TransferEntityInfo> All = new()
    {
        new TransferEntityInfo
        {
            Key       = "BusinessPartner",
            Label     = "Business Partner Master Data",
            Module    = "Business Partners",
            Table     = "KOCR",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("Code",        "Code",         "text",    required: true, isKey: true, maxLength: 50,
                    hint: "Unique BP code, e.g. C0001"),
                new("CardName",    "Name",         "text",    required: true, maxLength: 200),
                new("FrgnName",    "Foreign Name", "text",    maxLength: 200),
                new("TypeStatus",  "Type",         "enum",    required: true,
                    options: new List<string> { "Customer", "Vendor", "Lead" }),
                new("ActiveStatus","Status",       "enum",
                    hint: "defaults to Active",
                    options: new List<string> { "Active", "Inactive", "Advanced" }),
                new("GroupCode",   "Group",        "text",    maxLength: 20),
                new("Currency",    "Currency",     "text",    maxLength: 3,
                    hint: "3-letter ISO code, e.g. USD / KHR"),
                new("VatNumber",   "VAT Number",   "text",    maxLength: 50),
                new("Tel1",        "Tel 1",        "text",    maxLength: 30),
                new("Tel2",        "Tel 2",        "text",    maxLength: 30),
                new("MobilePhone", "Mobile Phone", "text",    maxLength: 30),
                new("Fax",         "Fax",          "text",    maxLength: 30),
                new("Email",       "E-Mail",       "text",    maxLength: 100),
                new("Website",     "Web Site",     "text",    maxLength: 100),
                new("Industry",    "Industry",     "text",    maxLength: 50),
                new("Territory",   "Territory",    "text",    maxLength: 100),
                new("PayTerms",    "Payment Terms","text",    maxLength: 50),
                new("CreditLimit", "Credit Limit", "decimal", min: 0),
                new("CommitmentLimit","Commitment Limit","decimal", min: 0),
                new("TotalDiscount", "Total Discount %","decimal", min: 0, max: 100),
                new("Remarks",     "Remarks",      "text",    maxLength: 1000),
            }
        },

        new TransferEntityInfo
        {
            Key       = "ChartOfAccount",
            Label     = "Chart of Accounts",
            Module    = "Financials",
            Table     = "KCOA",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("AcctCode",   "Account Code", "text", required: true, isKey: true, maxLength: 20,
                    hint: "Unique G/L code, e.g. 1100"),
                new("AcctName",   "Account Name", "text", required: true, maxLength: 200),
                new("FrgnName",   "Foreign Name", "text", maxLength: 200),
                new("Level",      "Level",        "int",  required: true, min: 1,
                    hint: "1 = root. A level-2+ account needs a parent"),
                new("FatherNum",  "Parent Code",  "text", maxLength: 20,
                    hint: "AcctCode of the parent — leave blank at level 1"),
                new("AcctType",   "Type",         "enum", required: true,
                    hint: "A=Assets L=Liabilities M=Memo E=Capital I=Turnover " +
                          "C=CostOfSales X=OperatingCosts N=NonOperating T=Taxation",
                    options: new List<string> { "A", "L", "M", "E", "I", "C", "X", "N", "T" }),
                new("IsGroup",    "Title Account","bool",
                    hint: "Yes = header/title account that can have children"),
                new("IsActive",   "Active",       "bool", hint: "defaults to Yes"),
                new("Currency",   "Currency",     "text", maxLength: 3, hint: "defaults to USD"),
                new("AllowPosting","Allow Posting","bool", hint: "defaults to Yes"),
                new("BlockManualPosting","Block Manual Posting","bool"),
                new("ControlAccount","Control Account","bool"),
                new("CashAccount",  "Cash Account",  "bool"),
                new("CashFlowRelevant","Cash Flow Relevant","bool"),
                new("Confidential", "Confidential",  "bool"),
                new("DfltVat",      "Default VAT",   "text", maxLength: 20),
                new("TaxCode",      "Tax Code",      "text", maxLength: 20),
                new("Project",      "Project",       "text", maxLength: 50),
                new("CostCenter",   "Cost Center",   "text", maxLength: 50),
                new("Department",   "Department",    "text", maxLength: 50),
                new("ExternalCode", "External Code", "text", maxLength: 50),
                new("Description",  "Description",   "text", maxLength: 500),
                // Balances are derived from postings — deliberately not importable.
            }
        },

        new TransferEntityInfo
        {
            Key       = "ItemMaster",
            Label     = "Item Master Data",
            Module    = "Inventory",
            Table     = "KITM",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("ItemCode",    "Item Code",    "text", required: true, isKey: true, maxLength: 50,
                    hint: "Unique item code, e.g. ITM-0001"),
                new("ItemName",    "Item Name",    "text", required: true, maxLength: 100),
                new("ForeignName", "Foreign Name", "text", maxLength: 150),
                new("ItemGroup",   "Item Group",   "text", maxLength: 100),
                new("ItemType",    "Item Type",    "enum",
                    hint: "defaults to Inventory",
                    options: new List<string> { "Inventory", "Sales", "Labor", "Travel", "Fixed Asset" }),
                new("Status",      "Status",       "enum",
                    hint: "defaults to Active",
                    options: new List<string> { "Active", "InActive" }),
                new("Barcode",     "Barcode",      "text", maxLength: 100),
                new("Manufacturer","Manufacturer", "text", maxLength: 100),

                // ── Flags ─────────────────────────────────────────
                new("SalesItem",   "Sales Item",    "bool", hint: "defaults to Yes"),
                new("PurchaseItem","Purchase Item", "bool", hint: "defaults to Yes"),
                new("StockItem",   "Stock Item",    "bool", hint: "defaults to Yes"),

                // ── Units ─────────────────────────────────────────
                new("UomGroup",    "UoM Group",    "text", maxLength: 50),
                new("UomCode",     "UoM Code",     "text", maxLength: 50),
                new("UomName",     "UoM Name",     "text", maxLength: 50),

                // ── Inventory ─────────────────────────────────────
                new("ManageBy",       "Manage By",       "enum",
                    hint: "defaults to None",
                    options: new List<string> { "None", "Serial", "Batch" }),
                new("Warehouse",      "Warehouse",       "text", maxLength: 100),
                new("MinStock",       "Min Stock",       "decimal", min: 0),
                new("MaxStock",       "Max Stock",       "decimal", min: 0),
                new("ValuationMethod","Valuation Method","enum",
                    hint: "defaults to MovingAverage",
                    options: new List<string> { "MovingAverage", "Standard", "FIFO" }),

                // ── Sales ─────────────────────────────────────────
                new("SalePrice",   "Sale Price",   "decimal", min: 0),
                new("PriceList",   "Price List",   "text", maxLength: 100),
                new("SaleTaxGroup","Sale Tax Group","text", maxLength: 50),
                new("SaleDiscount","Sale Discount %","decimal", min: 0, max: 100),
                new("SaleUom",     "Sales UoM",    "text", maxLength: 50),
                new("SaleDescription","Sales Description","text", maxLength: 250),

                // ── Purchasing ────────────────────────────────────
                new("PurchasePrice",   "Purchase Price",   "decimal", min: 0),
                new("PreferredVendor", "Preferred Vendor", "text", maxLength: 100,
                    hint: "Business Partner code"),
                new("VendorItemNo",    "Vendor Item No",   "text", maxLength: 100),
                new("PurchaseUom",     "Purchase UoM",     "text", maxLength: 50),
                new("LeadTime",        "Lead Time (days)", "int",  min: 0),
                new("PurchaseTaxGroup","Purchase Tax Group","text", maxLength: 50),

                // ── Other ─────────────────────────────────────────
                new("CountryOfOrigin", "Country of Origin","text", maxLength: 100),
                new("ShippingType",    "Shipping Type",    "text", maxLength: 50),
                new("Remarks",         "Remarks",          "text", maxLength: 1000),
                // On-hand quantity and cost come from stock movements —
                // deliberately not importable.
            }
        },

        // ───────────────────────────────────────────────────────────
        // COMPANY DETAILS — the single company record (System
        // Initialization ▸ Company Details). It is a singleton, so the
        // file carries exactly one data row; a second row is rejected.
        // Every column maps 1:1 to a field on the Company entity.
        // ───────────────────────────────────────────────────────────
        new TransferEntityInfo
        {
            Key       = "CompanyDetails",
            Label     = "Company Details",
            Module    = "Administration",
            Table     = "Company",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                // ── General ▸ local language block ──────────────────
                new("LocalCompanyName",      "Company Name",            "text", required: true, maxLength: 200,
                    hint: "The one company record — a file with more than one data row is rejected"),
                new("LocalStreet",           "Street",                  "text", maxLength: 200),
                new("LocalStreetNo",         "Street No.",              "text", maxLength: 200),
                new("LocalBlock",            "Block",                   "text", maxLength: 200),
                new("LocalBuildingFloorRoom","Building/Floor/Room",     "text", maxLength: 200),
                new("LocalCity",             "City",                    "text", maxLength: 200),
                new("LocalPostcode",         "Postal Code",             "text", maxLength: 200),
                new("LocalCounty",           "County",                  "text", maxLength: 200),
                new("LocalState",            "State",                   "text", maxLength: 200),
                new("LocalCountryRegion",    "Country/Region",          "text", maxLength: 200),
                new("LocalInternetAddress",  "Internet Address",        "text", maxLength: 200),
                new("LocalPrintingHeader",   "Printing Header",         "text", maxLength: 200),
                new("LocalActiveManager",    "Active Manager",          "text", maxLength: 200),
                new("LocalAliasName",        "Alias Name",              "text", maxLength: 200),
                new("LocalFax",              "Fax",                     "text", maxLength: 200),
                new("LocalEmail",            "E-Mail",                  "text", maxLength: 200),
                new("LocalGln",              "GLN",                     "text", maxLength: 200),

                // ── General ▸ foreign language block ────────────────
                new("ForeignCompanyName",      "Company Name (Foreign)",       "text", maxLength: 200),
                new("ForeignStreet",           "Street (Foreign)",             "text", maxLength: 200),
                new("ForeignStreetNo",         "Street No. (Foreign)",         "text", maxLength: 200),
                new("ForeignBlock",            "Block (Foreign)",              "text", maxLength: 200),
                new("ForeignBuildingFloorRoom","Building/Floor/Room (Foreign)","text", maxLength: 200),
                new("ForeignCity",             "City (Foreign)",               "text", maxLength: 200),
                new("ForeignPostcode",         "Postal Code (Foreign)",        "text", maxLength: 200),
                new("ForeignCounty",           "County (Foreign)",             "text", maxLength: 200),
                new("ForeignState",            "State (Foreign)",              "text", maxLength: 200),
                new("ForeignCountryRegion",    "Country/Region (Foreign)",     "text", maxLength: 200),
                new("ForeignInternetAddress",  "Internet Address (Foreign)",   "text", maxLength: 200),
                new("ForeignPrintingHeader",   "Printing Header (Foreign)",    "text", maxLength: 200),
                new("ForeignActiveManager",    "Active Manager (Foreign)",     "text", maxLength: 200),
                new("ForeignAliasName",        "Alias Name (Foreign)",         "text", maxLength: 200),
                new("ForeignFax",              "Fax (Foreign)",               "text", maxLength: 200),
                new("ForeignEmail",            "E-Mail (Foreign)",            "text", maxLength: 200),
                new("ForeignGln",              "GLN (Foreign)",              "text", maxLength: 200),

                // ── Accounting Data ────────────────────────────────
                new("TaxOffice",          "Tax Office",              "text",    maxLength: 200),
                new("VatNumber1",         "VAT Number 1",            "text",    maxLength: 200),
                new("VatNumber2",         "VAT Number 2",            "text",    maxLength: 200),
                new("VatNumber3",         "VAT Number 3",            "text",    maxLength: 200),
                new("CompanyRegNo",       "Company Reg. No.",        "text",    maxLength: 200),
                new("CompanyTaxRate",     "Company Tax Rate %",      "decimal", min: 0, max: 100),
                new("ExemptionNumber",    "Exemption Number",        "text",    maxLength: 200),
                new("TaxDeductionNumber", "Tax Deduction Number",    "text",    maxLength: 200),
                new("TaxOfficial",        "Tax Official",            "text",    maxLength: 200),
                new("UseDeferredTax",     "Use Deferred Tax",        "bool"),
                new("ApplyExchangeRateOnDeferredTax", "Apply Exchange Rate on Deferred Tax", "bool"),
                new("TaxRateDetermination","Tax Rate Determination", "text",    maxLength: 200),
                new("Holidays",           "Holidays",                "text",    maxLength: 200),
                new("ExtendedTaxReporting","Extended Tax Reporting", "bool"),
                new("EoriNumber",         "EORI Number",             "text",    maxLength: 200),
                new("AllowExternalTaxCalculationOnAR", "Allow External Tax Calculation on A/R", "bool"),

                // ── Basic Initialisation ──────────────────────────
                new("ChartOfAccountsTemplate", "Chart of Accounts Template", "text", maxLength: 200),
                new("LocalCurrency",           "Local Currency",             "text", maxLength: 200,
                    hint: "3-letter ISO code, e.g. USD / KHR"),
                new("SystemCurrency",          "System Currency",            "text", maxLength: 200),
                new("DefaultAccountCurrency",  "Default Account Currency",   "text", maxLength: 200),
                new("DisplayCreditBalanceNegative",    "Display Credit Balance with Negative Sign", "bool"),
                new("UseSegmentationAccounts",         "Use Segmentation Accounts",                 "bool"),
                new("AllowNegativeReversalPosting",    "Allow Negative Amounts in Reversal",        "bool"),
                new("PermitMoreThanOneDocumentType",   "Permit More Than One Document Type",        "bool"),
                new("MultiLanguageSupport",            "Multi-Language Support",                    "bool"),
                new("UseContinuousStock",              "Use Perpetual Inventory",                   "bool"),
                new("ItemGroupsValuationMethod",       "Item Groups Valuation Method",             "text", maxLength: 200),
                new("ManageItemCostPerWarehouse",     "Manage Item Cost per Warehouse",           "bool"),
                new("UsePurchaseAccountsPosting",     "Use Purchase Accounts Posting System",      "bool"),
                new("AllowStockReleaseWithoutItemCost","Allow Stock Release Without Item Cost",    "bool"),
                new("ManageSerialBatchCostBy",         "Manage Serial/Batch Cost By",              "text", maxLength: 200),
                new("EnableSeparateNetGrossPrice",    "Enable Separate Net/Gross Price",           "bool"),
                new("OrderingParty",           "Ordering Party",             "text", maxLength: 200),
                new("DefaultBankCountry",      "Default Bank Country",       "text", maxLength: 200),
                new("DefaultBank",             "Default Bank",               "text", maxLength: 200),
                new("DefaultAccountNo",        "Default Account No.",        "text", maxLength: 200),
                new("DefaultBranch",           "Default Branch",             "text", maxLength: 200),
                new("InstallBankStatementProcessing", "Install Bank Statement Processing",         "bool"),
                new("EnableFixedAssets",              "Enable Fixed Assets",                       "bool"),
                new("CalculateDepreciationBy",        "Calculate Depreciation By",                "text", maxLength: 200),
                new("EnableMultipleBranches",         "Enable Multiple Branches",                  "bool"),
                new("MaskCreditCardNumber",           "Mask Credit Card Number",                   "bool"),
                new("EnableAdvancedGLDetermination",  "Enable Advanced G/L Account Determination", "bool"),
                new("AllowAnyAccountTypeForRevenue",  "Allow Any Account Type for Revenue",        "bool"),
                new("EnableProjectManagement",        "Enable Project Management",                 "bool"),
                new("EnablePersonalDataProtection",   "Enable Personal Data Protection",           "bool"),
                new("TermsAndConditionsFile",         "Terms & Conditions File",                  "text", maxLength: 200),
            }
        },

        // ───────────────────────────────────────────────────────────
        // USER — application logins (Administration ▸ Set Up ▸ Users)
        // Matched on UserId. Password is supplied in plain text and
        // stored hashed; leave it blank on an update to keep the
        // existing password.
        // ───────────────────────────────────────────────────────────
        new TransferEntityInfo
        {
            Key       = "User",
            Label     = "Users",
            Module    = "Administration",
            Table     = "KUSR",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("UserId",       "User ID",   "text", required: true, isKey: true, maxLength: 50,
                    hint: "Unique login id, e.g. jdoe"),
                new("UserName",     "User Name", "text", required: true, maxLength: 100),
                new("Email",        "E-Mail",    "text", required: true, maxLength: 100,
                    hint: "Must be unique across all users"),
                new("Password",     "Password",  "text",
                    hint: "Plain text, stored hashed. Min 8 chars with an uppercase, a lowercase, " +
                          "a digit and a special character (@$!%*?&). Required for a new user; " +
                          "leave blank on update to keep the current password"),
                new("UserGroup",    "User Group","text", required: true, maxLength: 100,
                    hint: "Name of an existing user group"),
                new("Phone",        "Phone",     "text", maxLength: 20),
                new("ActiveStatus", "Status",    "enum",
                    hint: "A = Active, I = Inactive; defaults to A",
                    options: new List<string> { "A", "I" }),
                new("IsLocked",     "Locked",    "bool", hint: "defaults to No"),
            }
        },

        // ───────────────────────────────────────────────────────────
        // ITEM GROUP — groups items for reporting & defaults (OITB).
        // Matched on Code (stored upper-case, as the Item Group screen does).
        // ───────────────────────────────────────────────────────────
        new TransferEntityInfo
        {
            Key       = "ItemGroup",
            Label     = "Item Groups",
            Module    = "Inventory",
            Table     = "KITB",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("Code",        "Code",        "text", required: true, isKey: true, maxLength: 50,
                    hint: "Unique group code, e.g. IG-001 — stored upper-case"),
                new("Name",        "Name",        "text", required: true, maxLength: 200),
                new("Description", "Description", "text", maxLength: 500),
                new("DefaultUom",  "Default UoM", "text", maxLength: 50,
                    hint: "Default unit of measure for items in this group"),
                new("IsActive",    "Active",      "bool", hint: "defaults to Yes"),
                new("Remarks",     "Remarks",     "text", maxLength: 500),
            }
        },

        // ───────────────────────────────────────────────────────────
        // EMPLOYEE MASTER — the HR register of people (OHEM).
        // Matched on Code (stored upper-case). Distinct from Sales
        // Employees / Buyers.
        // ───────────────────────────────────────────────────────────
        new TransferEntityInfo
        {
            Key       = "EmployeeMaster",
            Label     = "Employee Master",
            Module    = "Human Resources",
            Table     = "KEMP",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("Code",         "Code",       "text", required: true, isKey: true, maxLength: 50,
                    hint: "Unique employee code, e.g. EMP-001 — stored upper-case"),
                new("FirstName",    "First Name", "text", required: true, maxLength: 100),
                new("LastName",     "Last Name",  "text", required: true, maxLength: 100),
                new("EmployeeType", "Employee Type", "enum",
                    hint: "defaults to Permanent",
                    options: new List<string> { "Permanent", "Contract", "Temporary", "Intern" }),

                new("JobTitle",   "Job Title",  "text", maxLength: 100),
                new("Department", "Department", "text", maxLength: 100),
                new("Branch",     "Branch",     "text", maxLength: 100),
                new("Manager",    "Manager",    "text", maxLength: 100,
                    hint: "Manager's employee code or name"),

                new("Gender",        "Gender",         "enum",
                    options: new List<string> { "Male", "Female", "Other" }),
                new("DateOfBirth",   "Date of Birth",  "date", hint: "YYYY-MM-DD"),
                new("Nationality",   "Nationality",    "text", maxLength: 100),
                new("MaritalStatus", "Marital Status", "enum",
                    options: new List<string> { "Single", "Married", "Divorced", "Widowed" }),
                new("NationalId",    "National ID",    "text", maxLength: 50),
                new("Dependents",    "Dependents",     "int", min: 0, max: 20,
                    hint: "Non-working spouse + minor children — 150,000 KHR tax relief each"),

                new("Mobile",  "Mobile",  "text", maxLength: 50),
                new("Phone",   "Phone",   "text", maxLength: 50),
                new("Email",   "E-Mail",  "text", maxLength: 200),
                new("Address", "Address", "text", maxLength: 300),

                new("HireDate",        "Hire Date",        "date", hint: "YYYY-MM-DD"),
                new("TerminationDate", "Termination Date", "date", hint: "YYYY-MM-DD"),
                new("Salary",          "Salary",           "decimal", min: 0),
                new("BankName",        "Bank Name",        "text", maxLength: 150),
                new("BankAccountNo",   "Bank Account No.", "text", maxLength: 50),

                new("UserCode",   "Linked User Code", "text", maxLength: 50),
                new("ExternalId", "External ID",      "text", maxLength: 100,
                    hint: "Payroll / HR system reference"),

                new("IsActive", "Active",  "bool", hint: "defaults to Yes"),
                new("Remarks",  "Remarks", "text", maxLength: 500),
            }
        },

        // ───────────────────────────────────────────────────────────
        // ATTENDANCE — one row per employee per day (KATT).
        // Matched on Employee Code + Date; re-importing the same pair
        // updates that day's record.
        // ───────────────────────────────────────────────────────────
        new TransferEntityInfo
        {
            Key       = "Attendance",
            Label     = "Attendance",
            Module    = "Human Resources",
            Table     = "KATT",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("EmployeeCode", "Employee Code", "text", required: true, maxLength: 50,
                    hint: "Employee Master code, e.g. EMP-001"),
                new("EmployeeName", "Employee Name", "text", maxLength: 200),
                new("Date",         "Date",          "date", required: true, hint: "YYYY-MM-DD"),
                new("Status",       "Status",        "enum", required: true,
                    options: new List<string> { "Present", "Absent", "Late", "HalfDay", "Leave", "Holiday" }),
                new("CheckIn",       "Check In",       "text", maxLength: 10, hint: "e.g. 08:00"),
                new("CheckOut",      "Check Out",      "text", maxLength: 10, hint: "e.g. 17:00"),
                new("HoursWorked",   "Hours Worked",   "decimal", min: 0, max: 24),
                new("OvertimeHours", "Overtime Hours", "decimal", min: 0, max: 24),
                new("LeaveType",     "Leave Type",     "enum",
                    options: new List<string> { "Annual", "Sick", "Unpaid", "Maternity", "Other" }),
                new("Remarks",       "Remarks",        "text", maxLength: 500),
            }
        },

        // ───────────────────────────────────────────────────────────
        // PAYROLL — one row per employee per pay period (KPAY).
        // Matched on Employee Code + Period Year + Period Month.
        // Tax on Salary, NSSF, Overtime Pay, unpaid-leave and Gross/Net
        // are computed server-side (Cambodian statutory rules + that
        // month's attendance) — they are not imported.
        // ───────────────────────────────────────────────────────────
        new TransferEntityInfo
        {
            Key       = "Payroll",
            Label     = "Payroll",
            Module    = "Human Resources",
            Table     = "KPAY",
            CanImport = true,
            CanExport = true,
            Columns   = new List<TransferColumn>
            {
                new("EmployeeCode", "Employee Code", "text", required: true, maxLength: 50,
                    hint: "Employee Master code, e.g. EMP-001"),
                new("EmployeeName", "Employee Name", "text", maxLength: 200),
                new("PeriodYear",   "Period Year",   "int", required: true, min: 2000, max: 2100),
                new("PeriodMonth",  "Period Month",  "int", required: true, min: 1, max: 12),
                new("BasicSalary",  "Basic Salary",  "decimal", required: true, min: 0,
                    hint: "Monthly basic in USD"),
                new("TaxableAllowance", "Taxable Allowance", "decimal", min: 0,
                    hint: "Added to the Tax on Salary base"),
                new("ExemptAllowance",  "Exempt Allowance",  "decimal", min: 0,
                    hint: "Meal / transport within limits — not taxed"),
                new("Bonus",            "Bonus",             "decimal", min: 0),
                new("OtherDeductions",  "Other Deductions",  "decimal", min: 0,
                    hint: "Advances etc. — non-statutory"),
                new("ExchangeRate",     "Exchange Rate",     "decimal", min: 0,
                    hint: "KHR per 1 USD; blank uses the latest USD rate"),
                new("PaymentDate",      "Payment Date",      "date", hint: "YYYY-MM-DD"),
                new("PaymentMethod",    "Payment Method",    "enum",
                    options: new List<string> { "Bank", "Cash", "Cheque" }),
                new("Status",           "Status",            "enum", required: true,
                    options: new List<string> { "Draft", "Approved", "Paid" }),
                new("Remarks",          "Remarks",           "text", maxLength: 500),
            }
        },
    };

    public static TransferEntityInfo? Find(string key) =>
        All.FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));
}

// ═══════════════════════════════════════════════════════════════════
// REQUEST / RESPONSE DTOs
// ═══════════════════════════════════════════════════════════════════

/// <summary>One parsed source row plus whatever validation found.</summary>
public class TransferRowResult
{
    public int    RowNumber { get; set; }        // 1-based, excluding the header
    public string Action    { get; set; } = "";  // add | update | skip | error
    public Dictionary<string, string?> Values { get; set; } = new();
    public List<string> Errors   { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public bool IsValid => Errors.Count == 0;
}

/// <summary>Dry-run result — nothing has been written.</summary>
public class ImportPreviewResponse
{
    public string EntityKey   { get; set; } = null!;
    public string EntityLabel { get; set; } = null!;
    public string Mode        { get; set; } = "A";
    public string? FileName   { get; set; }

    public List<string>            Headers        { get; set; } = new();
    public List<string>            UnknownHeaders { get; set; } = new();
    public List<string>            MissingHeaders { get; set; } = new();
    public List<TransferRowResult> Rows           { get; set; } = new();

    public int TotalRows   { get; set; }
    public int ValidRows   { get; set; }
    public int InvalidRows { get; set; }
    public int AddCount    { get; set; }
    public int UpdateCount { get; set; }
    public bool CanCommit  { get; set; }
}

/// <summary>What actually happened after a commit.</summary>
public class ImportResultResponse
{
    public int    JobId       { get; set; }
    public string JobNo       { get; set; } = null!;
    public string Status      { get; set; } = null!;
    public string EntityKey   { get; set; } = null!;
    public string EntityLabel { get; set; } = null!;
    public int    TotalRows   { get; set; }
    public int    SuccessRows { get; set; }
    public int    FailedRows  { get; set; }
    public int    SkippedRows { get; set; }
    public int    DurationMs  { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>Row shape for the job-history grid.</summary>
public class TransferJobResponse
{
    public int      Id          { get; set; }
    public string   JobNo       { get; set; } = null!;
    public string   Direction   { get; set; } = null!;
    public string   DirectionLabel => Direction == "E" ? "Export" : "Import";
    public string   EntityKey   { get; set; } = null!;
    public string   EntityLabel { get; set; } = null!;
    public string?  FileName    { get; set; }
    public string   Format      { get; set; } = "CSV";
    public long     FileSize    { get; set; }
    public string   Mode        { get; set; } = "A";
    public string   Status      { get; set; } = "P";
    public string   StatusLabel => Status switch
    {
        "S" => "Success",
        "W" => "Completed with errors",
        "F" => "Failed",
        "R" => "Running",
        _   => "Pending",
    };
    public int       TotalRows   { get; set; }
    public int       SuccessRows { get; set; }
    public int       FailedRows  { get; set; }
    public int       SkippedRows { get; set; }
    public string?   ErrorLog    { get; set; }
    public int       DurationMs  { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? CompletedAt { get; set; }
}

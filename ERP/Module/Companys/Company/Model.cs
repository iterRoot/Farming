// using FarmingApi.Modules.Position;

namespace FarmingApi.Modules.Company;

public class CompanyListResponse
{
	public Guid GuidId { get; set; }
	public int Id { get; set; }

	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public string? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
}


public class CompanyDetailResponse
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public string? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
	// public List<PositionListResponse> Positions { get; set; } = null!;
}

public class CompanyInsertRequest
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public IFormFile? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
}


public class CompanyUpdateRequest
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public IFormFile? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
}

// ══════════════════════════════════════════════════════════════════
// "Company Details" — nested shape the Company Details screen uses:
// { local, foreign, accounting, basicInit }
// ══════════════════════════════════════════════════════════════════
public class CompanyLanguageBlock
{
    public string? CompanyName       { get; set; }
    public string? Street            { get; set; }
    public string? StreetNo          { get; set; }
    public string? Block             { get; set; }
    public string? BuildingFloorRoom { get; set; }
    public string? City              { get; set; }
    public string? Postcode          { get; set; }
    public string? County            { get; set; }
    public string? State             { get; set; }
    public string? CountryRegion     { get; set; }
    public string? InternetAddress   { get; set; }
    public string? PrintingHeader    { get; set; }
    public string? ActiveManager     { get; set; }
    public string? AliasName         { get; set; }
    public string? Fax               { get; set; }
    public string? Email             { get; set; }
    public string? Gln               { get; set; }
}

public class CompanyAccountingBlock
{
    public string? TaxOffice          { get; set; }
    public string? VatNumber1         { get; set; }
    public string? VatNumber2         { get; set; }
    public string? VatNumber3         { get; set; }
    public string? CompanyRegNo       { get; set; }
    public string? CompanyTaxRate     { get; set; }   // string in the UI ("0.00")
    public string? ExemptionNumber    { get; set; }
    public string? TaxDeductionNumber { get; set; }
    public string? TaxOfficial        { get; set; }
    public bool    UseDeferredTax     { get; set; }
    public bool    ApplyExchangeRateOnDeferredTax { get; set; }
    public string? TaxRateDetermination { get; set; }
    public string? Holidays             { get; set; }
    public bool    ExtendedTaxReporting { get; set; }
    public string? EoriNumber           { get; set; }
    public bool    AllowExternalTaxCalculationOnAR { get; set; }
}

public class CompanyBasicInitBlock
{
    public string? ChartOfAccountsTemplate { get; set; }
    public string? LocalCurrency           { get; set; }
    public string? SystemCurrency          { get; set; }
    public string? DefaultAccountCurrency  { get; set; }
    public bool    DisplayCreditBalanceNegative  { get; set; }
    public bool    UseSegmentationAccounts       { get; set; }
    public bool    AllowNegativeReversalPosting  { get; set; }
    public bool    PermitMoreThanOneDocumentType { get; set; }
    public bool    MultiLanguageSupport          { get; set; }
    public bool    UseContinuousStock            { get; set; }
    public string? ItemGroupsValuationMethod     { get; set; }
    public bool    ManageItemCostPerWarehouse    { get; set; }
    public bool    UsePurchaseAccountsPosting    { get; set; }
    public bool    AllowStockReleaseWithoutItemCost { get; set; }
    public string? ManageSerialBatchCostBy       { get; set; }
    public bool    EnableSeparateNetGrossPrice   { get; set; }
    public string? OrderingParty                 { get; set; }
    public string? DefaultBankCountry            { get; set; }
    public string? DefaultBank                   { get; set; }
    public string? DefaultAccountNo              { get; set; }
    public string? DefaultBranch                 { get; set; }
    public bool    InstallBankStatementProcessing { get; set; }
    public bool    EnableFixedAssets             { get; set; }
    public string? CalculateDepreciationBy       { get; set; }
    public bool    EnableMultipleBranches        { get; set; }
    public bool    MaskCreditCardNumber          { get; set; }
    public bool    EnableAdvancedGLDetermination { get; set; }
    public bool    AllowAnyAccountTypeForRevenue { get; set; }
    public bool    EnableProjectManagement       { get; set; }
    public bool    EnablePersonalDataProtection  { get; set; }
    public string? TermsAndConditionsFile        { get; set; }
}

public class CompanyDetailsDto
{
    public int?                    Id        { get; set; }
    public CompanyLanguageBlock    Local     { get; set; } = new();
    public CompanyLanguageBlock    Foreign   { get; set; } = new();
    public CompanyAccountingBlock  Accounting{ get; set; } = new();
    public CompanyBasicInitBlock   BasicInit { get; set; } = new();
}

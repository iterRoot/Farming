using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Company;

// SAP B1 style "Company Details" — General (local + foreign address blocks),
// Accounting Data and Basic Initialisation.
public class Company : AuditableEntity
{
    // ── Legacy / summary ──────────────────────────────────────
    public string  Name      { get; set; } = null!;
    public string? ShortName { get; set; }
    public string? Logo      { get; set; }
    public string? Desc      { get; set; }
    public string? Address   { get; set; }

    // ══════════════════════════════════════════════════════════
    // GENERAL — local language block
    // ══════════════════════════════════════════════════════════
    public string? LocalCompanyName      { get; set; }
    public string? LocalStreet           { get; set; }
    public string? LocalStreetNo         { get; set; }
    public string? LocalBlock            { get; set; }
    public string? LocalBuildingFloorRoom{ get; set; }
    public string? LocalCity             { get; set; }
    public string? LocalPostcode         { get; set; }
    public string? LocalCounty           { get; set; }
    public string? LocalState            { get; set; }
    public string? LocalCountryRegion    { get; set; }
    public string? LocalInternetAddress  { get; set; }
    public string? LocalPrintingHeader   { get; set; }
    public string? LocalActiveManager    { get; set; }
    public string? LocalAliasName        { get; set; }
    public string? LocalFax              { get; set; }
    public string? LocalEmail            { get; set; }
    public string? LocalGln              { get; set; }

    // ══════════════════════════════════════════════════════════
    // GENERAL — foreign language block
    // ══════════════════════════════════════════════════════════
    public string? ForeignCompanyName      { get; set; }
    public string? ForeignStreet           { get; set; }
    public string? ForeignStreetNo         { get; set; }
    public string? ForeignBlock            { get; set; }
    public string? ForeignBuildingFloorRoom{ get; set; }
    public string? ForeignCity             { get; set; }
    public string? ForeignPostcode         { get; set; }
    public string? ForeignCounty           { get; set; }
    public string? ForeignState            { get; set; }
    public string? ForeignCountryRegion    { get; set; }
    public string? ForeignInternetAddress  { get; set; }
    public string? ForeignPrintingHeader   { get; set; }
    public string? ForeignActiveManager    { get; set; }
    public string? ForeignAliasName        { get; set; }
    public string? ForeignFax              { get; set; }
    public string? ForeignEmail            { get; set; }
    public string? ForeignGln              { get; set; }

    // ══════════════════════════════════════════════════════════
    // ACCOUNTING DATA
    // ══════════════════════════════════════════════════════════
    public string? TaxOffice            { get; set; }
    public string? VatNumber1           { get; set; }
    public string? VatNumber2           { get; set; }
    public string? VatNumber3           { get; set; }
    public string? CompanyRegNo         { get; set; }
    public decimal CompanyTaxRate       { get; set; }
    public string? ExemptionNumber      { get; set; }
    public string? TaxDeductionNumber   { get; set; }
    public string? TaxOfficial          { get; set; }
    public bool    UseDeferredTax       { get; set; }
    public bool    ApplyExchangeRateOnDeferredTax { get; set; }
    public string? TaxRateDetermination { get; set; }
    public string? Holidays             { get; set; }
    public bool    ExtendedTaxReporting { get; set; }
    public string? EoriNumber           { get; set; }
    public bool    AllowExternalTaxCalculationOnAR { get; set; }

    // ══════════════════════════════════════════════════════════
    // BASIC INITIALISATION
    // ══════════════════════════════════════════════════════════
    public string? ChartOfAccountsTemplate { get; set; }
    public string? LocalCurrency           { get; set; }
    public string? SystemCurrency          { get; set; }
    public string? DefaultAccountCurrency  { get; set; }
    public bool    DisplayCreditBalanceNegative { get; set; } = true;
    public bool    UseSegmentationAccounts       { get; set; }
    public bool    AllowNegativeReversalPosting  { get; set; }
    public bool    PermitMoreThanOneDocumentType { get; set; }
    public bool    MultiLanguageSupport          { get; set; }
    public bool    UseContinuousStock            { get; set; } = true;
    public string? ItemGroupsValuationMethod     { get; set; }
    public bool    ManageItemCostPerWarehouse    { get; set; }
    public bool    UsePurchaseAccountsPosting    { get; set; }
    public bool    AllowStockReleaseWithoutItemCost { get; set; } = true;
    public string? ManageSerialBatchCostBy       { get; set; }
    public bool    EnableSeparateNetGrossPrice   { get; set; }
    public string? OrderingParty                 { get; set; }
    public string? DefaultBankCountry            { get; set; }
    public string? DefaultBank                   { get; set; }
    public string? DefaultAccountNo              { get; set; }
    public string? DefaultBranch                 { get; set; }
    public bool    InstallBankStatementProcessing { get; set; }
    public bool    EnableFixedAssets             { get; set; } = true;
    public string? CalculateDepreciationBy       { get; set; }
    public bool    EnableMultipleBranches        { get; set; }
    public bool    MaskCreditCardNumber          { get; set; } = true;
    public bool    EnableAdvancedGLDetermination { get; set; }
    public bool    AllowAnyAccountTypeForRevenue { get; set; }
    public bool    EnableProjectManagement       { get; set; } = true;
    public bool    EnablePersonalDataProtection  { get; set; }
    public string? TermsAndConditionsFile        { get; set; }
}


public class CompanyConfig : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(m => m.Logo).HasMaxLength(100);
        builder.Property(m => m.Name).HasMaxLength(100);
        builder.Property(m => m.ShortName).HasMaxLength(20);
        builder.Property(m => m.Desc).HasMaxLength(255);
        builder.Property(m => m.Address).HasMaxLength(200);

        builder.Property(m => m.CompanyTaxRate).HasColumnType("decimal(9,4)");

        // Address blocks + settings — keep them comfortably sized
        foreach (var p in new[]
        {
            "LocalCompanyName","LocalStreet","LocalStreetNo","LocalBlock","LocalBuildingFloorRoom",
            "LocalCity","LocalPostcode","LocalCounty","LocalState","LocalCountryRegion",
            "LocalInternetAddress","LocalPrintingHeader","LocalActiveManager","LocalAliasName",
            "LocalFax","LocalEmail","LocalGln",
            "ForeignCompanyName","ForeignStreet","ForeignStreetNo","ForeignBlock","ForeignBuildingFloorRoom",
            "ForeignCity","ForeignPostcode","ForeignCounty","ForeignState","ForeignCountryRegion",
            "ForeignInternetAddress","ForeignPrintingHeader","ForeignActiveManager","ForeignAliasName",
            "ForeignFax","ForeignEmail","ForeignGln",
            "TaxOffice","VatNumber1","VatNumber2","VatNumber3","CompanyRegNo","ExemptionNumber",
            "TaxDeductionNumber","TaxOfficial","TaxRateDetermination","Holidays","EoriNumber",
            "ChartOfAccountsTemplate","LocalCurrency","SystemCurrency","DefaultAccountCurrency",
            "ItemGroupsValuationMethod","ManageSerialBatchCostBy","OrderingParty","DefaultBankCountry",
            "DefaultBank","DefaultAccountNo","DefaultBranch","CalculateDepreciationBy",
            "TermsAndConditionsFile",
        })
            builder.Property<string>(p).HasMaxLength(200);
    }
}

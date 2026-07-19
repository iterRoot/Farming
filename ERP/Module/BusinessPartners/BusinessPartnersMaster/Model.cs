namespace FarmingApi.Modules.BusinessPartners;

// ══════════════════════════════════════════════════════════════════
// SUB-TABLE DTOs
// ══════════════════════════════════════════════════════════════════

public class BPAddressDto
{
    public int     Id                { get; set; }
    public int     AdresType         { get; set; }  // 0=Ship-To, 1=Bill-To
    public string? AdressName        { get; set; }
    public string? AddressName2      { get; set; }
    public string? AddressName3      { get; set; }
    public string? Street            { get; set; }
    public string? StreetNo          { get; set; }
    public string? Block             { get; set; }
    public string? BuildingFloorRoom { get; set; }
    public string? City              { get; set; }
    public string? ZipCode           { get; set; }
    public string? County            { get; set; }
    public string? State             { get; set; }
    public string? Country           { get; set; }
    public string? VatNumber         { get; set; }
    public string? TaxOffice         { get; set; }
    public string? GLN               { get; set; }
    public bool    IsDefault         { get; set; }
}

public class BPContactDto
{
    public int       Id                    { get; set; }
    public string?   Name                  { get; set; }
    public string?   FirstName             { get; set; }
    public string?   MiddleName            { get; set; }
    public string?   Surname               { get; set; }
    public string?   Title                 { get; set; }
    public string?   Position              { get; set; }
    public string?   Address               { get; set; }
    public string?   Tel1                  { get; set; }
    public string?   Tel2                  { get; set; }
    public string?   MobilePhone           { get; set; }
    public string?   Fax                   { get; set; }
    public string?   Email                 { get; set; }
    public string?   EmailGroup            { get; set; }
    public string?   Pager                 { get; set; }
    public string?   Remarks1              { get; set; }
    public string?   Remarks2              { get; set; }
    public string?   Password              { get; set; }
    public string?   CountryOfBirth        { get; set; }
    public DateTime? DateOfBirth           { get; set; }
    public int       Gender                { get; set; }  // 0=Not Specified, 1=Male, 2=Female
    public string?   Profession            { get; set; }
    public string?   CityOfBirth           { get; set; }
    public string?   ConnectedAddress      { get; set; }
    public bool      BlockMarketingContent { get; set; }
    public bool      Active                { get; set; } = true;
    public bool      EDocRecipient         { get; set; }
    public bool      IsDefault             { get; set; }
}

public class BPBankAccountDto
{
    public int       Id              { get; set; }
    public string?   BankCountry     { get; set; }
    public string?   BankName        { get; set; }
    public string?   BankCode        { get; set; }
    public string?   AccountNo       { get; set; }
    public string?   SwiftNum        { get; set; }
    public string?   AccountName     { get; set; }
    public string?   Branch          { get; set; }
    public string?   CtrlIntId       { get; set; }
    public string?   Iban            { get; set; }
    public string?   MandateId       { get; set; }
    public DateTime? DateOfSignature { get; set; }
    public string?   Currency        { get; set; }
    public bool      IsDefault       { get; set; }
}

public class BPPaymentMethodDto
{
    public int     Id          { get; set; }
    public string? Code        { get; set; }
    public string? Description { get; set; }
    public bool    Include     { get; set; }
    public bool    Active      { get; set; } = true;
}

// ══════════════════════════════════════════════════════════════════
// SHARED EDITABLE FIELDS — inherited by Create, Update, Response
// ══════════════════════════════════════════════════════════════════
public abstract class BPEditableFields
{
    // Core
    public string  CardName  { get; set; } = null!;
    public string? FrgnName  { get; set; }
    public string? AliasName { get; set; }

    // Type & Status
    public int       TypeStatus    { get; set; }
    public int       ActiveStatus  { get; set; }
    public DateTime? ActiveFrom    { get; set; }
    public DateTime? ActiveTo      { get; set; }
    public string?   ActiveRemarks { get; set; }

    // Header
    public string? GroupCode        { get; set; }
    public string? Currency         { get; set; }
    public string? VatNumber        { get; set; }
    public string? UnifiedVatNumber { get; set; }
    public string? IdNo2            { get; set; }

    // General tab — left
    public string? Tel1               { get; set; }
    public string? Tel2               { get; set; }
    public string? MobilePhone        { get; set; }
    public string? Fax                { get; set; }
    public string? Email              { get; set; }
    public string? Website            { get; set; }
    public string? ShippingType       { get; set; }
    public string? Password           { get; set; }
    public string? FactoringIndicator { get; set; }
    public string? BPProject          { get; set; }
    public string? Industry           { get; set; }
    public int     TypeOfBusiness     { get; set; }

    // General tab — right
    public string? DefaultContactPerson  { get; set; }
    public string? Remarks               { get; set; }
    public string? SalesEmployee         { get; set; }
    public string? BPChannelCode         { get; set; }
    public string? Technician            { get; set; }
    public string? Territory             { get; set; }
    public string? GLN                   { get; set; }
    public string? BranchAssignment      { get; set; }
    public bool    BlockMarketingContent { get; set; }

    // Payment Terms tab
    public string?   PayTerms                { get; set; }
    public decimal   InterestOnArrears       { get; set; }
    public string?   PriceList               { get; set; }
    public decimal   TotalDiscount           { get; set; }
    public decimal   CreditLimit             { get; set; }
    public decimal   CommitmentLimit         { get; set; }
    public string?   DunningTerm             { get; set; }
    public string?   EffectiveDiscountGroups { get; set; }
    public string?   EffectivePrice          { get; set; }
    public string?   CreditCardType          { get; set; }
    public string?   CreditCardNo            { get; set; }
    public DateTime? CreditCardExpiry        { get; set; }
    public string?   CreditCardIdNumber      { get; set; }
    public decimal?  AverageDelay            { get; set; }
    public string?   Priority                { get; set; }
    public string?   DefaultIban             { get; set; }
    public string?   Holidays                { get; set; }
    public string?   PaymentDates            { get; set; }
    public bool      AllowPartialDeliveryOfSO   { get; set; }
    public bool      AllowPartialDeliveryPerRow { get; set; }
    public bool      DoNotApplyDiscountGroups   { get; set; }
    public bool      EndorsableCheques          { get; set; }
    public bool      AcceptsEndorsedCheques     { get; set; }

    // Payment Run tab
    public string? HouseBankCountry          { get; set; }
    public string? HouseBankCode             { get; set; }
    public string? HouseBankAccount          { get; set; }
    public string? HouseBankBranch           { get; set; }
    public string? HouseBankIban             { get; set; }
    public string? HouseBankSwift            { get; set; }
    public string? HouseBankControlNo        { get; set; }
    public string? ReferenceDetails          { get; set; }
    public bool    PaymentBlock              { get; set; }
    public string? PaymentBlockCode          { get; set; }
    public bool    SinglePayment             { get; set; }
    public bool    CollectionAuthorisation   { get; set; }
    public string? BankChargesAllocationCode { get; set; }
    public bool    AutoCalcBankCharges       { get; set; }

    // Accounting tab
    public string?   ConsolidatingBP           { get; set; }
    public int       ConsolidationType          { get; set; }
    public string?   ARControlAccount           { get; set; }
    public string?   DownPaymentClearingAccount { get; set; }
    public string?   DownPaymentInterimAccount  { get; set; }
    public bool      BlockDunningLetters        { get; set; }
    public string?   DunningLevel               { get; set; }
    public DateTime? DunningDate                { get; set; }
    public string?   ConnectedSupplier          { get; set; }
    public string?   PlanningGroup              { get; set; }
    public bool      UseShippedGoodsAccount     { get; set; }
    public bool      Affiliate                  { get; set; }

    // Tax
    public string? TaxId    { get; set; }
    public string? VatGroup { get; set; }

    // Sub-tables
    public List<BPAddressDto>       Addresses      { get; set; } = new();
    public List<BPContactDto>       Contacts       { get; set; } = new();
    public List<BPBankAccountDto>   BankAccts      { get; set; } = new();
    public List<BPPaymentMethodDto> PaymentMethods { get; set; } = new();
}

// ══════════════════════════════════════════════════════════════════
// CREATE / UPDATE REQUESTS
// ══════════════════════════════════════════════════════════════════
public class BPCreateRequest : BPEditableFields
{
    public string  Code       { get; set; } = null!;
    public string? CodeSeries { get; set; }
}

public class BPUpdateRequest : BPEditableFields { }

// ══════════════════════════════════════════════════════════════════
// RESPONSE
// ══════════════════════════════════════════════════════════════════
public class BPResponse : BPEditableFields
{
    public int    Id         { get; set; }
    public string Code       { get; set; } = null!;
    public string? CodeSeries { get; set; }

    public string TypeStatusLabel     { get; set; } = null!;
    public string ActiveStatusLabel   { get; set; } = null!;
    public string TypeOfBusinessLabel { get; set; } = null!;

    // Computed balances (read-only)
    public decimal Balance              { get; set; }
    public decimal DeliveriesBalance    { get; set; }
    public decimal OrdersBalance        { get; set; }
    public decimal OpportunitiesBalance { get; set; }
    public decimal ChequesBalance       { get; set; }

    public int       VersionNum { get; set; }
    public DateTime  CreatedAt  { get; set; }
    public DateTime? UpdatedAt  { get; set; }
}

// ══════════════════════════════════════════════════════════════════
// LIST RESPONSE (lightweight — no sub-tables)
// ══════════════════════════════════════════════════════════════════
public class BPListResponse
{
    public int     Id                { get; set; }
    public string  Code              { get; set; } = null!;
    public string  CardName          { get; set; } = null!;
    public int     TypeStatus        { get; set; }
    public string  TypeStatusLabel   { get; set; } = null!;
    public int     ActiveStatus      { get; set; }
    public string  ActiveStatusLabel { get; set; } = null!;
    public string? GroupCode         { get; set; }
    public string? Currency          { get; set; }
    public string? Tel1              { get; set; }
    public string? MobilePhone       { get; set; }
    public string? Email             { get; set; }
    public decimal Balance           { get; set; }
    public DateTime CreatedAt        { get; set; }
}
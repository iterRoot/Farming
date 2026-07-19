using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

// ══════════════════════════════════════════════════════════════════
// TABLE MAP
//   KOCR  → Business Partners Master  (SAP: OCRD)
//   KCRD  → BP Address                (SAP: CRD1)
//   KCPR  → BP Contact Person         (SAP: OCPR)
//   KCRB  → BP Bank Account           (SAP: OCRB)
//   KPMT  → BP Payment Method         (SAP: no direct equiv — OPYM)
// ══════════════════════════════════════════════════════════════════

// ── MASTER ────────────────────────────────────────────────────────
public class BusinessPartnersMaster : AuditableEntity
{
    // Core Identity
    public string  Code       { get; set; } = null!;
    public string? CodeSeries { get; set; }
    public string  CardName   { get; set; } = null!;
    public string? FrgnName   { get; set; }
    public string? AliasName  { get; set; }

    // Type & Status
    public int       TypeStatus    { get; set; }   // BPType
    public int       ActiveStatus  { get; set; }   // BPActiveStatus
    public DateTime? ActiveFrom    { get; set; }
    public DateTime? ActiveTo      { get; set; }
    public string?   ActiveRemarks { get; set; }

    // Header dropdowns
    public string? GroupCode        { get; set; }
    public string? Currency         { get; set; }
    public string? VatNumber        { get; set; }
    public string? UnifiedVatNumber { get; set; }
    public string? IdNo2            { get; set; }

    // General tab — left column
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
    public int     TypeOfBusiness     { get; set; } // BPTypeOfBusiness

    // General tab — right column
    public string? DefaultContactPerson  { get; set; }
    public string? Remarks               { get; set; }
    public string? SalesEmployee         { get; set; }
    public string? BPChannelCode         { get; set; }
    public string? Technician            { get; set; }
    public string? Territory             { get; set; }
    public string? GLN                   { get; set; }
    public string? BranchAssignment      { get; set; }
    public bool    BlockMarketingContent { get; set; }

    // Payment Terms tab — left
    public string?  PayTerms                { get; set; }
    public decimal  InterestOnArrears       { get; set; }
    public string?  PriceList               { get; set; }
    public decimal  TotalDiscount           { get; set; }
    public decimal  CreditLimit             { get; set; }
    public decimal  CommitmentLimit         { get; set; }
    public string?  DunningTerm             { get; set; }
    public string?  EffectiveDiscountGroups { get; set; }
    public string?  EffectivePrice          { get; set; }

    // Payment Terms tab — right
    public string?   CreditCardType     { get; set; }
    public string?   CreditCardNo       { get; set; }
    public DateTime? CreditCardExpiry   { get; set; }
    public string?   CreditCardIdNumber { get; set; }
    public decimal?  AverageDelay       { get; set; }
    public string?   Priority           { get; set; }
    public string?   DefaultIban        { get; set; }
    public string?   Holidays           { get; set; }
    public string?   PaymentDates       { get; set; }

    // Payment Terms tab — checkboxes
    public bool AllowPartialDeliveryOfSO   { get; set; }
    public bool AllowPartialDeliveryPerRow { get; set; }
    public bool DoNotApplyDiscountGroups   { get; set; }
    public bool EndorsableCheques          { get; set; }
    public bool AcceptsEndorsedCheques     { get; set; }

    // Payment Run tab — House Bank
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
    public string?   ConsolidatingBP             { get; set; }
    public int       ConsolidationType            { get; set; } // BPConsolidationType
    public string?   ARControlAccount             { get; set; }
    public string?   DownPaymentClearingAccount   { get; set; }
    public string?   DownPaymentInterimAccount    { get; set; }
    public bool      BlockDunningLetters          { get; set; }
    public string?   DunningLevel                 { get; set; }
    public DateTime? DunningDate                  { get; set; }
    public string?   ConnectedSupplier            { get; set; }
    public string?   PlanningGroup                { get; set; }
    public bool      UseShippedGoodsAccount       { get; set; }
    public bool      Affiliate                    { get; set; }

    // Computed balances (system-managed, never set from requests)
    public decimal Balance              { get; set; }
    public decimal DeliveriesBalance    { get; set; }
    public decimal OrdersBalance        { get; set; }
    public decimal OpportunitiesBalance { get; set; }
    public decimal ChequesBalance       { get; set; }

    // Legacy tax fields
    public string? TaxId    { get; set; }
    public string? VatGroup { get; set; }

    // System
    public int? UserSign  { get; set; }
    public int  VersionNum { get; set; } = 1;

    // Navigation
    public ICollection<BPAddress>       Addresses      { get; set; } = new List<BPAddress>();
    public ICollection<BPContact>       Contacts       { get; set; } = new List<BPContact>();
    public ICollection<BPBankAccount>   BankAccts      { get; set; } = new List<BPBankAccount>();
    public ICollection<BPPaymentMethod> PaymentMethods { get; set; } = new List<BPPaymentMethod>();
}

// ── BP ADDRESS — KCRD ─────────────────────────────────────────────
public class BPAddress : AuditableEntity
{
    public int BPId { get; set; }

    public int     AdresType         { get; set; }  // 0=Ship-To, 1=Bill-To
    public string? AdressName        { get; set; }  // Address ID label
    public string? AddressName2      { get; set; }
    public string? AddressName3      { get; set; }
    public string? Street            { get; set; }  // Street / PO Box
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

    public BusinessPartnersMaster? BP { get; set; }
}

// ── BP CONTACT — KCPR ─────────────────────────────────────────────
public class BPContact : AuditableEntity
{
    public int BPId { get; set; }

    public string?   Name                  { get; set; }  // Contact ID
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

    public BusinessPartnersMaster? BP { get; set; }
}

// ── BP BANK ACCOUNT — KCRB ───────────────────────────────────────
public class BPBankAccount : AuditableEntity
{
    public int BPId { get; set; }

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

    public BusinessPartnersMaster? BP { get; set; }
}

// ── BP PAYMENT METHOD — KPMT ─────────────────────────────────────
public class BPPaymentMethod : AuditableEntity
{
    public int BPId { get; set; }

    public string? Code        { get; set; }
    public string? Description { get; set; }
    public bool    Include     { get; set; }
    public bool    Active      { get; set; } = true;

    public BusinessPartnersMaster? BP { get; set; }
}

// ══════════════════════════════════════════════════════════════════
// CONFIGURATIONS
// ══════════════════════════════════════════════════════════════════

public class BusinessPartnersMasterConfig : IEntityTypeConfiguration<BusinessPartnersMaster>
{
    public void Configure(EntityTypeBuilder<BusinessPartnersMaster> builder)
    {
        builder.ToTable("KOCR");
        builder.HasKey(x => x.Id);

        // Core
        builder.Property(m => m.Code).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.Code).IsUnique();
        builder.Property(m => m.CodeSeries).HasMaxLength(30);
        builder.Property(m => m.CardName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.FrgnName).HasMaxLength(200);
        builder.Property(m => m.AliasName).HasMaxLength(200);

        // Type & Status
        builder.Property(m => m.TypeStatus).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.ActiveStatus).IsRequired().HasDefaultValue(0);
        builder.Property(m => m.ActiveRemarks).HasMaxLength(200);

        // Header
        builder.Property(m => m.GroupCode).HasMaxLength(20);
        builder.Property(m => m.Currency).HasMaxLength(3);
        builder.Property(m => m.VatNumber).HasMaxLength(50);
        builder.Property(m => m.UnifiedVatNumber).HasMaxLength(50);
        builder.Property(m => m.IdNo2).HasMaxLength(50);

        // General — left
        builder.Property(m => m.Tel1).HasMaxLength(30);
        builder.Property(m => m.Tel2).HasMaxLength(30);
        builder.Property(m => m.MobilePhone).HasMaxLength(30);
        builder.Property(m => m.Fax).HasMaxLength(30);
        builder.Property(m => m.Email).HasMaxLength(100);
        builder.Property(m => m.Website).HasMaxLength(100);
        builder.Property(m => m.ShippingType).HasMaxLength(50);
        builder.Property(m => m.Password).HasMaxLength(100);
        builder.Property(m => m.FactoringIndicator).HasMaxLength(50);
        builder.Property(m => m.BPProject).HasMaxLength(50);
        builder.Property(m => m.Industry).HasMaxLength(50);
        builder.Property(m => m.TypeOfBusiness).HasDefaultValue(0);

        // General — right
        builder.Property(m => m.DefaultContactPerson).HasMaxLength(100);
        builder.Property(m => m.Remarks).HasMaxLength(1000);
        builder.Property(m => m.SalesEmployee).HasMaxLength(100);
        builder.Property(m => m.BPChannelCode).HasMaxLength(50);
        builder.Property(m => m.Technician).HasMaxLength(100);
        builder.Property(m => m.Territory).HasMaxLength(100);
        builder.Property(m => m.GLN).HasMaxLength(50);
        builder.Property(m => m.BranchAssignment).HasMaxLength(100);
        builder.Property(m => m.BlockMarketingContent).HasDefaultValue(false);

        // Payment Terms
        builder.Property(m => m.PayTerms).HasMaxLength(50);
        builder.Property(m => m.InterestOnArrears).HasPrecision(9, 4).HasDefaultValue(0);
        builder.Property(m => m.PriceList).HasMaxLength(50);
        builder.Property(m => m.TotalDiscount).HasPrecision(9, 4).HasDefaultValue(0);
        builder.Property(m => m.CreditLimit).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.CommitmentLimit).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.DunningTerm).HasMaxLength(50);
        builder.Property(m => m.EffectiveDiscountGroups).HasMaxLength(50);
        builder.Property(m => m.EffectivePrice).HasMaxLength(50);
        builder.Property(m => m.CreditCardType).HasMaxLength(50);
        builder.Property(m => m.CreditCardNo).HasMaxLength(30);
        builder.Property(m => m.CreditCardIdNumber).HasMaxLength(50);
        builder.Property(m => m.AverageDelay).HasPrecision(9, 2);
        builder.Property(m => m.Priority).HasMaxLength(50);
        builder.Property(m => m.DefaultIban).HasMaxLength(50);
        builder.Property(m => m.Holidays).HasMaxLength(50);
        builder.Property(m => m.PaymentDates).HasMaxLength(500);
        builder.Property(m => m.AllowPartialDeliveryOfSO).HasDefaultValue(false);
        builder.Property(m => m.AllowPartialDeliveryPerRow).HasDefaultValue(false);
        builder.Property(m => m.DoNotApplyDiscountGroups).HasDefaultValue(false);
        builder.Property(m => m.EndorsableCheques).HasDefaultValue(false);
        builder.Property(m => m.AcceptsEndorsedCheques).HasDefaultValue(false);

        // Payment Run
        builder.Property(m => m.HouseBankCountry).HasMaxLength(50);
        builder.Property(m => m.HouseBankCode).HasMaxLength(30);
        builder.Property(m => m.HouseBankAccount).HasMaxLength(50);
        builder.Property(m => m.HouseBankBranch).HasMaxLength(100);
        builder.Property(m => m.HouseBankIban).HasMaxLength(50);
        builder.Property(m => m.HouseBankSwift).HasMaxLength(20);
        builder.Property(m => m.HouseBankControlNo).HasMaxLength(50);
        builder.Property(m => m.ReferenceDetails).HasMaxLength(100);
        builder.Property(m => m.PaymentBlock).HasDefaultValue(false);
        builder.Property(m => m.PaymentBlockCode).HasMaxLength(20);
        builder.Property(m => m.SinglePayment).HasDefaultValue(false);
        builder.Property(m => m.CollectionAuthorisation).HasDefaultValue(false);
        builder.Property(m => m.BankChargesAllocationCode).HasMaxLength(20);
        builder.Property(m => m.AutoCalcBankCharges).HasDefaultValue(false);

        // Accounting
        builder.Property(m => m.ConsolidatingBP).HasMaxLength(50);
        builder.Property(m => m.ConsolidationType).HasDefaultValue(0);
        builder.Property(m => m.ARControlAccount).HasMaxLength(20);
        builder.Property(m => m.DownPaymentClearingAccount).HasMaxLength(20);
        builder.Property(m => m.DownPaymentInterimAccount).HasMaxLength(20);
        builder.Property(m => m.BlockDunningLetters).HasDefaultValue(false);
        builder.Property(m => m.DunningLevel).HasMaxLength(20);
        builder.Property(m => m.ConnectedSupplier).HasMaxLength(50);
        builder.Property(m => m.PlanningGroup).HasMaxLength(20);
        builder.Property(m => m.UseShippedGoodsAccount).HasDefaultValue(false);
        builder.Property(m => m.Affiliate).HasDefaultValue(false);

        // Computed balances
        builder.Property(m => m.Balance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.DeliveriesBalance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.OrdersBalance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.OpportunitiesBalance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.ChequesBalance).HasPrecision(18, 2).HasDefaultValue(0);

        // Tax
        builder.Property(m => m.TaxId).HasMaxLength(50);
        builder.Property(m => m.VatGroup).HasMaxLength(20);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        // Indexes
        builder.HasIndex(m => m.TypeStatus);
        builder.HasIndex(m => m.ActiveStatus);
        builder.HasIndex(m => m.CardName);

        // Relations
        builder.HasMany(m => m.Addresses).WithOne(a => a.BP)
            .HasForeignKey(a => a.BPId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Contacts).WithOne(c => c.BP)
            .HasForeignKey(c => c.BPId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.BankAccts).WithOne(b => b.BP)
            .HasForeignKey(b => b.BPId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.PaymentMethods).WithOne(p => p.BP)
            .HasForeignKey(p => p.BPId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BPAddressConfig : IEntityTypeConfiguration<BPAddress>
{
    public void Configure(EntityTypeBuilder<BPAddress> builder)
    {
        builder.ToTable("KCRD");
        builder.HasKey(x => x.Id);
        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.AdresType).HasDefaultValue(0);
        builder.Property(m => m.AdressName).HasMaxLength(100);
        builder.Property(m => m.AddressName2).HasMaxLength(100);
        builder.Property(m => m.AddressName3).HasMaxLength(100);
        builder.Property(m => m.Street).HasMaxLength(300);
        builder.Property(m => m.StreetNo).HasMaxLength(50);
        builder.Property(m => m.Block).HasMaxLength(100);
        builder.Property(m => m.BuildingFloorRoom).HasMaxLength(100);
        builder.Property(m => m.City).HasMaxLength(100);
        builder.Property(m => m.ZipCode).HasMaxLength(20);
        builder.Property(m => m.County).HasMaxLength(100);
        builder.Property(m => m.State).HasMaxLength(100);
        builder.Property(m => m.Country).HasMaxLength(50);
        builder.Property(m => m.VatNumber).HasMaxLength(50);
        builder.Property(m => m.TaxOffice).HasMaxLength(100);
        builder.Property(m => m.GLN).HasMaxLength(50);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);
        builder.HasIndex(m => m.BPId);
    }
}

public class BPContactConfig : IEntityTypeConfiguration<BPContact>
{
    public void Configure(EntityTypeBuilder<BPContact> builder)
    {
        builder.ToTable("KCPR");
        builder.HasKey(x => x.Id);
        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(100);
        builder.Property(m => m.FirstName).HasMaxLength(100);
        builder.Property(m => m.MiddleName).HasMaxLength(100);
        builder.Property(m => m.Surname).HasMaxLength(100);
        builder.Property(m => m.Title).HasMaxLength(30);
        builder.Property(m => m.Position).HasMaxLength(100);
        builder.Property(m => m.Address).HasMaxLength(300);
        builder.Property(m => m.Tel1).HasMaxLength(30);
        builder.Property(m => m.Tel2).HasMaxLength(30);
        builder.Property(m => m.MobilePhone).HasMaxLength(30);
        builder.Property(m => m.Fax).HasMaxLength(30);
        builder.Property(m => m.Email).HasMaxLength(100);
        builder.Property(m => m.EmailGroup).HasMaxLength(100);
        builder.Property(m => m.Pager).HasMaxLength(30);
        builder.Property(m => m.Remarks1).HasMaxLength(500);
        builder.Property(m => m.Remarks2).HasMaxLength(500);
        builder.Property(m => m.Password).HasMaxLength(100);
        builder.Property(m => m.CountryOfBirth).HasMaxLength(50);
        builder.Property(m => m.Gender).HasDefaultValue(0);
        builder.Property(m => m.Profession).HasMaxLength(100);
        builder.Property(m => m.CityOfBirth).HasMaxLength(100);
        builder.Property(m => m.ConnectedAddress).HasMaxLength(100);
        builder.Property(m => m.BlockMarketingContent).HasDefaultValue(false);
        builder.Property(m => m.Active).HasDefaultValue(true);
        builder.Property(m => m.EDocRecipient).HasDefaultValue(false);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);
        builder.HasIndex(m => m.BPId);
    }
}

public class BPBankAccountConfig : IEntityTypeConfiguration<BPBankAccount>
{
    public void Configure(EntityTypeBuilder<BPBankAccount> builder)
    {
        builder.ToTable("KCRB");
        builder.HasKey(x => x.Id);
        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.BankCountry).HasMaxLength(50);
        builder.Property(m => m.BankName).HasMaxLength(100);
        builder.Property(m => m.BankCode).HasMaxLength(20);
        builder.Property(m => m.AccountNo).HasMaxLength(50);
        builder.Property(m => m.SwiftNum).HasMaxLength(20);
        builder.Property(m => m.AccountName).HasMaxLength(100);
        builder.Property(m => m.Branch).HasMaxLength(100);
        builder.Property(m => m.CtrlIntId).HasMaxLength(50);
        builder.Property(m => m.Iban).HasMaxLength(50);
        builder.Property(m => m.MandateId).HasMaxLength(50);
        builder.Property(m => m.Currency).HasMaxLength(3);
        builder.Property(m => m.IsDefault).HasDefaultValue(false);
        builder.HasIndex(m => m.BPId);
    }
}

public class BPPaymentMethodConfig : IEntityTypeConfiguration<BPPaymentMethod>
{
    public void Configure(EntityTypeBuilder<BPPaymentMethod> builder)
    {
        builder.ToTable("KPMT");
        builder.HasKey(x => x.Id);
        builder.Property(m => m.BPId).IsRequired();
        builder.Property(m => m.Code).HasMaxLength(30);
        builder.Property(m => m.Description).HasMaxLength(100);
        builder.Property(m => m.Include).HasDefaultValue(false);
        builder.Property(m => m.Active).HasDefaultValue(true);
        builder.HasIndex(m => m.BPId);
    }
}
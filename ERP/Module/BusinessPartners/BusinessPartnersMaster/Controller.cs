using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

public class BusinessPartnersMasterController : MyController
{
    private readonly IBusinessPartnersMasterRepository _repository;

    public BusinessPartnersMasterController(IBusinessPartnersMasterRepository repository)
    {
        _repository = repository;
    }

    // ── GET ALL ───────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets([FromQuery] int? typeStatus = null)
    {
        var query = _repository.GetAll();
        if (typeStatus.HasValue)
            query = query.Where(b => b.TypeStatus == typeStatus.Value);

        var result = query
            .OrderBy(b => b.Code)
            .Select(b => new BPListResponse
            {
                Id                = b.Id,
                Code              = b.Code,
                CardName          = b.CardName,
                TypeStatus        = b.TypeStatus,
                TypeStatusLabel   = b.TypeStatus == 0 ? "Customer" : b.TypeStatus == 1 ? "Vendor" : "Lead",
                ActiveStatus      = b.ActiveStatus,
                ActiveStatusLabel = b.ActiveStatus == 0 ? "Active" : b.ActiveStatus == 1 ? "Inactive" : "Advanced",
                GroupCode         = b.GroupCode,
                Currency          = b.Currency,
                Tel1              = b.Tel1,
                MobilePhone       = b.MobilePhone,
                Email             = b.Email,
                Balance           = b.Balance,
                CreatedAt         = b.CreatedAt,
            })
            .ToList();

        return Ok(result);
    }

    [AllowAnonymous] [HttpGet("Customers")]
    public IActionResult GetCustomers() => Gets(typeStatus: 0);

    [AllowAnonymous] [HttpGet("Vendors")]
    public IActionResult GetVendors() => Gets(typeStatus: 1);

    // ── Shared eager-load helper ──────────────────────────────────
    private IQueryable<BusinessPartnersMaster> WithIncludes() =>
        _repository.GetAll()
            .Include(b => b.Addresses)
            .Include(b => b.Contacts)
            .Include(b => b.BankAccts)
            .Include(b => b.PaymentMethods);

    // ── GET BY ID / CODE ──────────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var bp = WithIncludes().FirstOrDefault(e => e.Id == id);
        if (bp == null) return NotFound($"Business partner {id} not found");
        return Ok(MapToResponse(bp));
    }

    [HttpGet("Code/{code}")]
    public IActionResult GetByCode(string code)
    {
        var bp = WithIncludes().FirstOrDefault(e => e.Code == code);
        if (bp == null) return NotFound($"Business partner '{code}' not found");
        return Ok(MapToResponse(bp));
    }

    // ── CREATE ────────────────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] BPCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Code))     return BadRequest("Code is required");
        if (string.IsNullOrWhiteSpace(request.CardName)) return BadRequest("Card Name is required");

        var err = Validate(request);
        if (err != null) return BadRequest(err);

        var code = request.Code.Trim().ToUpper();
        if (_repository.GetAll().Any(b => b.Code == code))
            return BadRequest($"Business partner code '{code}' already exists");

        var entity = new BusinessPartnersMaster
        {
            Code       = code,
            CodeSeries = request.CodeSeries?.Trim(),
            Balance    = 0,
            VersionNum = 1,
            CreatedAt  = DateTime.UtcNow,
        };

        ApplyFields(entity, request);
        ApplySubTables(entity, request);

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Business partner created", id = entity.Id, code = entity.Code });
    }

    // ── UPDATE ────────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BPUpdateRequest request)
    {
        // WithIncludes() required so .Clear() on collections actually tracks the deletes
        var bp = WithIncludes().FirstOrDefault(e => e.Id == id);
        if (bp == null) return NotFound($"Business partner {id} not found");
        if (string.IsNullOrWhiteSpace(request.CardName)) return BadRequest("Card Name is required");

        var err = Validate(request);
        if (err != null) return BadRequest(err);

        ApplyFields(bp, request);

        bp.Addresses.Clear();
        bp.Contacts.Clear();
        bp.BankAccts.Clear();
        bp.PaymentMethods.Clear();
        ApplySubTables(bp, request);

        bp.UpdatedAt   = DateTime.UtcNow;
        bp.VersionNum += 1;

        _repository.Update(bp);
        _repository.Commit();
        return NoContent();
    }

    // ── DELETE ────────────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var bp = _repository.GetSingle(b => b.Id == id);
        if (bp == null) return NotFound($"Business partner {id} not found");
        if (bp.Balance != 0) return BadRequest("Cannot delete a business partner with outstanding balance");

        _repository.Remove(bp);
        _repository.Commit();
        return NoContent();
    }

    // ── VALIDATION ────────────────────────────────────────────────
    private static string? Validate(BPEditableFields r)
    {
        if (r.ActiveStatus == (int)BPActiveStatus.Advanced && r.ActiveFrom == null && r.ActiveTo == null)
            return "Advanced status requires an Active From / To date range";
        if (r.TotalDiscount is < 0 or > 100)
            return "Total Discount % must be between 0 and 100";
        if (r.InterestOnArrears < 0)
            return "Interest on Arrears % cannot be negative";
        if (r.CreditLimit < 0 || r.CommitmentLimit < 0)
            return "Credit / Commitment limits cannot be negative";
        if (r.Addresses.Where(a => a.AdresType == 0).Count(a => a.IsDefault) > 1)
            return "Only one Ship-To address can be marked as default";
        if (r.Addresses.Where(a => a.AdresType == 1).Count(a => a.IsDefault) > 1)
            return "Only one Bill-To address can be marked as default";
        if (r.Contacts.Count(c => c.IsDefault) > 1)
            return "Only one contact person can be marked as default";
        if (r.BankAccts.Count(b => b.IsDefault) > 1)
            return "Only one bank account can be marked as default";
        if (r.Contacts.Where(c => !string.IsNullOrWhiteSpace(c.Name))
                      .GroupBy(c => c.Name!.Trim()).Any(g => g.Count() > 1))
            return "Contact IDs must be unique per business partner";
        return null;
    }

    // ── APPLY FIELDS ──────────────────────────────────────────────
    private static void ApplyFields(BusinessPartnersMaster bp, BPEditableFields r)
    {
        bp.CardName  = r.CardName.Trim();
        bp.FrgnName  = r.FrgnName?.Trim();
        bp.AliasName = r.AliasName?.Trim();

        bp.TypeStatus    = r.TypeStatus;
        bp.ActiveStatus  = r.ActiveStatus;
        bp.ActiveFrom    = r.ActiveFrom;
        bp.ActiveTo      = r.ActiveTo;
        bp.ActiveRemarks = r.ActiveRemarks?.Trim();

        bp.GroupCode        = r.GroupCode?.Trim();
        bp.Currency         = r.Currency?.Trim();
        bp.VatNumber        = r.VatNumber?.Trim();
        bp.UnifiedVatNumber = r.UnifiedVatNumber?.Trim();
        bp.IdNo2            = r.IdNo2?.Trim();

        bp.Tel1               = r.Tel1?.Trim();
        bp.Tel2               = r.Tel2?.Trim();
        bp.MobilePhone        = r.MobilePhone?.Trim();
        bp.Fax                = r.Fax?.Trim();
        bp.Email              = r.Email?.Trim();
        bp.Website            = r.Website?.Trim();
        bp.ShippingType       = r.ShippingType?.Trim();
        bp.Password           = r.Password?.Trim();
        bp.FactoringIndicator = r.FactoringIndicator?.Trim();
        bp.BPProject          = r.BPProject?.Trim();
        bp.Industry           = r.Industry?.Trim();
        bp.TypeOfBusiness     = r.TypeOfBusiness;

        bp.DefaultContactPerson  = r.DefaultContactPerson?.Trim();
        bp.Remarks               = r.Remarks?.Trim();
        bp.SalesEmployee         = r.SalesEmployee?.Trim();
        bp.BPChannelCode         = r.BPChannelCode?.Trim();
        bp.Technician            = r.Technician?.Trim();
        bp.Territory             = r.Territory?.Trim();
        bp.GLN                   = r.GLN?.Trim();
        bp.BranchAssignment      = r.BranchAssignment?.Trim();
        bp.BlockMarketingContent = r.BlockMarketingContent;

        bp.PayTerms                = r.PayTerms?.Trim();
        bp.InterestOnArrears       = r.InterestOnArrears;
        bp.PriceList               = r.PriceList?.Trim();
        bp.TotalDiscount           = r.TotalDiscount;
        bp.CreditLimit             = r.CreditLimit;
        bp.CommitmentLimit         = r.CommitmentLimit;
        bp.DunningTerm             = r.DunningTerm?.Trim();
        bp.EffectiveDiscountGroups = r.EffectiveDiscountGroups?.Trim();
        bp.EffectivePrice          = r.EffectivePrice?.Trim();
        bp.CreditCardType          = r.CreditCardType?.Trim();
        bp.CreditCardNo            = r.CreditCardNo?.Trim();
        bp.CreditCardExpiry        = r.CreditCardExpiry;
        bp.CreditCardIdNumber      = r.CreditCardIdNumber?.Trim();
        bp.AverageDelay            = r.AverageDelay;
        bp.Priority                = r.Priority?.Trim();
        bp.DefaultIban             = r.DefaultIban?.Trim();
        bp.Holidays                = r.Holidays?.Trim();
        bp.PaymentDates            = r.PaymentDates?.Trim();
        bp.AllowPartialDeliveryOfSO   = r.AllowPartialDeliveryOfSO;
        bp.AllowPartialDeliveryPerRow = r.AllowPartialDeliveryPerRow;
        bp.DoNotApplyDiscountGroups   = r.DoNotApplyDiscountGroups;
        bp.EndorsableCheques          = r.EndorsableCheques;
        bp.AcceptsEndorsedCheques     = r.AcceptsEndorsedCheques;

        bp.HouseBankCountry          = r.HouseBankCountry?.Trim();
        bp.HouseBankCode             = r.HouseBankCode?.Trim();
        bp.HouseBankAccount          = r.HouseBankAccount?.Trim();
        bp.HouseBankBranch           = r.HouseBankBranch?.Trim();
        bp.HouseBankIban             = r.HouseBankIban?.Trim();
        bp.HouseBankSwift            = r.HouseBankSwift?.Trim();
        bp.HouseBankControlNo        = r.HouseBankControlNo?.Trim();
        bp.ReferenceDetails          = r.ReferenceDetails?.Trim();
        bp.PaymentBlock              = r.PaymentBlock;
        bp.PaymentBlockCode          = r.PaymentBlockCode?.Trim();
        bp.SinglePayment             = r.SinglePayment;
        bp.CollectionAuthorisation   = r.CollectionAuthorisation;
        bp.BankChargesAllocationCode = r.BankChargesAllocationCode?.Trim();
        bp.AutoCalcBankCharges       = r.AutoCalcBankCharges;

        bp.ConsolidatingBP           = r.ConsolidatingBP?.Trim();
        bp.ConsolidationType          = r.ConsolidationType;
        bp.ARControlAccount           = r.ARControlAccount?.Trim();
        bp.DownPaymentClearingAccount = r.DownPaymentClearingAccount?.Trim();
        bp.DownPaymentInterimAccount  = r.DownPaymentInterimAccount?.Trim();
        bp.BlockDunningLetters        = r.BlockDunningLetters;
        bp.DunningLevel               = r.DunningLevel?.Trim();
        bp.DunningDate                = r.DunningDate;
        bp.ConnectedSupplier          = r.ConnectedSupplier?.Trim();
        bp.PlanningGroup              = r.PlanningGroup?.Trim();
        bp.UseShippedGoodsAccount     = r.UseShippedGoodsAccount;
        bp.Affiliate                  = r.Affiliate;

        bp.TaxId    = r.TaxId?.Trim();
        bp.VatGroup = r.VatGroup?.Trim();
    }

    // ── APPLY SUB-TABLES ─────────────────────────────────────────
    private static void ApplySubTables(BusinessPartnersMaster bp, BPEditableFields r)
    {
        foreach (var a in r.Addresses)
            bp.Addresses.Add(new BPAddress
            {
                AdresType         = a.AdresType,
                AdressName        = a.AdressName?.Trim(),
                AddressName2      = a.AddressName2?.Trim(),
                AddressName3      = a.AddressName3?.Trim(),
                Street            = a.Street?.Trim(),
                StreetNo          = a.StreetNo?.Trim(),
                Block             = a.Block?.Trim(),
                BuildingFloorRoom = a.BuildingFloorRoom?.Trim(),
                City              = a.City?.Trim(),
                ZipCode           = a.ZipCode?.Trim(),
                County            = a.County?.Trim(),
                State             = a.State?.Trim(),
                Country           = a.Country?.Trim(),
                VatNumber         = a.VatNumber?.Trim(),
                TaxOffice         = a.TaxOffice?.Trim(),
                GLN               = a.GLN?.Trim(),
                IsDefault         = a.IsDefault,
                CreatedAt         = DateTime.UtcNow,
            });

        foreach (var c in r.Contacts)
            bp.Contacts.Add(new BPContact
            {
                Name                  = c.Name?.Trim(),
                FirstName             = c.FirstName?.Trim(),
                MiddleName            = c.MiddleName?.Trim(),
                Surname               = c.Surname?.Trim(),
                Title                 = c.Title?.Trim(),
                Position              = c.Position?.Trim(),
                Address               = c.Address?.Trim(),
                Tel1                  = c.Tel1?.Trim(),
                Tel2                  = c.Tel2?.Trim(),
                MobilePhone           = c.MobilePhone?.Trim(),
                Fax                   = c.Fax?.Trim(),
                Email                 = c.Email?.Trim(),
                EmailGroup            = c.EmailGroup?.Trim(),
                Pager                 = c.Pager?.Trim(),
                Remarks1              = c.Remarks1?.Trim(),
                Remarks2              = c.Remarks2?.Trim(),
                Password              = c.Password?.Trim(),
                CountryOfBirth        = c.CountryOfBirth?.Trim(),
                DateOfBirth           = c.DateOfBirth,
                Gender                = c.Gender,
                Profession            = c.Profession?.Trim(),
                CityOfBirth           = c.CityOfBirth?.Trim(),
                ConnectedAddress      = c.ConnectedAddress?.Trim(),
                BlockMarketingContent = c.BlockMarketingContent,
                Active                = c.Active,
                EDocRecipient         = c.EDocRecipient,
                IsDefault             = c.IsDefault,
                CreatedAt             = DateTime.UtcNow,
            });

        foreach (var b in r.BankAccts)
            bp.BankAccts.Add(new BPBankAccount
            {
                BankCountry     = b.BankCountry?.Trim(),
                BankName        = b.BankName?.Trim(),
                BankCode        = b.BankCode?.Trim(),
                AccountNo       = b.AccountNo?.Trim(),
                SwiftNum        = b.SwiftNum?.Trim(),
                AccountName     = b.AccountName?.Trim(),
                Branch          = b.Branch?.Trim(),
                CtrlIntId       = b.CtrlIntId?.Trim(),
                Iban            = b.Iban?.Trim(),
                MandateId       = b.MandateId?.Trim(),
                DateOfSignature = b.DateOfSignature,
                Currency        = b.Currency?.Trim(),
                IsDefault       = b.IsDefault,
                CreatedAt       = DateTime.UtcNow,
            });

        foreach (var p in r.PaymentMethods)
            bp.PaymentMethods.Add(new BPPaymentMethod
            {
                Code        = p.Code?.Trim(),
                Description = p.Description?.Trim(),
                Include     = p.Include,
                Active      = p.Active,
                CreatedAt   = DateTime.UtcNow,
            });
    }

    // ── MAP ENTITY → RESPONSE ─────────────────────────────────────
    private static BPResponse MapToResponse(BusinessPartnersMaster bp) => new()
    {
        Id         = bp.Id,
        Code       = bp.Code,
        CodeSeries = bp.CodeSeries,
        CardName   = bp.CardName,
        FrgnName   = bp.FrgnName,
        AliasName  = bp.AliasName,

        TypeStatus        = bp.TypeStatus,
        TypeStatusLabel   = bp.TypeStatus == 0 ? "Customer" : bp.TypeStatus == 1 ? "Vendor" : "Lead",
        ActiveStatus      = bp.ActiveStatus,
        ActiveStatusLabel = bp.ActiveStatus == 0 ? "Active" : bp.ActiveStatus == 1 ? "Inactive" : "Advanced",
        ActiveFrom        = bp.ActiveFrom,
        ActiveTo          = bp.ActiveTo,
        ActiveRemarks     = bp.ActiveRemarks,

        TypeOfBusiness      = bp.TypeOfBusiness,
        TypeOfBusinessLabel = bp.TypeOfBusiness switch { 1 => "Private", 2 => "Government", 3 => "Employee", _ => "Company" },

        GroupCode = bp.GroupCode, Currency = bp.Currency, VatNumber = bp.VatNumber,
        UnifiedVatNumber = bp.UnifiedVatNumber, IdNo2 = bp.IdNo2,

        Tel1 = bp.Tel1, Tel2 = bp.Tel2, MobilePhone = bp.MobilePhone,
        Fax = bp.Fax, Email = bp.Email, Website = bp.Website,
        ShippingType = bp.ShippingType, Password = bp.Password,
        FactoringIndicator = bp.FactoringIndicator, BPProject = bp.BPProject,
        Industry = bp.Industry,

        DefaultContactPerson = bp.DefaultContactPerson, Remarks = bp.Remarks,
        SalesEmployee = bp.SalesEmployee, BPChannelCode = bp.BPChannelCode,
        Technician = bp.Technician, Territory = bp.Territory,
        GLN = bp.GLN, BranchAssignment = bp.BranchAssignment,
        BlockMarketingContent = bp.BlockMarketingContent,

        PayTerms = bp.PayTerms, InterestOnArrears = bp.InterestOnArrears,
        PriceList = bp.PriceList, TotalDiscount = bp.TotalDiscount,
        CreditLimit = bp.CreditLimit, CommitmentLimit = bp.CommitmentLimit,
        DunningTerm = bp.DunningTerm, EffectiveDiscountGroups = bp.EffectiveDiscountGroups,
        EffectivePrice = bp.EffectivePrice, CreditCardType = bp.CreditCardType,
        CreditCardNo = bp.CreditCardNo, CreditCardExpiry = bp.CreditCardExpiry,
        CreditCardIdNumber = bp.CreditCardIdNumber, AverageDelay = bp.AverageDelay,
        Priority = bp.Priority, DefaultIban = bp.DefaultIban,
        Holidays = bp.Holidays, PaymentDates = bp.PaymentDates,
        AllowPartialDeliveryOfSO = bp.AllowPartialDeliveryOfSO,
        AllowPartialDeliveryPerRow = bp.AllowPartialDeliveryPerRow,
        DoNotApplyDiscountGroups = bp.DoNotApplyDiscountGroups,
        EndorsableCheques = bp.EndorsableCheques,
        AcceptsEndorsedCheques = bp.AcceptsEndorsedCheques,

        HouseBankCountry = bp.HouseBankCountry, HouseBankCode = bp.HouseBankCode,
        HouseBankAccount = bp.HouseBankAccount, HouseBankBranch = bp.HouseBankBranch,
        HouseBankIban = bp.HouseBankIban, HouseBankSwift = bp.HouseBankSwift,
        HouseBankControlNo = bp.HouseBankControlNo, ReferenceDetails = bp.ReferenceDetails,
        PaymentBlock = bp.PaymentBlock, PaymentBlockCode = bp.PaymentBlockCode,
        SinglePayment = bp.SinglePayment, CollectionAuthorisation = bp.CollectionAuthorisation,
        BankChargesAllocationCode = bp.BankChargesAllocationCode,
        AutoCalcBankCharges = bp.AutoCalcBankCharges,

        ConsolidatingBP = bp.ConsolidatingBP, ConsolidationType = bp.ConsolidationType,
        ARControlAccount = bp.ARControlAccount,
        DownPaymentClearingAccount = bp.DownPaymentClearingAccount,
        DownPaymentInterimAccount = bp.DownPaymentInterimAccount,
        BlockDunningLetters = bp.BlockDunningLetters, DunningLevel = bp.DunningLevel,
        DunningDate = bp.DunningDate, ConnectedSupplier = bp.ConnectedSupplier,
        PlanningGroup = bp.PlanningGroup, UseShippedGoodsAccount = bp.UseShippedGoodsAccount,
        Affiliate = bp.Affiliate,

        TaxId = bp.TaxId, VatGroup = bp.VatGroup,

        Balance = bp.Balance, DeliveriesBalance = bp.DeliveriesBalance,
        OrdersBalance = bp.OrdersBalance, OpportunitiesBalance = bp.OpportunitiesBalance,
        ChequesBalance = bp.ChequesBalance,

        Addresses = bp.Addresses.Select(a => new BPAddressDto
        {
            Id = a.Id, AdresType = a.AdresType, AdressName = a.AdressName,
            AddressName2 = a.AddressName2, AddressName3 = a.AddressName3,
            Street = a.Street, StreetNo = a.StreetNo, Block = a.Block,
            BuildingFloorRoom = a.BuildingFloorRoom, City = a.City,
            ZipCode = a.ZipCode, County = a.County, State = a.State,
            Country = a.Country, VatNumber = a.VatNumber,
            TaxOffice = a.TaxOffice, GLN = a.GLN, IsDefault = a.IsDefault,
        }).ToList(),

        Contacts = bp.Contacts.Select(c => new BPContactDto
        {
            Id = c.Id, Name = c.Name, FirstName = c.FirstName,
            MiddleName = c.MiddleName, Surname = c.Surname, Title = c.Title,
            Position = c.Position, Address = c.Address,
            Tel1 = c.Tel1, Tel2 = c.Tel2, MobilePhone = c.MobilePhone,
            Fax = c.Fax, Email = c.Email, EmailGroup = c.EmailGroup,
            Pager = c.Pager, Remarks1 = c.Remarks1, Remarks2 = c.Remarks2,
            Password = c.Password, CountryOfBirth = c.CountryOfBirth,
            DateOfBirth = c.DateOfBirth, Gender = c.Gender,
            Profession = c.Profession, CityOfBirth = c.CityOfBirth,
            ConnectedAddress = c.ConnectedAddress,
            BlockMarketingContent = c.BlockMarketingContent,
            Active = c.Active, EDocRecipient = c.EDocRecipient, IsDefault = c.IsDefault,
        }).ToList(),

        BankAccts = bp.BankAccts.Select(b => new BPBankAccountDto
        {
            Id = b.Id, BankCountry = b.BankCountry, BankName = b.BankName,
            BankCode = b.BankCode, AccountNo = b.AccountNo, SwiftNum = b.SwiftNum,
            AccountName = b.AccountName, Branch = b.Branch, CtrlIntId = b.CtrlIntId,
            Iban = b.Iban, MandateId = b.MandateId, DateOfSignature = b.DateOfSignature,
            Currency = b.Currency, IsDefault = b.IsDefault,
        }).ToList(),

        PaymentMethods = bp.PaymentMethods.Select(p => new BPPaymentMethodDto
        {
            Id = p.Id, Code = p.Code, Description = p.Description,
            Include = p.Include, Active = p.Active,
        }).ToList(),

        VersionNum = bp.VersionNum,
        CreatedAt  = bp.CreatedAt,
        UpdatedAt  = bp.UpdatedAt,
    };
}
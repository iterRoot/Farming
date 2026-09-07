using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
// using FarmingApi.Services;

namespace FarmingApi.Modules.Company;

public class CompanyController : MyController
{
	private readonly IMapper _mapper;
	private readonly ICompanyRepository _repository;
	// private readonly ICloudStorageSingletonService _service;

	public CompanyController(
		ICompanyRepository repository,
		IMapper mapper
		// ICloudStorageSingletonService service
	)
	{
		_mapper = mapper;
		_repository = repository;
		// _service = service;
	}

	// ═══════════════════════════════════════════════════════════
	// COMPANY DETAILS — singleton
	// The Company Details screen edits "the" company, so these two
	// endpoints read/write the single record and create it on first save.
	// ═══════════════════════════════════════════════════════════

	// ── GET /Company/Details ───────────────────────────────────
	[AllowAnonymous]
	[HttpGet("Details")]
	public IActionResult GetDetails()
	{
		var item = _repository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		// Nothing saved yet → return an empty shell so the form still loads
		return Ok(item == null ? new CompanyDetailsDto() : ToDto(item));
	}

	// ── PUT /Company/Details ───────────────────────────────────
	[AllowAnonymous]
	[HttpPut("Details")]
	public IActionResult SaveDetails([FromBody] CompanyDetailsDto dto)
	{
		if (dto == null) return BadRequest("No company details supplied.");

		var name = (dto.Local?.CompanyName ?? "").Trim();
		if (name.Length == 0) return BadRequest("Company Name is required.");

		var item = _repository.GetAll().OrderBy(x => x.Id).FirstOrDefault();
		var isNew = item == null;
		if (isNew) item = new Company { Name = name, CreatedAt = DateTime.UtcNow, InActive = false };

		ApplyDto(item!, dto);
		item!.Name = name;                       // keep the legacy summary field in step

		if (isNew) _repository.Add(item);
		else { item.UpdatedAt = DateTime.UtcNow; _repository.Update(item); }

		_repository.Commit();
		return Ok(ToDto(item));
	}

	// ── mapping helpers ────────────────────────────────────────
	private static CompanyDetailsDto ToDto(Company c) => new()
	{
		Id = c.Id,
		Local = new CompanyLanguageBlock
		{
			CompanyName = c.LocalCompanyName, Street = c.LocalStreet, StreetNo = c.LocalStreetNo,
			Block = c.LocalBlock, BuildingFloorRoom = c.LocalBuildingFloorRoom, City = c.LocalCity,
			Postcode = c.LocalPostcode, County = c.LocalCounty, State = c.LocalState,
			CountryRegion = c.LocalCountryRegion, InternetAddress = c.LocalInternetAddress,
			PrintingHeader = c.LocalPrintingHeader, ActiveManager = c.LocalActiveManager,
			AliasName = c.LocalAliasName, Fax = c.LocalFax, Email = c.LocalEmail, Gln = c.LocalGln,
		},
		Foreign = new CompanyLanguageBlock
		{
			CompanyName = c.ForeignCompanyName, Street = c.ForeignStreet, StreetNo = c.ForeignStreetNo,
			Block = c.ForeignBlock, BuildingFloorRoom = c.ForeignBuildingFloorRoom, City = c.ForeignCity,
			Postcode = c.ForeignPostcode, County = c.ForeignCounty, State = c.ForeignState,
			CountryRegion = c.ForeignCountryRegion, InternetAddress = c.ForeignInternetAddress,
			PrintingHeader = c.ForeignPrintingHeader, ActiveManager = c.ForeignActiveManager,
			AliasName = c.ForeignAliasName, Fax = c.ForeignFax, Email = c.ForeignEmail, Gln = c.ForeignGln,
		},
		Accounting = new CompanyAccountingBlock
		{
			TaxOffice = c.TaxOffice, VatNumber1 = c.VatNumber1, VatNumber2 = c.VatNumber2,
			VatNumber3 = c.VatNumber3, CompanyRegNo = c.CompanyRegNo,
			CompanyTaxRate = c.CompanyTaxRate.ToString("0.00"),
			ExemptionNumber = c.ExemptionNumber, TaxDeductionNumber = c.TaxDeductionNumber,
			TaxOfficial = c.TaxOfficial, UseDeferredTax = c.UseDeferredTax,
			ApplyExchangeRateOnDeferredTax = c.ApplyExchangeRateOnDeferredTax,
			TaxRateDetermination = c.TaxRateDetermination, Holidays = c.Holidays,
			ExtendedTaxReporting = c.ExtendedTaxReporting, EoriNumber = c.EoriNumber,
			AllowExternalTaxCalculationOnAR = c.AllowExternalTaxCalculationOnAR,
		},
		BasicInit = new CompanyBasicInitBlock
		{
			ChartOfAccountsTemplate = c.ChartOfAccountsTemplate, LocalCurrency = c.LocalCurrency,
			SystemCurrency = c.SystemCurrency, DefaultAccountCurrency = c.DefaultAccountCurrency,
			DisplayCreditBalanceNegative = c.DisplayCreditBalanceNegative,
			UseSegmentationAccounts = c.UseSegmentationAccounts,
			AllowNegativeReversalPosting = c.AllowNegativeReversalPosting,
			PermitMoreThanOneDocumentType = c.PermitMoreThanOneDocumentType,
			MultiLanguageSupport = c.MultiLanguageSupport, UseContinuousStock = c.UseContinuousStock,
			ItemGroupsValuationMethod = c.ItemGroupsValuationMethod,
			ManageItemCostPerWarehouse = c.ManageItemCostPerWarehouse,
			UsePurchaseAccountsPosting = c.UsePurchaseAccountsPosting,
			AllowStockReleaseWithoutItemCost = c.AllowStockReleaseWithoutItemCost,
			ManageSerialBatchCostBy = c.ManageSerialBatchCostBy,
			EnableSeparateNetGrossPrice = c.EnableSeparateNetGrossPrice,
			OrderingParty = c.OrderingParty, DefaultBankCountry = c.DefaultBankCountry,
			DefaultBank = c.DefaultBank, DefaultAccountNo = c.DefaultAccountNo,
			DefaultBranch = c.DefaultBranch,
			InstallBankStatementProcessing = c.InstallBankStatementProcessing,
			EnableFixedAssets = c.EnableFixedAssets, CalculateDepreciationBy = c.CalculateDepreciationBy,
			EnableMultipleBranches = c.EnableMultipleBranches,
			MaskCreditCardNumber = c.MaskCreditCardNumber,
			EnableAdvancedGLDetermination = c.EnableAdvancedGLDetermination,
			AllowAnyAccountTypeForRevenue = c.AllowAnyAccountTypeForRevenue,
			EnableProjectManagement = c.EnableProjectManagement,
			EnablePersonalDataProtection = c.EnablePersonalDataProtection,
			TermsAndConditionsFile = c.TermsAndConditionsFile,
		},
	};

	private static void ApplyDto(Company c, CompanyDetailsDto d)
	{
		var l = d.Local ?? new(); var fg = d.Foreign ?? new();
		var a = d.Accounting ?? new(); var b = d.BasicInit ?? new();

		c.LocalCompanyName = l.CompanyName; c.LocalStreet = l.Street; c.LocalStreetNo = l.StreetNo;
		c.LocalBlock = l.Block; c.LocalBuildingFloorRoom = l.BuildingFloorRoom; c.LocalCity = l.City;
		c.LocalPostcode = l.Postcode; c.LocalCounty = l.County; c.LocalState = l.State;
		c.LocalCountryRegion = l.CountryRegion; c.LocalInternetAddress = l.InternetAddress;
		c.LocalPrintingHeader = l.PrintingHeader; c.LocalActiveManager = l.ActiveManager;
		c.LocalAliasName = l.AliasName; c.LocalFax = l.Fax; c.LocalEmail = l.Email; c.LocalGln = l.Gln;

		c.ForeignCompanyName = fg.CompanyName; c.ForeignStreet = fg.Street; c.ForeignStreetNo = fg.StreetNo;
		c.ForeignBlock = fg.Block; c.ForeignBuildingFloorRoom = fg.BuildingFloorRoom; c.ForeignCity = fg.City;
		c.ForeignPostcode = fg.Postcode; c.ForeignCounty = fg.County; c.ForeignState = fg.State;
		c.ForeignCountryRegion = fg.CountryRegion; c.ForeignInternetAddress = fg.InternetAddress;
		c.ForeignPrintingHeader = fg.PrintingHeader; c.ForeignActiveManager = fg.ActiveManager;
		c.ForeignAliasName = fg.AliasName; c.ForeignFax = fg.Fax; c.ForeignEmail = fg.Email; c.ForeignGln = fg.Gln;

		c.TaxOffice = a.TaxOffice; c.VatNumber1 = a.VatNumber1; c.VatNumber2 = a.VatNumber2;
		c.VatNumber3 = a.VatNumber3; c.CompanyRegNo = a.CompanyRegNo;
		c.CompanyTaxRate = decimal.TryParse(a.CompanyTaxRate, out var rate) ? rate : 0m;
		c.ExemptionNumber = a.ExemptionNumber; c.TaxDeductionNumber = a.TaxDeductionNumber;
		c.TaxOfficial = a.TaxOfficial; c.UseDeferredTax = a.UseDeferredTax;
		c.ApplyExchangeRateOnDeferredTax = a.ApplyExchangeRateOnDeferredTax;
		c.TaxRateDetermination = a.TaxRateDetermination; c.Holidays = a.Holidays;
		c.ExtendedTaxReporting = a.ExtendedTaxReporting; c.EoriNumber = a.EoriNumber;
		c.AllowExternalTaxCalculationOnAR = a.AllowExternalTaxCalculationOnAR;

		c.ChartOfAccountsTemplate = b.ChartOfAccountsTemplate; c.LocalCurrency = b.LocalCurrency;
		c.SystemCurrency = b.SystemCurrency; c.DefaultAccountCurrency = b.DefaultAccountCurrency;
		c.DisplayCreditBalanceNegative = b.DisplayCreditBalanceNegative;
		c.UseSegmentationAccounts = b.UseSegmentationAccounts;
		c.AllowNegativeReversalPosting = b.AllowNegativeReversalPosting;
		c.PermitMoreThanOneDocumentType = b.PermitMoreThanOneDocumentType;
		c.MultiLanguageSupport = b.MultiLanguageSupport; c.UseContinuousStock = b.UseContinuousStock;
		c.ItemGroupsValuationMethod = b.ItemGroupsValuationMethod;
		c.ManageItemCostPerWarehouse = b.ManageItemCostPerWarehouse;
		c.UsePurchaseAccountsPosting = b.UsePurchaseAccountsPosting;
		c.AllowStockReleaseWithoutItemCost = b.AllowStockReleaseWithoutItemCost;
		c.ManageSerialBatchCostBy = b.ManageSerialBatchCostBy;
		c.EnableSeparateNetGrossPrice = b.EnableSeparateNetGrossPrice;
		c.OrderingParty = b.OrderingParty; c.DefaultBankCountry = b.DefaultBankCountry;
		c.DefaultBank = b.DefaultBank; c.DefaultAccountNo = b.DefaultAccountNo;
		c.DefaultBranch = b.DefaultBranch;
		c.InstallBankStatementProcessing = b.InstallBankStatementProcessing;
		c.EnableFixedAssets = b.EnableFixedAssets; c.CalculateDepreciationBy = b.CalculateDepreciationBy;
		c.EnableMultipleBranches = b.EnableMultipleBranches;
		c.MaskCreditCardNumber = b.MaskCreditCardNumber;
		c.EnableAdvancedGLDetermination = b.EnableAdvancedGLDetermination;
		c.AllowAnyAccountTypeForRevenue = b.AllowAnyAccountTypeForRevenue;
		c.EnableProjectManagement = b.EnableProjectManagement;
		c.EnablePersonalDataProtection = b.EnablePersonalDataProtection;
		c.TermsAndConditionsFile = b.TermsAndConditionsFile;
	}

	[AllowAnonymous]

	[HttpGet]
	public IActionResult Gets()
	{
		var iQueryable = _repository.GetAll();
		var results = _mapper.ProjectTo<CompanyListResponse>(iQueryable).ToList();

		return Ok(results);
	}

	[HttpGet("{id:int}")]
	public IActionResult Get(int id)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		var result = _mapper.Map<CompanyDetailResponse>(item);
		return Ok(result);
	}

	[HttpPost]
	public async Task<IActionResult> Insert([FromForm] CompanyInsertRequest request)
	{
		var item = _mapper.Map<Company>(request);
		// if (request.Logo is not null)
		// {
		// 	var url = await _service.UploadFile(request.Logo.OpenReadStream());
		// 	item.Logo = url;
		// }
		item.CreatedAt = DateTime.UtcNow;
		// item.CreatedBy = GetClaim()!.Id;
		_repository.Add(item);
		_repository.Commit();
		return NoContent();
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromForm] CompanyUpdateRequest request)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		_mapper.Map(request, item);
		// if (request.Logo != null)
		// {
		// 	var stream = request.Logo.OpenReadStream();
		// 	// var url = await _service.ReplaceFile(item.Logo, stream);
		// 	item.Logo = url;
		// }
		item.UpdatedAt = DateTime.UtcNow;
		// item.UpdatedBy = GetClaim()!.Id;
		_repository.Update(item);
		_repository.Commit();
		return NoContent();
	}

	[HttpDelete]
	public IActionResult Delete(int id)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		item.DeletedAt = DateTime.UtcNow;
		// item.DeletedBy = GetClaim()!.Id;
		_repository.Remove(item);
		_repository.Commit();
		return NoContent();
	}
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

public class BusinessPartnersMasterController : MyController
{
    private readonly IBusinessPartnersMasterRepository _repository;

    public BusinessPartnersMasterController(IBusinessPartnersMasterRepository repository)
    {
        _repository = repository;
    }

    // ── GET ALL (list) ────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets([FromQuery] int? typeStatus = null)
    {
        var query = _repository.GetAll();

        // Filter by type: 0=Customer, 1=Vendor, 2=Lead
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
                ActiveStatusLabel = b.ActiveStatus == 0 ? "Active" : "Inactive",
                Tel1              = b.Tel1,
                MobilePhone       = b.MobilePhone,
                Email             = b.Email,
                Balance           = b.Balance,
                CreatedAt         = b.CreatedAt,
            })
            .ToList();

        return Ok(result);
    }

    // ── GET CUSTOMERS ─────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet("Customers")]
    public IActionResult GetCustomers()
        => Gets(typeStatus: 0);

    // ── GET VENDORS ───────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet("Vendors")]
    public IActionResult GetVendors()
        => Gets(typeStatus: 1);

    // ── GET BY ID ─────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var bp = _repository.GetSingle(e => e.Id == id);
        if (bp == null) return NotFound($"Business partner {id} not found");

        return Ok(MapToResponse(bp));
    }

    // ── GET BY CODE ───────────────────────────────────────────────
    [HttpGet("Code/{code}")]
    public IActionResult GetByCode(string code)
    {
        var bp = _repository.GetSingle(e => e.Code == code);
        if (bp == null) return NotFound($"Business partner '{code}' not found");

        return Ok(MapToResponse(bp));
    }

    // ── CREATE ────────────────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] BPCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validations
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Code is required");
        if (string.IsNullOrWhiteSpace(request.CardName))
            return BadRequest("Card Name is required");

        // Sanitize code
        var code = request.Code.Trim().ToUpper();

        // Duplicate check
        if (_repository.GetAll().Any(b => b.Code == code))
            return BadRequest($"Business partner code '{code}' already exists");

        var entity = new BusinessPartnersMaster
        {
            Code         = code,
            CardName     = request.CardName.Trim(),
            FrgnName     = request.FrgnName?.Trim(),
            TypeStatus   = request.TypeStatus,
            ActiveStatus = request.ActiveStatus,
            Tel1         = request.Tel1?.Trim(),
            Tel2         = request.Tel2?.Trim(),
            MobilePhone  = request.MobilePhone?.Trim(),
            Fax          = request.Fax?.Trim(),
            Email        = request.Email?.Trim(),
            Website      = request.Website?.Trim(),
            Note         = request.Note?.Trim(),
            Currency     = request.Currency?.Trim(),
            CreditLimit  = request.CreditLimit,
            PayTerms     = request.PayTerms?.Trim(),
            PriceList    = request.PriceList?.Trim(),
            GroupCode    = request.GroupCode?.Trim(),
            TaxId        = request.TaxId?.Trim(),
            VatGroup     = request.VatGroup?.Trim(),
            Balance      = 0,
            VersionNum   = 1,
            CreatedAt    = DateTime.UtcNow,

            // Sub-tables
            Addresses = request.Addresses.Select(a => new BPAddress
            {
                AdresType  = a.AdresType,
                AdressName = a.AdressName?.Trim(),
                Street     = a.Street?.Trim(),
                Block      = a.Block?.Trim(),
                City       = a.City?.Trim(),
                ZipCode    = a.ZipCode?.Trim(),
                County     = a.County?.Trim(),
                Country    = a.Country?.Trim(),
                State      = a.State?.Trim(),
                IsDefault  = a.IsDefault,
                CreatedAt  = DateTime.UtcNow,
            }).ToList(),

            Contacts = request.Contacts.Select(c => new BPContact
            {
                Name        = c.Name?.Trim(),
                Position    = c.Position?.Trim(),
                Tel1        = c.Tel1?.Trim(),
                MobilePhone = c.MobilePhone?.Trim(),
                Email       = c.Email?.Trim(),
                Note        = c.Note?.Trim(),
                IsDefault   = c.IsDefault,
                CreatedAt   = DateTime.UtcNow,
            }).ToList(),

            BankAccts = request.BankAccts.Select(b => new BPBankAccount
            {
                BankCode  = b.BankCode?.Trim(),
                BankName  = b.BankName?.Trim(),
                AccountNo = b.AccountNo?.Trim(),
                Branch    = b.Branch?.Trim(),
                Currency  = b.Currency?.Trim(),
                Iban      = b.Iban?.Trim(),
                SwiftNum  = b.SwiftNum?.Trim(),
                IsDefault = b.IsDefault,
                CreatedAt = DateTime.UtcNow,
            }).ToList(),
        };

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Business partner created", id = entity.Id, code = entity.Code });
    }

    // ── UPDATE ────────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BPUpdateRequest request)
    {
        var bp = _repository.GetSingle(e => e.Id == id);
        if (bp == null) return NotFound($"Business partner {id} not found");

        if (string.IsNullOrWhiteSpace(request.CardName))
            return BadRequest("Card Name is required");

        // Update scalar fields
        bp.CardName     = request.CardName.Trim();
        bp.FrgnName     = request.FrgnName?.Trim();
        bp.TypeStatus   = request.TypeStatus;
        bp.ActiveStatus = request.ActiveStatus;
        bp.Tel1         = request.Tel1?.Trim();
        bp.Tel2         = request.Tel2?.Trim();
        bp.MobilePhone  = request.MobilePhone?.Trim();
        bp.Fax          = request.Fax?.Trim();
        bp.Email        = request.Email?.Trim();
        bp.Website      = request.Website?.Trim();
        bp.Note         = request.Note?.Trim();
        bp.Currency     = request.Currency?.Trim();
        bp.CreditLimit  = request.CreditLimit;
        bp.PayTerms     = request.PayTerms?.Trim();
        bp.PriceList    = request.PriceList?.Trim();
        bp.GroupCode    = request.GroupCode?.Trim();
        bp.TaxId        = request.TaxId?.Trim();
        bp.VatGroup     = request.VatGroup?.Trim();
        bp.UpdatedAt    = DateTime.UtcNow;
        bp.VersionNum  += 1;

        // ── Full replace for sub-tables ──────────────────────────
        bp.Addresses.Clear();
        foreach (var a in request.Addresses)
            bp.Addresses.Add(new BPAddress
            {
                AdresType  = a.AdresType,
                AdressName = a.AdressName?.Trim(),
                Street     = a.Street?.Trim(),
                Block      = a.Block?.Trim(),
                City       = a.City?.Trim(),
                ZipCode    = a.ZipCode?.Trim(),
                County     = a.County?.Trim(),
                Country    = a.Country?.Trim(),
                State      = a.State?.Trim(),
                IsDefault  = a.IsDefault,
                CreatedAt  = DateTime.UtcNow,
            });

        bp.Contacts.Clear();
        foreach (var c in request.Contacts)
            bp.Contacts.Add(new BPContact
            {
                Name        = c.Name?.Trim(),
                Position    = c.Position?.Trim(),
                Tel1        = c.Tel1?.Trim(),
                MobilePhone = c.MobilePhone?.Trim(),
                Email       = c.Email?.Trim(),
                Note        = c.Note?.Trim(),
                IsDefault   = c.IsDefault,
                CreatedAt   = DateTime.UtcNow,
            });

        bp.BankAccts.Clear();
        foreach (var b in request.BankAccts)
            bp.BankAccts.Add(new BPBankAccount
            {
                BankCode  = b.BankCode?.Trim(),
                BankName  = b.BankName?.Trim(),
                AccountNo = b.AccountNo?.Trim(),
                Branch    = b.Branch?.Trim(),
                Currency  = b.Currency?.Trim(),
                Iban      = b.Iban?.Trim(),
                SwiftNum  = b.SwiftNum?.Trim(),
                IsDefault = b.IsDefault,
                CreatedAt = DateTime.UtcNow,
            });

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

        if (bp.Balance != 0)
            return BadRequest("Cannot delete a business partner with outstanding balance");

        _repository.Remove(bp);
        _repository.Commit();

        return NoContent();
    }

    // ── HELPER: Map entity → response ─────────────────────────────
    private static BPResponse MapToResponse(BusinessPartnersMaster bp) => new()
    {
        Id                = bp.Id,
        Code              = bp.Code,
        CardName          = bp.CardName,
        FrgnName          = bp.FrgnName,
        TypeStatus        = bp.TypeStatus,
        TypeStatusLabel   = bp.TypeStatus == 0 ? "Customer" : bp.TypeStatus == 1 ? "Vendor" : "Lead",
        ActiveStatus      = bp.ActiveStatus,
        ActiveStatusLabel = bp.ActiveStatus == 0 ? "Active" : "Inactive",
        Tel1              = bp.Tel1,
        Tel2              = bp.Tel2,
        MobilePhone       = bp.MobilePhone,
        Fax               = bp.Fax,
        Email             = bp.Email,
        Website           = bp.Website,
        Note              = bp.Note,
        Currency          = bp.Currency,
        Balance           = bp.Balance,
        CreditLimit       = bp.CreditLimit,
        PayTerms          = bp.PayTerms,
        PriceList         = bp.PriceList,
        GroupCode         = bp.GroupCode,
        TaxId             = bp.TaxId,
        VatGroup          = bp.VatGroup,
        VersionNum        = bp.VersionNum,
        CreatedAt         = bp.CreatedAt,
        UpdatedAt         = bp.UpdatedAt,

        Addresses = bp.Addresses.Select(a => new BPAddressDto
        {
            Id         = a.Id,
            AdresType  = a.AdresType,
            AdressName = a.AdressName,
            Street     = a.Street,
            Block      = a.Block,
            City       = a.City,
            ZipCode    = a.ZipCode,
            County     = a.County,
            Country    = a.Country,
            State      = a.State,
            IsDefault  = a.IsDefault,
        }).ToList(),

        Contacts = bp.Contacts.Select(c => new BPContactDto
        {
            Id          = c.Id,
            Name        = c.Name,
            Position    = c.Position,
            Tel1        = c.Tel1,
            MobilePhone = c.MobilePhone,
            Email       = c.Email,
            Note        = c.Note,
            IsDefault   = c.IsDefault,
        }).ToList(),

        BankAccts = bp.BankAccts.Select(b => new BPBankAccountDto
        {
            Id        = b.Id,
            BankCode  = b.BankCode,
            BankName  = b.BankName,
            AccountNo = b.AccountNo,
            Branch    = b.Branch,
            Currency  = b.Currency,
            Iban      = b.Iban,
            SwiftNum  = b.SwiftNum,
            IsDefault = b.IsDefault,
        }).ToList(),
    };
}
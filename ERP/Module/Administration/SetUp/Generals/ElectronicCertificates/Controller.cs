using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.ElectronicCertificates;

[ApiController]
[Route("[controller]")]
public class ElectronicCertificateController : ControllerBase
{
    private readonly IElectronicCertificateRepository _repo;
    private readonly IMapper                          _mapper;
    private readonly MyDbContext                      _db;

    public ElectronicCertificateController(
        IElectronicCertificateRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /ElectronicCertificate ─────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? certType    = null,
        [FromQuery] string? holderType  = null,
        [FromQuery] string? holderCode  = null,
        [FromQuery] string? status      = null,
        [FromQuery] string? linkedModule= null,
        [FromQuery] int     page        = 1,
        [FromQuery] int     pageSize    = 50)
    {
        var now   = DateTime.UtcNow.Date;
        var query = _repo.GetAll()
            .Include(x => x.Attributes.OrderBy(a => a.SortOrder))
            .Where(x => certType    == null || x.CertType   == certType)
            .Where(x => holderType  == null || x.HolderType == holderType)
            .Where(x => holderCode  == null || x.HolderCode == holderCode)
            .Where(x => status      == null || x.Status     == status)
            .Where(x => linkedModule== null || x.LinkedModule == linkedModule);

        // Auto-expire in query filter
        var total = query.Count();
        var list  = query
            .OrderByDescending(x => x.IssuedDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToList();

        // Auto-update Expired status
        foreach (var c in list.Where(c => c.Status == "Active" && c.ExpiryDate.HasValue && c.ExpiryDate.Value.Date < now))
        {
            c.Status = "Expired";
            _repo.Update(c);
        }
        _repo.Commit();

        return Ok(new
        {
            total,
            page,
            pageSize,
            pages = (int)Math.Ceiling(total / (double)pageSize),
            data  = _mapper.Map<List<ElectronicCertificateResponse>>(list),
        });
    }

    // ── GET /ElectronicCertificate/{id} ────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Attributes.OrderBy(a => a.SortOrder))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ElectronicCertificateResponse>(e));
    }

    // ── GET /ElectronicCertificate/ByNo/{certNo} ──────────────
    [AllowAnonymous][HttpGet("ByNo/{certNo}")]
    public IActionResult GetByNo(string certNo)
    {
        var e = _repo.GetAll()
            .Include(x => x.Attributes)
            .FirstOrDefault(x => x.CertNo == certNo.Trim().ToUpper());
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ElectronicCertificateResponse>(e));
    }

    // ── GET /ElectronicCertificate/ExpiryAlerts ───────────────
    // Returns certs expiring within N days (default 90)
    [AllowAnonymous][HttpGet("ExpiryAlerts")]
    public IActionResult GetExpiryAlerts([FromQuery] int days = 90)
    {
        var threshold = DateTime.UtcNow.Date.AddDays(days);
        var list = _repo.GetAll()
            .Where(x => x.Status == "Active"
                && x.ExpiryDate.HasValue
                && x.ExpiryDate.Value.Date <= threshold
                && x.ExpiryDate.Value.Date >= DateTime.UtcNow.Date)
            .OrderBy(x => x.ExpiryDate)
            .ToList();
        return Ok(_mapper.Map<List<ElectronicCertificateResponse>>(list));
    }

    // ── GET /ElectronicCertificate/Summary ────────────────────
    [AllowAnonymous][HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        var now = DateTime.UtcNow.Date;

        // Sync expired
        foreach (var c in all.Where(c =>
            c.Status == "Active" && c.ExpiryDate.HasValue && c.ExpiryDate.Value.Date < now))
        {
            c.Status = "Expired"; _repo.Update(c);
        }
        _repo.Commit();

        return Ok(new CertSummary
        {
            Total        = all.Count,
            Active       = all.Count(x => x.Status == "Active"),
            Expired      = all.Count(x => x.Status == "Expired"),
            Pending      = all.Count(x => x.Status == "Pending"),
            Revoked      = all.Count(x => x.Status == "Revoked"),
            ExpiringSoon = all.Count(x =>
                x.Status == "Active" &&
                x.ExpiryDate.HasValue &&
                x.ExpiryDate.Value.Date >= now &&
                (x.ExpiryDate.Value.Date - now).Days <= (x.RenewalDaysAlert ?? 90)),
            ByType   = all.GroupBy(x => x.CertType)
                          .ToDictionary(g => g.Key, g => g.Count()),
            ByHolder = all.GroupBy(x => x.HolderType)
                          .ToDictionary(g => g.Key, g => g.Count()),
        });
    }

    // ── POST /ElectronicCertificate ────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ElectronicCertificateRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity       = _mapper.Map<ElectronicCertificate>(dto);
        entity.CertNo    = GenerateCertNo(dto.CertType);
        entity.Status    = "Pending";
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        int order = 1;
        foreach (var a in entity.Attributes)
            a.SortOrder = a.SortOrder > 0 ? a.SortOrder : order++;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicCertificateResponse>(entity));
    }

    // ── PUT /ElectronicCertificate/{id} ────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ElectronicCertificateRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Attributes)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Revoked")
            return BadRequest("Revoked certificates cannot be edited");

        _db.RemoveRange(entity.Attributes);
        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        int order = 1;
        foreach (var a in entity.Attributes)
            a.SortOrder = a.SortOrder > 0 ? a.SortOrder : order++;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicCertificateResponse>(entity));
    }

    // ── POST /ElectronicCertificate/{id}/Verify ───────────────
    [AllowAnonymous][HttpPost("{id:int}/Verify")]
    public IActionResult Verify(int id, [FromQuery] string? verifiedBy = null)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Revoked") return BadRequest("Cannot verify a revoked certificate");

        entity.Status     = "Active";
        entity.VerifiedAt = DateTime.UtcNow;
        entity.VerifiedBy = verifiedBy;
        entity.UpdatedAt  = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"Certificate '{entity.CertNo}' verified and activated", verifiedAt = entity.VerifiedAt });
    }

    // ── POST /ElectronicCertificate/{id}/Revoke ───────────────
    [AllowAnonymous][HttpPost("{id:int}/Revoke")]
    public IActionResult Revoke(int id, [FromBody] RevokeRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Revoked") return BadRequest("Certificate is already revoked");

        entity.Status           = "Revoked";
        entity.RevocationReason = dto.Reason;
        entity.RevokedAt        = DateTime.UtcNow;
        entity.IsActive         = false;
        entity.UpdatedAt        = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"Certificate '{entity.CertNo}' has been revoked", revokedAt = entity.RevokedAt });
    }

    // ── POST /ElectronicCertificate/{id}/Renew ────────────────
    // Creates a new certificate as the renewal of an existing one
    [AllowAnonymous][HttpPost("{id:int}/Renew")]
    public IActionResult Renew(int id, [FromBody] RenewRequest dto)
    {
        var old = _repo.GetAll()
            .Include(x => x.Attributes)
            .FirstOrDefault(x => x.Id == id);
        if (old == null) return NotFound();
        if (old.RenewedToId.HasValue)
            return BadRequest("This certificate has already been renewed");

        var newCert = new ElectronicCertificate
        {
            CertNo           = GenerateCertNo(old.CertType),
            CertName         = old.CertName,
            CertType         = old.CertType,
            CertStandard     = old.CertStandard,
            SerialNumber     = dto.NewSerialNumber ?? old.SerialNumber,
            CertBody         = old.CertBody,
            HolderType       = old.HolderType,
            HolderCode       = old.HolderCode,
            HolderName       = old.HolderName,
            HolderDept       = old.HolderDept,
            IssuingAuthority = old.IssuingAuthority,
            IssuingCountry   = old.IssuingCountry,
            AuthorityContact = old.AuthorityContact,
            IssuedDate       = dto.NewIssuedDate,
            ExpiryDate       = dto.NewExpiryDate,
            RenewalDaysAlert = old.RenewalDaysAlert,
            Status           = "Pending",
            FileUrl          = dto.NewFileUrl ?? old.FileUrl,
            LinkedModule     = old.LinkedModule,
            LinkedDocType    = old.LinkedDocType,
            LinkedDocId      = old.LinkedDocId,
            LinkedDocNo      = old.LinkedDocNo,
            Scope            = old.Scope,
            CoveredSites     = old.CoveredSites,
            Tags             = old.Tags,
            IsPublic         = old.IsPublic,
            IsActive         = true,
            RenewedFromId    = old.Id,
            Remarks          = dto.Notes,
            CreatedAt        = DateTime.UtcNow,
            InActive         = false,
            Attributes       = old.Attributes.Select(a => new CertificateAttribute
            {
                AttrKey   = a.AttrKey,
                AttrLabel = a.AttrLabel,
                AttrValue = a.AttrValue,
                DataType  = a.DataType,
                SortOrder = a.SortOrder,
            }).ToList(),
        };

        _repo.Add(newCert);
        old.RenewedToId = 0; // temp to mark renewal initiated
        old.Status      = "Renewed";
        old.UpdatedAt   = DateTime.UtcNow;
        _repo.Update(old);
        _repo.Commit();

        // Update cross-reference now we have the new ID
        old.RenewedToId    = newCert.Id;
        newCert.RenewedFromId = old.Id;
        _repo.Update(old);
        _repo.Update(newCert);
        _repo.Commit();

        return Ok(_mapper.Map<ElectronicCertificateResponse>(newCert));
    }

    // ── POST /ElectronicCertificate/{id}/Sign ─────────────────
    [AllowAnonymous][HttpPost("{id:int}/Sign")]
    public IActionResult Sign(int id,
        [FromQuery] string? certThumbprint = null,
        [FromQuery] string? hashValue      = null)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsSigned      = true;
        entity.SignatureCert = certThumbprint;
        entity.SignatureHash = hashValue;
        entity.SignedAt      = DateTime.UtcNow;
        entity.UpdatedAt     = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = "Certificate digitally signed", signedAt = entity.SignedAt });
    }

    // ── DELETE /ElectronicCertificate/{id} ────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Active")
            return BadRequest("Cannot delete an active certificate. Revoke it first.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /ElectronicCertificate/Seed ──────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any()) return BadRequest("Certificates already seeded.");
        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} certificates seeded" });
    }

    // ─── Helpers ──────────────────────────────────────────────
    private string GenerateCertNo(string certType)
    {
        var prefix = certType switch
        {
            "Quality"      => "CRT-QTY",
            "Compliance"   => "CRT-CMP",
            "Origin"       => "CRT-ORG",
            "Safety"       => "CRT-SAF",
            "Environmental"=> "CRT-ENV",
            "Professional" => "CRT-PRO",
            "Product"      => "CRT-PRD",
            "Training"     => "CRT-TRN",
            "Vendor"       => "CRT-VND",
            _              => "CRT-CST",
        };
        var count = _repo.GetAll().Count(x => x.CertType == certType) + 1;
        return $"{prefix}-{DateTime.UtcNow.Year}-{count:D4}";
    }

    private static List<ElectronicCertificate> GetSeedData()
    {
        var today  = DateTime.UtcNow.Date;
        return new()
        {
            // ── Quality ─────────────────────────────────────
            new()
            {
                CertNo="CRT-QTY-2025-0001", CertName="ISO 9001:2015 Quality Management",
                CertType="Quality", CertStandard="ISO 9001:2015", SerialNumber="BV-KH-2025-001",
                CertBody="Bureau Veritas", HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="Bureau Veritas Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddYears(-1), ExpiryDate=today.AddYears(2),
                RenewalDaysAlert=90, Status="Active", IsPublic=true,
                Scope="Design and manufacture of agricultural products",
                VerifiedAt=today.AddYears(-1), VerifiedBy="admin",
                Tags="iso,quality,9001",
                Attributes=new List<CertificateAttribute>{
                    new(){ AttrKey="AuditDate",    AttrLabel="Last Audit Date",  DataType="Date",   AttrValue=today.AddMonths(-3).ToString("yyyy-MM-dd"), SortOrder=1 },
                    new(){ AttrKey="NextAudit",    AttrLabel="Next Audit Date",  DataType="Date",   AttrValue=today.AddMonths(9).ToString("yyyy-MM-dd"),  SortOrder=2 },
                    new(){ AttrKey="LeadAuditor",  AttrLabel="Lead Auditor",     DataType="Text",   AttrValue="John Smith",                               SortOrder=3 },
                    new(){ AttrKey="AuditScope",   AttrLabel="Audit Scope",      DataType="Text",   AttrValue="Full organisation",                         SortOrder=4 },
                }
            },
            new()
            {
                CertNo="CRT-QTY-2024-0002", CertName="HACCP Food Safety Certification",
                CertType="Quality", CertStandard="HACCP", CertBody="SGS",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="SGS Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddYears(-2), ExpiryDate=today.AddMonths(-3),
                RenewalDaysAlert=90, Status="Expired",
                Tags="haccp,food,safety",
            },

            // ── Compliance ──────────────────────────────────
            new()
            {
                CertNo="CRT-CMP-2025-0001", CertName="Business Registration Certificate",
                CertType="Compliance", CertBody="Ministry of Commerce",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="Ministry of Commerce, Kingdom of Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddYears(-5), ExpiryDate=today.AddYears(1),
                RenewalDaysAlert=60, Status="Active",
                SerialNumber="COMP-KH-2020-001", Tags="business,registration,commerce",
                Attributes=new List<CertificateAttribute>{
                    new(){ AttrKey="RegistrationNo",AttrLabel="Registration Number",DataType="Text",AttrValue="001234/MOC/2020",SortOrder=1 },
                    new(){ AttrKey="BusinessType",  AttrLabel="Business Type",      DataType="Text",AttrValue="Limited Company", SortOrder=2 },
                    new(){ AttrKey="TaxId",         AttrLabel="Tax ID",             DataType="Text",AttrValue="K001-1234567",    SortOrder=3 },
                }
            },
            new()
            {
                CertNo="CRT-CMP-2025-0002", CertName="VAT Registration Certificate",
                CertType="Compliance", CertBody="General Department of Taxation",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="GDT Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddYears(-5), ExpiryDate=null,
                Status="Active", Tags="vat,tax,gdt",
                Attributes=new List<CertificateAttribute>{
                    new(){ AttrKey="VATNo", AttrLabel="VAT Number", DataType="Text", AttrValue="V001-1234567", SortOrder=1 },
                }
            },

            // ── Certificate of Origin ───────────────────────
            new()
            {
                CertNo="CRT-ORG-2026-0001", CertName="Certificate of Origin — Rice Export",
                CertType="Origin", CertBody="Cambodia Chamber of Commerce",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="Cambodia Chamber of Commerce", IssuingCountry="KH",
                IssuedDate=today.AddDays(-15), ExpiryDate=today.AddDays(180),
                RenewalDaysAlert=30, Status="Active",
                Tags="origin,export,rice,co",
                Attributes=new List<CertificateAttribute>{
                    new(){ AttrKey="HSCode",        AttrLabel="HS Code",            DataType="Text",  AttrValue="1006.30.90", SortOrder=1 },
                    new(){ AttrKey="Destination",   AttrLabel="Destination Country",DataType="Text",  AttrValue="China",      SortOrder=2 },
                    new(){ AttrKey="Quantity",      AttrLabel="Quantity (MT)",      DataType="Number",AttrValue="50.000",     SortOrder=3 },
                    new(){ AttrKey="InvoiceNo",     AttrLabel="Invoice Number",     DataType="Text",  AttrValue="INV-2026-0045",SortOrder=4 },
                }
            },

            // ── Safety ──────────────────────────────────────
            new()
            {
                CertNo="CRT-SAF-2025-0001", CertName="Fire Safety Compliance Certificate",
                CertType="Safety", CertBody="National Police - Fire Department",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="Cambodia Fire Department", IssuingCountry="KH",
                IssuedDate=today.AddYears(-1), ExpiryDate=today.AddYears(1),
                RenewalDaysAlert=60, Status="Active",
                Tags="fire,safety,compliance",
            },

            // ── Product ─────────────────────────────────────
            new()
            {
                CertNo="CRT-PRD-2025-0001", CertName="Halal Certificate — Processed Rice",
                CertType="Product", CertBody="Islamic Board of Cambodia",
                HolderType="Product", HolderName="Jasmine Rice 5kg",
                IssuingAuthority="Islamic Board of Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddMonths(-8), ExpiryDate=today.AddMonths(16),
                RenewalDaysAlert=60, Status="Active",
                LinkedModule="ItemMaster", LinkedDocType="ItemMaster",
                Tags="halal,product,rice",
            },
            new()
            {
                CertNo="CRT-PRD-2025-0002", CertName="Organic Certification — Farm Produce",
                CertType="Product", CertBody="Organic Farming Association",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="Organic Farming Association of Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddMonths(-6), ExpiryDate=today.AddMonths(18),
                RenewalDaysAlert=90, Status="Active",
                Tags="organic,farm,produce",
            },

            // ── Professional ────────────────────────────────
            new()
            {
                CertNo="CRT-PRO-2025-0001", CertName="CPA Licence — Chief Accountant",
                CertType="Professional", CertBody="Kampuchea Institute of CPAs",
                HolderType="Employee", HolderCode="EMP-001",
                HolderName="Sophea Chann", HolderDept="Finance",
                IssuingAuthority="KICPAA", IssuingCountry="KH",
                IssuedDate=today.AddYears(-2), ExpiryDate=today.AddYears(1),
                RenewalDaysAlert=60, Status="Active",
                LinkedModule="HR", LinkedDocType="Employee",
                Tags="cpa,accounting,professional",
                Attributes=new List<CertificateAttribute>{
                    new(){ AttrKey="LicenceNo",   AttrLabel="Licence Number",    DataType="Text",  AttrValue="CPA-KH-00234",  SortOrder=1 },
                    new(){ AttrKey="CPEHours",    AttrLabel="CPE Hours Required",DataType="Number",AttrValue="40",            SortOrder=2 },
                }
            },
            new()
            {
                CertNo="CRT-TRN-2025-0001", CertName="Food Handler Certificate",
                CertType="Training", CertBody="Ministry of Health",
                HolderType="Employee", HolderCode="EMP-005",
                HolderName="Vireak Mao", HolderDept="Production",
                IssuingAuthority="Ministry of Health Cambodia", IssuingCountry="KH",
                IssuedDate=today.AddMonths(-4), ExpiryDate=today.AddMonths(8),
                RenewalDaysAlert=45, Status="Active",
                LinkedModule="HR", LinkedDocType="Employee",
                Tags="food,handler,training",
            },

            // ── Environmental ───────────────────────────────
            new()
            {
                CertNo="CRT-ENV-2025-0001", CertName="Environmental Impact Assessment",
                CertType="Environmental", CertBody="Ministry of Environment",
                HolderType="Company", HolderName="Farming Corp Co., Ltd.",
                IssuingAuthority="Ministry of Environment and Natural Resources", IssuingCountry="KH",
                IssuedDate=today.AddYears(-3), ExpiryDate=today.AddYears(2),
                RenewalDaysAlert=120, Status="Active",
                Tags="environment,eia,assessment",
            },

            // ── Vendor ──────────────────────────────────────
            new()
            {
                CertNo="CRT-VND-2025-0001", CertName="Approved Vendor — Agri Supplies Ltd",
                CertType="Vendor", CertBody="Internal Procurement",
                HolderType="Vendor", HolderCode="V-001",
                HolderName="Agri Supplies Ltd.",
                IssuingAuthority="Farming Corp — Procurement Dept.", IssuingCountry="KH",
                IssuedDate=today.AddYears(-1), ExpiryDate=today.AddYears(1),
                RenewalDaysAlert=60, Status="Active",
                LinkedModule="BusinessPartner", LinkedDocType="BusinessPartner",
                Tags="vendor,approved,supplier",
                Attributes=new List<CertificateAttribute>{
                    new(){ AttrKey="VendorScore", AttrLabel="Vendor Score",    DataType="Number",AttrValue="92", SortOrder=1 },
                    new(){ AttrKey="Tier",        AttrLabel="Vendor Tier",     DataType="Text",  AttrValue="Gold",SortOrder=2 },
                }
            },
        };
    }
}
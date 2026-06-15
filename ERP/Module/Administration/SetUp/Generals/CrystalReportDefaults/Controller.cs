using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Administration.CrystalReportDefaults;

[ApiController]
[Route("[controller]")]
public class CrystalReportDefaultController : ControllerBase
{
    private readonly ICrystalReportDefaultRepository _repo;
    private readonly IMapper                         _mapper;

    public CrystalReportDefaultController(
        ICrystalReportDefaultRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /CrystalReportDefault ──────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? category     = null,
        [FromQuery] string? documentType = null,
        [FromQuery] string? language     = null)
    {
        var list = _repo.GetAll()
            .Where(x => category     == null || x.Category     == category)
            .Where(x => documentType == null || x.DocumentType == documentType || x.DocumentType == "All")
            .Where(x => language     == null || x.Language     == language)
            .OrderBy(x => x.Category)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.ElementName)
            .ToList();
        return Ok(_mapper.Map<List<CrystalReportDefaultResponse>>(list));
    }

    // ── GET /CrystalReportDefault/{id} ────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<CrystalReportDefaultResponse>(e));
    }

    // ── GET /CrystalReportDefault/ForDocument/{docType}?lang=en
    // Returns merged set: All-scope defaults overridden by doc-specific ones
    [AllowAnonymous][HttpGet("ForDocument/{docType}")]
    public IActionResult ForDocument(string docType, [FromQuery] string lang = "en")
    {
        var globals  = _repo.GetAll()
            .Where(x => x.DocumentType == "All" && x.Language == lang && x.IsActive)
            .ToList();
        var specific = _repo.GetAll()
            .Where(x => x.DocumentType == docType && x.Language == lang && x.IsActive)
            .ToList();

        // Merge: specific overrides global by ElementKey
        var merged = globals.ToDictionary(g => g.ElementKey, g => g);
        foreach (var s in specific)
            merged[s.ElementKey] = s;

        var result = merged.Values
            .OrderBy(x => x.Category)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ToList();

        return Ok(_mapper.Map<List<CrystalReportDefaultResponse>>(result));
    }

    // ── GET /CrystalReportDefault/ByKey/{key} ─────────────────
    [AllowAnonymous][HttpGet("ByKey/{key}")]
    public IActionResult GetByKey(
        string key,
        [FromQuery] string docType = "All",
        [FromQuery] string lang    = "en")
    {
        // Try specific first, fall back to global
        var e = _repo.GetSingle(x =>
                x.ElementKey == key && x.DocumentType == docType && x.Language == lang)
            ?? _repo.GetSingle(x =>
                x.ElementKey == key && x.DocumentType == "All"   && x.Language == lang);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<CrystalReportDefaultResponse>(e));
    }

    // ── GET /CrystalReportDefault/Categories ──────────────────
    [AllowAnonymous][HttpGet("Categories")]
    public IActionResult GetCategories()
    {
        var cats = _repo.GetAll().Select(x => x.Category)
            .Distinct().OrderBy(x => x).ToList();
        return Ok(cats);
    }

    // ── POST /CrystalReportDefault ─────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] CrystalReportDefaultRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var key = dto.ElementKey.Trim().ToUpper();
        if (_repo.GetAll().Any(x =>
            x.ElementKey == key && x.DocumentType == dto.DocumentType && x.Language == dto.Language))
            return BadRequest($"Element '{key}' already exists for {dto.DocumentType}/{dto.Language}");

        var entity            = _mapper.Map<CrystalReportDefault>(dto);
        entity.ElementKey     = key;
        entity.CreatedAt      = DateTime.UtcNow;
        entity.InActive       = false;
        entity.IsSystemElement= false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<CrystalReportDefaultResponse>(entity));
    }

    // ── PUT /CrystalReportDefault/{id} ────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CrystalReportDefaultRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var key = dto.ElementKey.Trim().ToUpper();
        if (_repo.GetAll().Any(x =>
            x.ElementKey == key && x.DocumentType == dto.DocumentType &&
            x.Language == dto.Language && x.Id != id))
            return BadRequest($"Duplicate key '{key}' for {dto.DocumentType}/{dto.Language}");

        _mapper.Map(dto, entity);
        entity.ElementKey = key;
        entity.UpdatedAt  = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<CrystalReportDefaultResponse>(entity));
    }

    // ── PUT /CrystalReportDefault/BulkSave ────────────────────
    [AllowAnonymous][HttpPut("BulkSave")]
    public IActionResult BulkSave([FromBody] List<ElementValueItem> items)
    {
        int updated = 0;
        foreach (var item in items)
        {
            var entity = _repo.GetSingle(x => x.Id == item.Id);
            if (entity == null) continue;
            entity.Value     = item.Value;
            entity.UpdatedAt = DateTime.UtcNow;
            _repo.Update(entity);
            updated++;
        }
        _repo.Commit();
        return Ok(new { updated });
    }

    // ── POST /CrystalReportDefault/{id}/Reset ─────────────────
    [AllowAnonymous][HttpPost("{id:int}/Reset")]
    public IActionResult Reset(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        var old         = entity.Value;
        entity.Value    = entity.DefaultValue;
        entity.UpdatedAt= DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new ResetResult
        {
            ElementKey    = entity.ElementKey,
            OldValue      = old,
            RestoredValue = entity.DefaultValue,
            Message       = $"'{entity.ElementName}' reset to default",
        });
    }

    // ── POST /CrystalReportDefault/ResetAll ───────────────────
    [AllowAnonymous][HttpPost("ResetAll")]
    public IActionResult ResetAll([FromQuery] string? category = null)
    {
        var all = _repo.GetAll()
            .Where(x => category == null || x.Category == category)
            .ToList();
        foreach (var e in all)
        {
            e.Value     = e.DefaultValue;
            e.UpdatedAt = DateTime.UtcNow;
            _repo.Update(e);
        }
        _repo.Commit();
        return Ok(new { message = $"{all.Count} elements reset to defaults" });
    }

    // ── DELETE /CrystalReportDefault/{id} ─────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsSystemElement)
            return BadRequest("System elements cannot be deleted. Deactivate them instead.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /CrystalReportDefault/Seed ───────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Crystal Report defaults already seeded.");
        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} default elements created" });
    }

    // ─── Seed data ────────────────────────────────────────────
    private static List<CrystalReportDefault> GetSeedData() => new()
    {
        // ── Company Information ──────────────────────────────
        new(){ ElementKey="COMPANY_NAME",        ElementName="Company Legal Name",          ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="Farming Corp Co., Ltd.",          DefaultValue="Farming Corp Co., Ltd.",          IsSystemElement=true,  SortOrder=1,  HelpText="Full registered company name",     PreviewHint="Appears in report header" },
        new(){ ElementKey="COMPANY_NAME_KH",     ElementName="Company Name (Khmer)",        ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Language="km", Value="ក្រុមហ៊ុន ហ្វាមីង ខបស.",     DefaultValue="ក្រុមហ៊ុន ហ្វាមីង ខបស.",     IsSystemElement=true,  SortOrder=2  },
        new(){ ElementKey="COMPANY_ADDRESS",     ElementName="Company Address",             ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="No. 12, Street 271, Phnom Penh, Cambodia", DefaultValue="",       IsSystemElement=true,  SortOrder=3  },
        new(){ ElementKey="COMPANY_PHONE",       ElementName="Company Phone",               ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="+855 23 000 000",                 DefaultValue="",                                IsSystemElement=true,  SortOrder=4  },
        new(){ ElementKey="COMPANY_EMAIL",       ElementName="Company Email",               ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="info@farmingcorp.com.kh",         DefaultValue="",                                IsSystemElement=false, SortOrder=5  },
        new(){ ElementKey="COMPANY_WEBSITE",     ElementName="Company Website",             ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="www.farmingcorp.com.kh",          DefaultValue="",                                IsSystemElement=false, SortOrder=6  },
        new(){ ElementKey="COMPANY_TIN",         ElementName="Tax Identification Number",   ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="K001-1234567",                    DefaultValue="",                                IsSystemElement=true,  SortOrder=7,  HelpText="VAT/TIN printed on invoices" },
        new(){ ElementKey="COMPANY_REG_NO",      ElementName="Company Registration No.",   ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="Text",    Value="Co. 12345/2019",                  DefaultValue="",                                IsSystemElement=false, SortOrder=8  },
        new(){ ElementKey="COMPANY_BANK",        ElementName="Bank Account Details",        ElementType="CompanyInfo", Category="Company",  DocumentType="All",DataType="HTML",    Value="ABA Bank | Acc: 123-456-789 | USD",DefaultValue="",                               IsSystemElement=false, SortOrder=9,  HelpText="Shown on invoices and POs" },

        // ── Branding ──────────────────────────────────────────
        new(){ ElementKey="LOGO_URL",            ElementName="Company Logo URL",            ElementType="Logo",        Category="Branding", DocumentType="All",DataType="ImageUrl",Value="/assets/logo.png",                DefaultValue="/assets/logo.png",                IsSystemElement=true,  SortOrder=10, HelpText="Report header logo image", PreviewHint="Top-left of every report" },
        new(){ ElementKey="LOGO_WIDTH",          ElementName="Logo Width (mm)",             ElementType="Logo",        Category="Branding", DocumentType="All",DataType="Number",  Value="40",                              DefaultValue="40",                              IsSystemElement=false, SortOrder=11 },
        new(){ ElementKey="LOGO_HEIGHT",         ElementName="Logo Height (mm)",            ElementType="Logo",        Category="Branding", DocumentType="All",DataType="Number",  Value="20",                              DefaultValue="20",                              IsSystemElement=false, SortOrder=12 },
        new(){ ElementKey="PRIMARY_COLOR",       ElementName="Primary Brand Colour",        ElementType="Color",       Category="Branding", DocumentType="All",DataType="Color",   Value="#1e40af",                         DefaultValue="#1e40af",                         IsSystemElement=false, SortOrder=13, PreviewHint="Report heading and accent colour" },
        new(){ ElementKey="SECONDARY_COLOR",     ElementName="Secondary Brand Colour",      ElementType="Color",       Category="Branding", DocumentType="All",DataType="Color",   Value="#374151",                         DefaultValue="#374151",                         IsSystemElement=false, SortOrder=14 },
        new(){ ElementKey="HEADING_FONT",        ElementName="Heading Font",                ElementType="Font",        Category="Branding", DocumentType="All",DataType="Text",    Value="Arial",                           DefaultValue="Arial",                           IsSystemElement=false, SortOrder=15, HelpText="Font used in section headings" },
        new(){ ElementKey="BODY_FONT",           ElementName="Body Font",                   ElementType="Font",        Category="Branding", DocumentType="All",DataType="Text",    Value="Arial",                           DefaultValue="Arial",                           IsSystemElement=false, SortOrder=16 },
        new(){ ElementKey="FONT_SIZE_BODY",      ElementName="Body Font Size (pt)",         ElementType="Font",        Category="Branding", DocumentType="All",DataType="Number",  Value="9",                               DefaultValue="9",                               IsSystemElement=false, SortOrder=17 },

        // ── Page Layout ───────────────────────────────────────
        new(){ ElementKey="PAGE_HEADER_TEXT",    ElementName="Page Header Text",            ElementType="Header",      Category="Page",     DocumentType="All",DataType="Text",    Value="",                                DefaultValue="",                                IsSystemElement=false, SortOrder=20, HelpText="Appears at top of each printed page" },
        new(){ ElementKey="PAGE_FOOTER_TEXT",    ElementName="Page Footer Text",            ElementType="Footer",      Category="Page",     DocumentType="All",DataType="Text",    Value="This document is system generated.", DefaultValue="This document is system generated.", IsSystemElement=false, SortOrder=21 },
        new(){ ElementKey="PAGE_NUMBER_FORMAT",  ElementName="Page Number Format",          ElementType="PageNumber",  Category="Page",     DocumentType="All",DataType="Text",    Value="Page {0} of {1}",                 DefaultValue="Page {0} of {1}",                 IsSystemElement=false, SortOrder=22 },
        new(){ ElementKey="SHOW_PAGE_NUMBERS",   ElementName="Show Page Numbers",           ElementType="PageNumber",  Category="Page",     DocumentType="All",DataType="Boolean", Value="true",                            DefaultValue="true",                            IsSystemElement=false, SortOrder=23 },
        new(){ ElementKey="SHOW_PRINT_DATE",     ElementName="Show Printed Date/Time",      ElementType="Header",      Category="Page",     DocumentType="All",DataType="Boolean", Value="true",                            DefaultValue="true",                            IsSystemElement=false, SortOrder=24 },

        // ── Signatures ────────────────────────────────────────
        new(){ ElementKey="SIG_PREPARED_BY",     ElementName="Prepared By Label",           ElementType="Signature",   Category="Signature",DocumentType="All",DataType="Text",    Value="Prepared By",                     DefaultValue="Prepared By",                     IsSystemElement=false, SortOrder=30 },
        new(){ ElementKey="SIG_APPROVED_BY",     ElementName="Approved By Label",           ElementType="Signature",   Category="Signature",DocumentType="All",DataType="Text",    Value="Approved By",                     DefaultValue="Approved By",                     IsSystemElement=false, SortOrder=31 },
        new(){ ElementKey="SIG_RECEIVED_BY",     ElementName="Received By Label",           ElementType="Signature",   Category="Signature",DocumentType="All",DataType="Text",    Value="Received By",                     DefaultValue="Received By",                     IsSystemElement=false, SortOrder=32 },
        new(){ ElementKey="SIG_SHOW_DATE_LINE",  ElementName="Show Date Line Under Sig.",  ElementType="Signature",   Category="Signature",DocumentType="All",DataType="Boolean", Value="true",                            DefaultValue="true",                            IsSystemElement=false, SortOrder=33 },
        new(){ ElementKey="SIG_COUNT",           ElementName="Number of Signature Blocks", ElementType="Signature",   Category="Signature",DocumentType="All",DataType="Number",  Value="2",                               DefaultValue="2",                               IsSystemElement=false, SortOrder=34, HelpText="1–4 signature blocks per document" },

        // ── Watermark ─────────────────────────────────────────
        new(){ ElementKey="WATERMARK_ENABLED",   ElementName="Watermark Enabled",           ElementType="Watermark",   Category="Page",     DocumentType="All",DataType="Boolean", Value="false",                           DefaultValue="false",                           IsSystemElement=false, SortOrder=40 },
        new(){ ElementKey="WATERMARK_TEXT",      ElementName="Watermark Text",              ElementType="Watermark",   Category="Page",     DocumentType="All",DataType="Text",    Value="CONFIDENTIAL",                    DefaultValue="CONFIDENTIAL",                    IsSystemElement=false, SortOrder=41, HelpText="Text overlaid diagonally across pages" },
        new(){ ElementKey="WATERMARK_OPACITY",   ElementName="Watermark Opacity (%)",       ElementType="Watermark",   Category="Page",     DocumentType="All",DataType="Number",  Value="15",                              DefaultValue="15",                              IsSystemElement=false, SortOrder=42 },
        new(){ ElementKey="DRAFT_WATERMARK",     ElementName="DRAFT Watermark on Unposted", ElementType="Watermark",   Category="Page",     DocumentType="All",DataType="Boolean", Value="true",                            DefaultValue="true",                            IsSystemElement=false, SortOrder=43, HelpText="Auto-applies DRAFT watermark to unposted documents" },

        // ── Number & Date Format ──────────────────────────────
        new(){ ElementKey="DATE_FORMAT",         ElementName="Date Format",                 ElementType="DateFormat",  Category="Format",   DocumentType="All",DataType="Text",    Value="dd/MM/yyyy",                      DefaultValue="dd/MM/yyyy",                      IsSystemElement=true,  SortOrder=50, HelpText="Format applied to all dates in reports", Placeholder="dd/MM/yyyy" },
        new(){ ElementKey="DATETIME_FORMAT",     ElementName="Date-Time Format",            ElementType="DateFormat",  Category="Format",   DocumentType="All",DataType="Text",    Value="dd/MM/yyyy HH:mm",                DefaultValue="dd/MM/yyyy HH:mm",                IsSystemElement=false, SortOrder=51 },
        new(){ ElementKey="NUMBER_FORMAT",       ElementName="Number Format (JSON)",        ElementType="NumberFormat",Category="Format",   DocumentType="All",DataType="JSON",    Value="{\"decimal\":\".\",\"thousand\":\",\",\"places\":2}",DefaultValue="{\"decimal\":\".\",\"thousand\":\",\",\"places\":2}",IsSystemElement=true,SortOrder=52 },
        new(){ ElementKey="CURRENCY_SYMBOL",     ElementName="Currency Symbol",             ElementType="NumberFormat",Category="Format",   DocumentType="All",DataType="Text",    Value="KHR",                             DefaultValue="KHR",                             IsSystemElement=true,  SortOrder=53 },
        new(){ ElementKey="SHOW_CURRENCY_CODE",  ElementName="Show Currency Code",          ElementType="NumberFormat",Category="Format",   DocumentType="All",DataType="Boolean", Value="true",                            DefaultValue="true",                            IsSystemElement=false, SortOrder=54 },

        // ── Legal ─────────────────────────────────────────────
        new(){ ElementKey="LEGAL_DISCLAIMER",    ElementName="Legal Disclaimer",            ElementType="Legal",       Category="Legal",    DocumentType="All",DataType="Text",    Value="This document is confidential and intended solely for the named recipient.", DefaultValue="", IsSystemElement=false, SortOrder=60, HelpText="Appears at report footer" },
        new(){ ElementKey="INVOICE_TERMS",       ElementName="Invoice Payment Terms",       ElementType="Legal",       Category="Legal",    DocumentType="ARInvoice",DataType="Text",Value="Payment is due within 30 days of invoice date.", DefaultValue="",       IsSystemElement=false, SortOrder=61 },
        new(){ ElementKey="PO_TERMS",            ElementName="Purchase Order Terms",        ElementType="Legal",       Category="Legal",    DocumentType="PurchaseOrder",DataType="Text",Value="All goods are subject to inspection upon receipt.", DefaultValue="",   IsSystemElement=false, SortOrder=62 },
        new(){ ElementKey="QUO_VALIDITY",        ElementName="Quotation Validity Note",     ElementType="Legal",       Category="Legal",    DocumentType="Quotation",DataType="Text",Value="This quotation is valid for 7 days from the date of issue.",DefaultValue="",IsSystemElement=false, SortOrder=63 },
        new(){ ElementKey="COPY_LABEL",          ElementName="Copy Document Label",         ElementType="Legal",       Category="Legal",    DocumentType="All",DataType="Text",    Value="*** COPY ***",                    DefaultValue="*** COPY ***",                    IsSystemElement=false, SortOrder=64, HelpText="Text shown when printing a document copy" },
    };
}
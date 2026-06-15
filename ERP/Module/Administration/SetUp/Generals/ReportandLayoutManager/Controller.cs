using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.ReportLayoutManager;

[ApiController]
[Route("[controller]")]
public class ReportLayoutController : ControllerBase
{
    private readonly IReportLayoutRepository _repo;
    private readonly IMapper                 _mapper;
    private readonly MyDbContext             _db;

    public ReportLayoutController(
        IReportLayoutRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /ReportLayout ──────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? documentType = null,
        [FromQuery] string? module       = null,
        [FromQuery] string? layoutType   = null)
    {
        var list = _repo.GetAll()
            .Include(x => x.Parameters.OrderBy(p => p.ParamOrder))
            .Where(x => documentType == null || x.DocumentType == documentType)
            .Where(x => module       == null || x.Module       == module)
            .Where(x => layoutType   == null || x.LayoutType   == layoutType)
            .OrderBy(x => x.IsDefault ? 0 : 1)
            .ThenBy(x => x.Module)
            .ThenBy(x => x.DocumentType)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<ReportLayoutResponse>>(list));
    }

    // ── GET /ReportLayout/{id} ─────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Parameters.OrderBy(p => p.ParamOrder))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ReportLayoutResponse>(e));
    }

    // ── GET /ReportLayout/Default/{documentType} ──────────────
    [AllowAnonymous][HttpGet("Default/{documentType}")]
    public IActionResult GetDefault(string documentType)
    {
        var e = _repo.GetAll()
            .Include(x => x.Parameters)
            .FirstOrDefault(x => x.DocumentType == documentType && x.IsDefault && x.IsActive)
            ?? _repo.GetAll()
                .Include(x => x.Parameters)
                .FirstOrDefault(x => x.DocumentType == documentType && x.IsActive);
        if (e == null) return NotFound(new { message = $"No layout found for '{documentType}'" });
        return Ok(_mapper.Map<ReportLayoutResponse>(e));
    }

    // ── GET /ReportLayout/ByModule/{module} ───────────────────
    [AllowAnonymous][HttpGet("ByModule/{module}")]
    public IActionResult GetByModule(string module)
    {
        var list = _repo.GetAll()
            .Include(x => x.Parameters)
            .Where(x => x.Module == module && x.IsActive)
            .OrderBy(x => x.IsDefault ? 0 : 1)
            .ThenBy(x => x.DocumentType)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<ReportLayoutResponse>>(list));
    }

    // ── GET /ReportLayout/DocumentTypes ───────────────────────
    [AllowAnonymous][HttpGet("DocumentTypes")]
    public IActionResult GetDocumentTypes()
    {
        var types = _repo.GetAll()
            .Select(x => new { x.DocumentType, x.Module })
            .Distinct()
            .OrderBy(x => x.Module).ThenBy(x => x.DocumentType)
            .ToList();
        return Ok(types);
    }

    // ── POST /ReportLayout ─────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ReportLayoutRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Layout code '{code}' already exists");

        if (dto.IsDefault) UnsetDefault(dto.DocumentType, 0);

        var entity       = _mapper.Map<ReportLayout>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        int order = 1;
        foreach (var p in entity.Parameters)
            p.ParamOrder = p.ParamOrder > 0 ? p.ParamOrder : order++;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ReportLayoutResponse>(entity));
    }

    // ── PUT /ReportLayout/{id} ─────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ReportLayoutRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Parameters)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Layout code '{code}' already exists");

        if (dto.IsDefault) UnsetDefault(dto.DocumentType, id);

        var isSystem = entity.IsSystemLayout;
        _db.RemoveRange(entity.Parameters);
        _mapper.Map(dto, entity);
        entity.Code          = code;
        entity.IsSystemLayout= isSystem;
        entity.UpdatedAt     = DateTime.UtcNow;

        int order = 1;
        foreach (var p in entity.Parameters)
            p.ParamOrder = p.ParamOrder > 0 ? p.ParamOrder : order++;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ReportLayoutResponse>(entity));
    }

    // ── POST /ReportLayout/{id}/SetDefault ────────────────────
    [AllowAnonymous][HttpPost("{id:int}/SetDefault")]
    public IActionResult SetDefault(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        UnsetDefault(entity.DocumentType, id);
        entity.IsDefault = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.Name}' set as default for {entity.DocumentType}" });
    }

    // ── POST /ReportLayout/{id}/Duplicate ─────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Duplicate")]
    public IActionResult Duplicate(int id)
    {
        var source = _repo.GetAll()
            .Include(x => x.Parameters)
            .FirstOrDefault(x => x.Id == id);
        if (source == null) return NotFound();

        var newCode = source.Code + "-COPY";
        int suffix  = 1;
        while (_repo.GetAll().Any(x => x.Code == newCode))
            newCode = source.Code + $"-COPY{++suffix}";

        var copy = new ReportLayout
        {
            Code           = newCode,
            Name           = source.Name + " (Copy)",
            Description    = source.Description,
            LayoutType     = source.LayoutType,
            DocumentType   = source.DocumentType,
            Module         = source.Module,
            Category       = source.Category,
            LayoutEngine   = source.LayoutEngine,
            FilePath       = source.FilePath,
            FileUrl        = source.FileUrl,
            FileName       = source.FileName,
            PaperSize      = source.PaperSize,
            Orientation    = source.Orientation,
            PageWidth      = source.PageWidth,
            PageHeight     = source.PageHeight,
            MarginTop      = source.MarginTop,
            MarginBottom   = source.MarginBottom,
            MarginLeft     = source.MarginLeft,
            MarginRight    = source.MarginRight,
            OutputFormat   = source.OutputFormat,
            AllowFormatChange = source.AllowFormatChange,
            Language       = source.Language,
            Copies         = source.Copies,
            ShowWatermark  = source.ShowWatermark,
            WatermarkText  = source.WatermarkText,
            IsDefault      = false,
            IsSystemLayout = false,
            SortOrder      = source.SortOrder,
            Version        = "1.0",
            IsActive       = true,
            CreatedAt      = DateTime.UtcNow,
            InActive       = false,
            Parameters     = source.Parameters.Select(p => new ReportParameter
            {
                ParamOrder   = p.ParamOrder,
                ParamKey     = p.ParamKey,
                ParamLabel   = p.ParamLabel,
                DataType     = p.DataType,
                DefaultValue = p.DefaultValue,
                Options      = p.Options,
                IsRequired   = p.IsRequired,
                IsVisible    = p.IsVisible,
            }).ToList(),
        };
        _repo.Add(copy);
        _repo.Commit();
        return Ok(_mapper.Map<ReportLayoutResponse>(copy));
    }

    // ── DELETE /ReportLayout/{id} ──────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsSystemLayout)
            return BadRequest("System layouts cannot be deleted. Deactivate them instead.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /ReportLayout/Seed ────────────────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Report layouts already seeded.");

        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} report layouts seeded" });
    }

    // ─── Helpers ──────────────────────────────────────────────
    private void UnsetDefault(string docType, int exceptId)
    {
        var others = _repo.GetAll()
            .Where(x => x.DocumentType == docType && x.IsDefault && x.Id != exceptId)
            .ToList();
        foreach (var d in others) { d.IsDefault = false; _repo.Update(d); }
    }

    private static List<ReportLayout> GetSeedData() => new()
    {
        // ── Sales ────────────────────────────────────────────
        new(){ Code="RPT-ARINV-STD",  Name="AR Invoice — Standard",          LayoutType="PrintLayout",Module="Sales",     DocumentType="ARInvoice",      Category="Financial",   LayoutEngine="Crystal",FileName="ARInvoice_Standard.rpt",  PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1, Parameters=new List<ReportParameter>{ new(){ ParamOrder=1,ParamKey="Copies",ParamLabel="Number of Copies",DataType="Integer",DefaultValue="1",IsRequired=false,IsVisible=true } } },
        new(){ Code="RPT-ARINV-THAI", Name="AR Invoice — Bilingual EN/KH",   LayoutType="PrintLayout",Module="Sales",     DocumentType="ARInvoice",      Category="Financial",   LayoutEngine="Crystal",FileName="ARInvoice_Bilingual.rpt",  PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=false,IsSystemLayout=false,SortOrder=2 },
        new(){ Code="RPT-SO-STD",     Name="Sales Order — Standard",         LayoutType="PrintLayout",Module="Sales",     DocumentType="SalesOrder",     Category="Operational", LayoutEngine="Crystal",FileName="SalesOrder_Standard.rpt",  PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },
        new(){ Code="RPT-QUO-STD",    Name="Quotation — Standard",           LayoutType="PrintLayout",Module="Sales",     DocumentType="Quotation",      Category="Operational", LayoutEngine="Crystal",FileName="Quotation_Standard.rpt",   PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },
        new(){ Code="RPT-CRNOTE-STD", Name="Credit Note — Standard",         LayoutType="PrintLayout",Module="Sales",     DocumentType="CreditNote",     Category="Financial",   LayoutEngine="Crystal",FileName="CreditNote_Standard.rpt",  PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },

        // ── Purchasing ───────────────────────────────────────
        new(){ Code="RPT-PO-STD",     Name="Purchase Order — Standard",      LayoutType="PrintLayout",Module="Purchasing",DocumentType="PurchaseOrder",  Category="Operational", LayoutEngine="Crystal",FileName="PurchaseOrder_Standard.rpt",PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },
        new(){ Code="RPT-GR-STD",     Name="Goods Receipt Note",             LayoutType="PrintLayout",Module="Purchasing",DocumentType="GoodsReceipt",   Category="Operational", LayoutEngine="Crystal",FileName="GoodsReceipt_Standard.rpt", PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },

        // ── Inventory ────────────────────────────────────────
        new(){ Code="RPT-SLBL-STD",   Name="Stock Label — Barcode",          LayoutType="Label",      Module="Inventory", DocumentType="ItemMaster",     Category="Operational", LayoutEngine="Crystal",FileName="StockLabel_Barcode.rpt",    PaperSize="Custom",Orientation="Portrait",PageWidth=70,PageHeight=40,OutputFormat="PDF",IsDefault=true,IsSystemLayout=true,SortOrder=1 },
        new(){ Code="RPT-INVCOUNT",   Name="Inventory Count Sheet",          LayoutType="Report",     Module="Inventory", DocumentType="StockCount",     Category="Operational", LayoutEngine="Crystal",FileName="InventoryCount.rpt",         PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true,IsSystemLayout=true,SortOrder=1,
            Parameters=new List<ReportParameter>{
                new(){ ParamOrder=1,ParamKey="WarehouseCode",ParamLabel="Warehouse",     DataType="String",  IsRequired=true, IsVisible=true },
                new(){ ParamOrder=2,ParamKey="CountDate",    ParamLabel="Count Date",    DataType="Date",    DefaultValue="TODAY",IsRequired=true,IsVisible=true },
            }
        },

        // ── Financial ────────────────────────────────────────
        new(){ Code="RPT-AGED-AR",    Name="Aged Receivables Report",        LayoutType="Report",     Module="Financials",DocumentType="ARReport",       Category="Financial",   LayoutEngine="Crystal",FileName="AgedReceivables.rpt",       PaperSize="A4",Orientation="Landscape",OutputFormat="PDF",IsDefault=true,IsSystemLayout=true,SortOrder=1,
            Parameters=new List<ReportParameter>{
                new(){ ParamOrder=1,ParamKey="AsOfDate",ParamLabel="As Of Date",  DataType="Date",   DefaultValue="TODAY",IsRequired=true,IsVisible=true },
                new(){ ParamOrder=2,ParamKey="Currency", ParamLabel="Currency",   DataType="Select", DefaultValue="KHR",  Options="[\"KHR\",\"USD\",\"EUR\"]",IsRequired=false,IsVisible=true },
            }
        },
        new(){ Code="RPT-TRIAL-BAL",  Name="Trial Balance",                  LayoutType="Report",     Module="Financials",DocumentType="GLReport",       Category="Financial",   LayoutEngine="Crystal",FileName="TrialBalance.rpt",           PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true,IsSystemLayout=true,SortOrder=2,
            Parameters=new List<ReportParameter>{
                new(){ ParamOrder=1,ParamKey="DateFrom",ParamLabel="From Date",DataType="Date",IsRequired=true,IsVisible=true },
                new(){ ParamOrder=2,ParamKey="DateTo",  ParamLabel="To Date",  DataType="Date",IsRequired=true,IsVisible=true },
            }
        },

        // ── Sales Analytics ──────────────────────────────────
        new(){ Code="RPT-SALES-SUM",  Name="Sales Summary by Customer",      LayoutType="Report",     Module="Sales",     DocumentType="SalesReport",    Category="Management",  LayoutEngine="Crystal",FileName="SalesSummary_Customer.rpt",  PaperSize="A4",Orientation="Landscape",OutputFormat="PDF",IsDefault=false,IsSystemLayout=true,SortOrder=1,
            Parameters=new List<ReportParameter>{
                new(){ ParamOrder=1,ParamKey="DateFrom",   ParamLabel="From Date",   DataType="Date",  IsRequired=true, IsVisible=true },
                new(){ ParamOrder=2,ParamKey="DateTo",     ParamLabel="To Date",     DataType="Date",  IsRequired=true, IsVisible=true },
                new(){ ParamOrder=3,ParamKey="SalesPersonId",ParamLabel="Sales Person",DataType="String",IsRequired=false,IsVisible=true },
            }
        },

        // ── HR ───────────────────────────────────────────────
        new(){ Code="RPT-PAYSLIP",    Name="Payslip — Standard",             LayoutType="PrintLayout",Module="HR",        DocumentType="Payslip",        Category="HR",          LayoutEngine="Crystal",FileName="Payslip_Standard.rpt",       PaperSize="A5",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },
        new(){ Code="RPT-LEAVE-CERT", Name="Leave Certificate",              LayoutType="Certificate",Module="HR",        DocumentType="LeaveCertificate",Category="HR",         LayoutEngine="HTML",   FileName="LeaveCertificate.html",      PaperSize="A4",Orientation="Portrait",OutputFormat="PDF",IsDefault=true, IsSystemLayout=true, SortOrder=1 },
        new(){ Code="RPT-ATTEND",     Name="Attendance Report",              LayoutType="Report",     Module="HR",        DocumentType="AttendanceReport",Category="HR",         LayoutEngine="Excel",  FileName="AttendanceReport.xlsx",      PaperSize="A4",Orientation="Landscape",OutputFormat="Excel",IsDefault=true,IsSystemLayout=true,SortOrder=1,
            Parameters=new List<ReportParameter>{
                new(){ ParamOrder=1,ParamKey="Month",     ParamLabel="Month",      DataType="Select", Options="[\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\",\"8\",\"9\",\"10\",\"11\",\"12\"]",IsRequired=true,IsVisible=true },
                new(){ ParamOrder=2,ParamKey="Year",      ParamLabel="Year",       DataType="Integer",DefaultValue="2025",IsRequired=true,IsVisible=true },
                new(){ ParamOrder=3,ParamKey="Department",ParamLabel="Department", DataType="String", IsRequired=false,IsVisible=true },
            }
        },
    };
}
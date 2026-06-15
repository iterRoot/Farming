using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FarmingApi.Modules.Administration.ReferenceFieldLink;

[ApiController]
[Route("[controller]")]
public class ReferenceFieldLinkController : ControllerBase
{
    private readonly IReferenceFieldLinkRepository _repo;
    private readonly IMapper                       _mapper;
    private readonly MyDbContext                   _db;

    public ReferenceFieldLinkController(
        IReferenceFieldLinkRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /ReferenceFieldLink ────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll([FromQuery] string? sourceModule = null)
    {
        var list = _repo.GetAll()
            .Where(x => sourceModule == null || x.SourceModule == sourceModule)
            .OrderBy(x => x.SourceModule)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<ReferenceFieldLinkResponse>>(list));
    }

    // ── GET /ReferenceFieldLink/{id} ───────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ReferenceFieldLinkResponse>(e));
    }

    // ── GET /ReferenceFieldLink/ByField?module=ARInvoice&field=CustomerId
    [AllowAnonymous][HttpGet("ByField")]
    public IActionResult GetByField(
        [FromQuery] string module, [FromQuery] string field)
    {
        var e = _repo.GetSingle(x =>
            x.SourceModule == module && x.SourceField == field);
        if (e == null) return NotFound(new { message = $"No link defined for {module}.{field}" });
        return Ok(_mapper.Map<ReferenceFieldLinkResponse>(e));
    }

    // ── GET /ReferenceFieldLink/ByModule/{module} ──────────────
    [AllowAnonymous][HttpGet("ByModule/{module}")]
    public IActionResult GetByModule(string module)
    {
        var list = _repo.GetAll()
            .Where(x => x.SourceModule == module && x.IsActive)
            .OrderBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.SourceField)
            .ToList();
        return Ok(_mapper.Map<List<ReferenceFieldLinkResponse>>(list));
    }

    // ── GET /ReferenceFieldLink/Modules ───────────────────────
    [AllowAnonymous][HttpGet("Modules")]
    public IActionResult GetModules()
    {
        var modules = _repo.GetAll()
            .Select(x => x.SourceModule)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        return Ok(modules);
    }

    // ── POST /ReferenceFieldLink ───────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ReferenceFieldLinkRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Code '{code}' already exists");
        if (_repo.GetAll().Any(x =>
            x.SourceModule == dto.SourceModule && x.SourceField == dto.SourceField))
            return BadRequest($"A link for {dto.SourceModule}.{dto.SourceField} already exists");

        var entity       = _mapper.Map<ReferenceFieldLink>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ReferenceFieldLinkResponse>(entity));
    }

    // ── PUT /ReferenceFieldLink/{id} ───────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ReferenceFieldLinkRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Code '{code}' already exists");
        if (_repo.GetAll().Any(x =>
            x.SourceModule == dto.SourceModule &&
            x.SourceField  == dto.SourceField  && x.Id != id))
            return BadRequest($"Another link for {dto.SourceModule}.{dto.SourceField} already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ReferenceFieldLinkResponse>(entity));
    }

    // ── DELETE /ReferenceFieldLink/{id} ───────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /ReferenceFieldLink/Seed ──────────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Reference field links already seeded.");

        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} reference field links created" });
    }

    // ─── Seed factory data ────────────────────────────────────
    private static List<ReferenceFieldLink> GetSeedData() => new()
    {
        // AR Invoice
        new(){ Code="RFL-ARINV-CUST",   Name="AR Invoice → Customer",        SourceModule="ARInvoice",     SourceField="CustomerId",      SourceLabel="Customer",          TargetTable="BusinessPartner",  TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/BusinessPartner?type=Customer", TargetFilter="{\"type\":\"Customer\"}", LinkType="Cascade",  IsRequired=true,  AllowSearch=true, SortOrder=1,  AutoFillFields="[{\"sourceField\":\"customerCode\",\"targetField\":\"Code\"},{\"sourceField\":\"customerName\",\"targetField\":\"Name\"},{\"sourceField\":\"paymentTerms\",\"targetField\":\"PaymentTerms\"}]" },
        new(){ Code="RFL-ARINV-WH",     Name="AR Invoice → Warehouse",       SourceModule="ARInvoice",     SourceField="WarehouseId",     SourceLabel="Warehouse",         TargetTable="Warehouse",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Warehouse",                     LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=2  },
        new(){ Code="RFL-ARINV-SALES",  Name="AR Invoice → Sales Employee",  SourceModule="ARInvoice",     SourceField="SalesEmployeeId", SourceLabel="Sales Employee",    TargetTable="User",             TargetKeyField="Id",         TargetDisplayField="FullName",     TargetApiEndpoint="/User?role=Sales",               LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=3  },
        new(){ Code="RFL-ARINV-TER",    Name="AR Invoice → Territory",       SourceModule="ARInvoice",     SourceField="TerritoryId",     SourceLabel="Territory",         TargetTable="Territory",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Territory",                     LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=4  },

        // Sales Order
        new(){ Code="RFL-SO-CUST",      Name="Sales Order → Customer",       SourceModule="SalesOrder",    SourceField="CustomerId",      SourceLabel="Customer",          TargetTable="BusinessPartner",  TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/BusinessPartner?type=Customer", TargetFilter="{\"type\":\"Customer\"}", LinkType="Cascade",  IsRequired=true,  AllowSearch=true, SortOrder=1  },
        new(){ Code="RFL-SO-PRICELIST", Name="Sales Order → Price List",     SourceModule="SalesOrder",    SourceField="PriceListId",     SourceLabel="Price List",        TargetTable="PriceList",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/PriceList",                     LinkType="Lookup",   IsRequired=false, AllowSearch=false,SortOrder=2  },
        new(){ Code="RFL-SO-CURRENCY",  Name="Sales Order → Currency",       SourceModule="SalesOrder",    SourceField="Currency",        SourceLabel="Currency",          TargetTable="ExchangeRate",     TargetKeyField="Currency",   TargetDisplayField="CurrencyName", TargetApiEndpoint="/ExchangeRate/Latest",           LinkType="Lookup",   IsRequired=true,  AllowSearch=false,SortOrder=3  },

        // Purchase Order
        new(){ Code="RFL-PO-VENDOR",    Name="Purchase Order → Vendor",      SourceModule="PurchaseOrder", SourceField="VendorId",        SourceLabel="Vendor",            TargetTable="BusinessPartner",  TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/BusinessPartner?type=Vendor",   TargetFilter="{\"type\":\"Vendor\"}",  LinkType="Cascade",  IsRequired=true,  AllowSearch=true, SortOrder=1,  AutoFillFields="[{\"sourceField\":\"vendorCode\",\"targetField\":\"Code\"},{\"sourceField\":\"paymentTerms\",\"targetField\":\"PaymentTerms\"}]" },
        new(){ Code="RFL-PO-WH",        Name="Purchase Order → Warehouse",   SourceModule="PurchaseOrder", SourceField="WarehouseId",     SourceLabel="Warehouse",         TargetTable="Warehouse",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Warehouse",                     LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=2  },
        new(){ Code="RFL-PO-BUYER",     Name="Purchase Order → Buyer",       SourceModule="PurchaseOrder", SourceField="BuyerId",         SourceLabel="Buyer",             TargetTable="User",             TargetKeyField="Id",         TargetDisplayField="FullName",     TargetApiEndpoint="/User?role=Purchasing",          LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=3  },

        // Goods Receipt
        new(){ Code="RFL-GR-WH",        Name="Goods Receipt → Warehouse",    SourceModule="GoodsReceipt",  SourceField="WarehouseId",     SourceLabel="Warehouse",         TargetTable="Warehouse",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Warehouse",                     LinkType="Lookup",   IsRequired=true,  AllowSearch=true, SortOrder=1  },
        new(){ Code="RFL-GR-LOC",       Name="Goods Receipt → Location",     SourceModule="GoodsReceipt",  SourceField="LocationId",      SourceLabel="Bin Location",      TargetTable="Location",         TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Location",                      LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=2  },

        // Item Master
        new(){ Code="RFL-ITEM-GRP",     Name="Item → Item Group",            SourceModule="ItemMaster",    SourceField="ItemGroupId",     SourceLabel="Item Group",        TargetTable="ItemGroup",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/ItemGroup",                     LinkType="Lookup",   IsRequired=true,  AllowSearch=true, SortOrder=1  },
        new(){ Code="RFL-ITEM-UOM",     Name="Item → Unit of Measure",       SourceModule="ItemMaster",    SourceField="UomId",           SourceLabel="Unit of Measure",   TargetTable="UnitOfMeasure",    TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/UnitOfMeasure",                 LinkType="Lookup",   IsRequired=true,  AllowSearch=true, SortOrder=2  },
        new(){ Code="RFL-ITEM-MFR",     Name="Item → Manufacturer",          SourceModule="ItemMaster",    SourceField="ManufacturerId",  SourceLabel="Manufacturer",      TargetTable="Manufacturer",     TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Manufacturer",                  LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=3  },
        new(){ Code="RFL-ITEM-WH",      Name="Item → Default Warehouse",     SourceModule="ItemMaster",    SourceField="DefaultWarehouse",SourceLabel="Default Warehouse", TargetTable="Warehouse",        TargetKeyField="Code",       TargetDisplayField="Name",         TargetApiEndpoint="/Warehouse",                     LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=4  },

        // Business Partner
        new(){ Code="RFL-BP-TER",       Name="BP → Territory",               SourceModule="BusinessPartner",SourceField="TerritoryId",    SourceLabel="Territory",         TargetTable="Territory",        TargetKeyField="Id",         TargetDisplayField="Name",         TargetApiEndpoint="/Territory",                     LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=1  },
        new(){ Code="RFL-BP-GRPCODE",   Name="BP → BP Group",                SourceModule="BusinessPartner",SourceField="GroupCode",      SourceLabel="BP Group",          TargetTable="BPGroup",          TargetKeyField="Code",       TargetDisplayField="Name",         TargetApiEndpoint="/BPGroup",                       LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=2  },
        new(){ Code="RFL-BP-CURRENCY",  Name="BP → Default Currency",        SourceModule="BusinessPartner",SourceField="Currency",       SourceLabel="Currency",          TargetTable="ExchangeRate",     TargetKeyField="Currency",   TargetDisplayField="CurrencyName", TargetApiEndpoint="/ExchangeRate/Latest",           LinkType="Lookup",   IsRequired=false, AllowSearch=false,SortOrder=3  },

        // Journal Entry
        new(){ Code="RFL-JE-ACCOUNT",   Name="Journal Entry → GL Account",   SourceModule="JournalEntry",  SourceField="AccountCode",     SourceLabel="GL Account",        TargetTable="GLAccount",        TargetKeyField="Code",       TargetDisplayField="Name",         TargetApiEndpoint="/GLAccount",                     LinkType="Validate", IsRequired=true,  AllowSearch=true, SortOrder=1  },
        new(){ Code="RFL-JE-PROJ",      Name="Journal Entry → Project",      SourceModule="JournalEntry",  SourceField="ProjectCode",     SourceLabel="Project",           TargetTable="Project",          TargetKeyField="Code",       TargetDisplayField="Name",         TargetApiEndpoint="/Project",                       LinkType="Lookup",   IsRequired=false, AllowSearch=true, SortOrder=2  },
    };
}
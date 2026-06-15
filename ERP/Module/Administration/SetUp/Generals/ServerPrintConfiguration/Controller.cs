using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Sockets;

namespace FarmingApi.Modules.Administration.ServerPrintConfig;

[ApiController]
[Route("[controller]")]
public class ServerPrintConfigController : ControllerBase
{
    private readonly IServerPrintConfigRepository _repo;
    private readonly IMapper                      _mapper;
    private readonly MyDbContext                  _db;

    public ServerPrintConfigController(
        IServerPrintConfigRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /ServerPrintConfig ─────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll([FromQuery] string? printerType = null)
    {
        var list = _repo.GetAll()
            .Include(x => x.Routes.OrderBy(r => r.SortOrder))
            .Where(x => printerType == null || x.PrinterType == printerType)
            .OrderBy(x => x.IsDefault ? 0 : 1)
            .ThenBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.PrinterName)
            .ToList();
        return Ok(_mapper.Map<List<ServerPrintConfigResponse>>(list));
    }

    // ── GET /ServerPrintConfig/{id} ────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Routes)
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ServerPrintConfigResponse>(e));
    }

    // ── GET /ServerPrintConfig/Default ────────────────────────
    [AllowAnonymous][HttpGet("Default")]
    public IActionResult GetDefault()
    {
        var e = _repo.GetAll()
            .Include(x => x.Routes)
            .FirstOrDefault(x => x.IsDefault && x.IsActive)
            ?? _repo.GetAll().Include(x => x.Routes).FirstOrDefault(x => x.IsActive);
        if (e == null) return NotFound(new { message = "No active printer configured" });
        return Ok(_mapper.Map<ServerPrintConfigResponse>(e));
    }

    // ── GET /ServerPrintConfig/ForDocument/{docType} ──────────
    // Returns the best printer for a given document type
    // Priority: route-specific → default printer → first active
    [AllowAnonymous][HttpGet("ForDocument/{docType}")]
    public IActionResult ForDocument(string docType, [FromQuery] string? userCode = null)
    {
        // Find printer with a matching route for this document
        var routed = _repo.GetAll()
            .Include(x => x.Routes)
            .Where(x => x.IsActive)
            .ToList()
            .FirstOrDefault(x => x.Routes.Any(r =>
                r.DocumentType == docType && r.IsEnabled &&
                (r.UserCode == null || r.UserCode == userCode)));

        var printer = routed
            ?? _repo.GetAll().Include(x => x.Routes).FirstOrDefault(x => x.IsDefault && x.IsActive)
            ?? _repo.GetAll().Include(x => x.Routes).FirstOrDefault(x => x.IsActive);

        if (printer == null)
            return NotFound(new { message = $"No printer configured for '{docType}'" });

        var route = printer.Routes.FirstOrDefault(r =>
            r.DocumentType == docType &&
            (r.UserCode == null || r.UserCode == userCode));

        return Ok(new
        {
            printer  = _mapper.Map<ServerPrintConfigResponse>(printer),
            copies   = route?.Copies ?? printer.DefaultCopies,
            paperTray= route?.PaperTray,
            paperSize= route?.PaperSize ?? printer.PaperSize,
        });
    }

    // ── POST /ServerPrintConfig ────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ServerPrintConfigRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var code = dto.PrinterCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.PrinterCode == code))
            return BadRequest($"Printer code '{code}' already exists");

        if (dto.IsDefault) UnsetDefault(0);

        var entity          = _mapper.Map<ServerPrintConfig>(dto);
        entity.PrinterCode  = code;
        entity.CreatedAt    = DateTime.UtcNow;
        entity.InActive     = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ServerPrintConfigResponse>(entity));
    }

    // ── PUT /ServerPrintConfig/{id} ────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ServerPrintConfigRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Routes)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.PrinterCode.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.PrinterCode == code && x.Id != id))
            return BadRequest($"Printer code '{code}' already exists");

        if (dto.IsDefault) UnsetDefault(id);

        _db.RemoveRange(entity.Routes);
        _mapper.Map(dto, entity);
        entity.PrinterCode = code;
        entity.UpdatedAt   = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ServerPrintConfigResponse>(entity));
    }

    // ── POST /ServerPrintConfig/{id}/SetDefault ────────────────
    [AllowAnonymous][HttpPost("{id:int}/SetDefault")]
    public IActionResult SetDefault(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        UnsetDefault(id);
        entity.IsDefault = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.PrinterName}' set as default printer" });
    }

    // ── POST /ServerPrintConfig/{id}/Test ─────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Test")]
    public IActionResult Test(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        // Determine host and port to test
        string? host = entity.IpAddress ?? ParseUncHost(entity.NetworkPath);
        int     port = entity.Port ?? DefaultPort(entity.Protocol);

        if (string.IsNullOrWhiteSpace(host))
            return Ok(new PrinterTestResult { Success = false, Message = "No IP address or network path configured" });

        var sw = Stopwatch.StartNew();
        try
        {
            using var tcp      = new TcpClient();
            var       result   = tcp.BeginConnect(host, port, null, null);
            bool      connected= result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));
            sw.Stop();

            if (connected && tcp.Connected)
            {
                tcp.EndConnect(result);
                return Ok(new PrinterTestResult
                {
                    Success = true,
                    Message = $"Printer reachable at {host}:{port}",
                    PingMs  = (int)sw.ElapsedMilliseconds,
                });
            }
            return Ok(new PrinterTestResult
            {
                Success = false,
                Message = $"Cannot reach printer at {host}:{port} (timeout 5s)",
                PingMs  = (int)sw.ElapsedMilliseconds,
            });
        }
        catch (Exception ex)
        {
            return Ok(new PrinterTestResult
            {
                Success = false,
                Message = "Connection failed",
                Detail  = ex.Message,
                PingMs  = (int)sw.ElapsedMilliseconds,
            });
        }
    }

    // ── DELETE /ServerPrintConfig/{id} ────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsDefault)
            return BadRequest("Cannot delete the default printer. Assign another printer as default first.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /ServerPrintConfig/Seed ──────────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Printer configurations already seeded.");

        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} printers seeded" });
    }

    // ─── Helpers ──────────────────────────────────────────────
    private void UnsetDefault(int exceptId)
    {
        var others = _repo.GetAll().Where(x => x.IsDefault && x.Id != exceptId).ToList();
        foreach (var d in others) { d.IsDefault = false; _repo.Update(d); }
    }

    private static string? ParseUncHost(string? unc)
    {
        if (string.IsNullOrWhiteSpace(unc)) return null;
        var parts = unc.TrimStart('\\').Split('\\');
        return parts.Length > 0 ? parts[0] : null;
    }

    private static int DefaultPort(string protocol) => protocol switch
    {
        "IPP"  => 631,
        "LPD"  => 515,
        "WSD"  => 3911,
        "HTTP" => 80,
        _      => 9100,  // RAW / JetDirect
    };

    private static List<ServerPrintConfig> GetSeedData() => new()
    {
        new()
        {
            PrinterCode   = "PRT-MAIN",
            PrinterName   = "Main Office Laser (HP LaserJet)",
            PrinterType   = "Network",
            Location      = "Main Office",
            IpAddress     = "192.168.1.50",
            Port          = 9100,
            Protocol      = "RAW",
            DriverName    = "HP Universal Print Driver PCL6",
            PaperSize     = "A4",
            Orientation   = "Portrait",
            DefaultCopies = 1,
            ColorMode     = "Mono",
            DuplexMode    = "None",
            CanColor      = false,
            CanDuplex     = true,
            MaxDpi        = 1200,
            PrintLanguage = "PCL",
            IsDefault     = true,
            SortOrder     = 1,
            Routes        = new List<PrintDocumentRoute>
            {
                new(){ DocumentType="ARInvoice",    Copies=2, PaperTray="Tray1",     IsEnabled=true, SortOrder=1 },
                new(){ DocumentType="SalesOrder",   Copies=1, PaperTray="Tray1",     IsEnabled=true, SortOrder=2 },
                new(){ DocumentType="Quotation",    Copies=1, PaperTray="Tray1",     IsEnabled=true, SortOrder=3 },
                new(){ DocumentType="PurchaseOrder",Copies=2, PaperTray="Tray1",     IsEnabled=true, SortOrder=4 },
            }
        },
        new()
        {
            PrinterCode   = "PRT-WAREHOUSE",
            PrinterName   = "Warehouse Printer (Epson)",
            PrinterType   = "Network",
            Location      = "Warehouse",
            IpAddress     = "192.168.1.51",
            Port          = 9100,
            Protocol      = "RAW",
            PaperSize     = "A4",
            Orientation   = "Portrait",
            DefaultCopies = 1,
            ColorMode     = "Mono",
            DuplexMode    = "None",
            MaxDpi        = 600,
            PrintLanguage = "PCL",
            IsDefault     = false,
            SortOrder     = 2,
            Routes        = new List<PrintDocumentRoute>
            {
                new(){ DocumentType="GoodsReceipt", Copies=2, PaperTray="Tray1", IsEnabled=true, SortOrder=1 },
                new(){ DocumentType="GoodsIssue",   Copies=1, PaperTray="Tray1", IsEnabled=true, SortOrder=2 },
                new(){ DocumentType="DeliveryNote", Copies=2, PaperTray="Tray1", IsEnabled=true, SortOrder=3 },
            }
        },
        new()
        {
            PrinterCode   = "PRT-LABEL",
            PrinterName   = "Zebra Label Printer ZT230",
            PrinterType   = "Zebra",
            Location      = "Warehouse",
            IpAddress     = "192.168.1.52",
            Port          = 9100,
            Protocol      = "RAW",
            PaperSize     = "Custom",
            CustomWidth   = 100,
            CustomHeight  = 50,
            Orientation   = "Landscape",
            DefaultCopies = 1,
            ColorMode     = "Mono",
            PrintLanguage = "ZPL",
            MaxDpi        = 300,
            IsDefault     = false,
            SortOrder     = 3,
            Routes        = new List<PrintDocumentRoute>
            {
                new(){ DocumentType="StockLabel",  Copies=1, IsEnabled=true, SortOrder=1 },
                new(){ DocumentType="ShippingLabel",Copies=1, IsEnabled=true, SortOrder=2 },
            }
        },
        new()
        {
            PrinterCode   = "PRT-PDF",
            PrinterName   = "PDF Virtual Printer",
            PrinterType   = "Virtual",
            Location      = "All Workstations",
            PaperSize     = "A4",
            Orientation   = "Portrait",
            DefaultCopies = 1,
            ColorMode     = "Color",
            PrintLanguage = "PDF",
            MaxDpi        = 300,
            IsDefault     = false,
            SortOrder     = 4,
            Routes        = new List<PrintDocumentRoute>
            {
                new(){ DocumentType="All", Copies=1, IsEnabled=true, SortOrder=1 },
            }
        },
        new()
        {
            PrinterCode   = "PRT-HR",
            PrinterName   = "HR Department Printer",
            PrinterType   = "Network",
            Location      = "HR Department",
            IpAddress     = "192.168.1.53",
            Port          = 9100,
            Protocol      = "RAW",
            PaperSize     = "A4",
            DefaultCopies = 1,
            ColorMode     = "Mono",
            DuplexMode    = "LongEdge",
            CanDuplex     = true,
            PrintLanguage = "PCL",
            IsDefault     = false,
            SortOrder     = 5,
            Routes        = new List<PrintDocumentRoute>
            {
                new(){ DocumentType="Payslip",           Copies=1, PaperTray="Tray1", IsEnabled=true, SortOrder=1 },
                new(){ DocumentType="LeaveCertificate",  Copies=1, PaperTray="Tray1", IsEnabled=true, SortOrder=2 },
                new(){ DocumentType="AttendanceReport",  Copies=1, PaperTray="Tray1", IsEnabled=true, SortOrder=3 },
            }
        },
    };
}
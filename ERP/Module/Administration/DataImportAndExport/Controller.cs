using System.Text;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DataImportAndExport;

// ═══════════════════════════════════════════════════════════════════
// DATA IMPORT / EXPORT
// Route base: /DataImportAndExport
// ═══════════════════════════════════════════════════════════════════
public class DataImportAndExportController : MyController
{
    private const long MaxUploadBytes = 20 * 1024 * 1024; // 20 MB

    private readonly IDataTransferService      _service;
    private readonly IDataTransferJobRepository _jobs;

    public DataImportAndExportController(IDataTransferService service,
                                         IDataTransferJobRepository jobs)
    {
        _service = service;
        _jobs    = jobs;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport/Entities
    // What can be moved, and with which columns.
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Entities")]
    public IActionResult Entities() => Ok(_service.GetEntities());

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport/Template/{entityKey}
    // Empty CSV with the right headers + a hint row.
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Template/{entityKey}")]
    public IActionResult Template(string entityKey)
    {
        try
        {
            var bytes = _service.BuildTemplate(entityKey);
            return File(bytes, "text/csv", $"{entityKey}_template.csv");
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // POST /DataImportAndExport/Preview
    // Dry run. Parses, validates and labels each row add/update/error.
    // Writes nothing.
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("Preview")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Preview([FromForm] IFormFile? file,
                                             [FromForm] string entityKey,
                                             [FromForm] string mode = "A")
    {
        var check = ValidateUpload(file, entityKey, mode);
        if (check != null) return check;

        try
        {
            var text = await ReadAsync(file!);
            return Ok(_service.Preview(entityKey, mode, text, file!.FileName));
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // POST /DataImportAndExport/Import
    // Commits. All-or-nothing: one bad row rejects the whole file.
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("Import")]
    [RequestSizeLimit(MaxUploadBytes)]
    public async Task<IActionResult> Import([FromForm] IFormFile? file,
                                            [FromForm] string entityKey,
                                            [FromForm] string mode = "A")
    {
        var check = ValidateUpload(file, entityKey, mode);
        if (check != null) return check;

        try
        {
            var text   = await ReadAsync(file!);
            var result = _service.Commit(entityKey, mode, text, file!.FileName);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport/Export/{entityKey}
    // Downloads the catalog columns — the output re-imports cleanly.
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Export/{entityKey}")]
    public IActionResult Export(string entityKey)
    {
        try
        {
            var (name, content) = _service.Export(entityKey);
            return File(content, "text/csv", name);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport/Tables
    // Every table in the EF model — for the raw export below.
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Tables")]
    public IActionResult Tables() => Ok(_service.GetExportableTables());

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport/Export/Table/{tableName}
    // Generic dump of every scalar column of any table.
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Export/Table/{tableName}")]
    public IActionResult ExportTable(string tableName)
    {
        try
        {
            var (name, content) = _service.ExportTable(tableName);
            return File(content, "text/csv", name);
        }
        catch (InvalidOperationException ex)
        {
            return Error(ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport  — job history
    // ═══════════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Gets([FromQuery] string? direction = null,
                              [FromQuery] string? entityKey = null,
                              [FromQuery] int take = 100)
    {
        var query = _jobs.GetAll();

        if (!string.IsNullOrWhiteSpace(direction))
            query = query.Where(j => j.Direction == direction);

        if (!string.IsNullOrWhiteSpace(entityKey))
            query = query.Where(j => j.EntityKey == entityKey);

        var list = query.OrderByDescending(j => j.Id)
                        .Take(Math.Clamp(take, 1, 500))
                        .ToList()
                        .Select(Map)
                        .ToList();

        return Ok(list);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET /DataImportAndExport/{id}
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var job = _jobs.GetSingle(j => j.Id == id);
        if (job == null) return ItemNotFound();
        return Ok(Map(job));
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE /DataImportAndExport/{id}  — remove a history row
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var job = _jobs.GetSingle(j => j.Id == id);
        if (job == null) return ItemNotFound();

        _jobs.Remove(job);
        _jobs.Commit();
        return Success();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private IActionResult? ValidateUpload(IFormFile? file, string entityKey, string mode)
    {
        if (file == null || file.Length == 0) return Required("File");
        if (string.IsNullOrWhiteSpace(entityKey)) return Required("Entity");

        if (file.Length > MaxUploadBytes)
            return Error($"File is {file.Length / 1024 / 1024} MB — the limit is 20 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".csv" or ".txt"))
            return Error($"'{ext}' is not supported. Save the sheet as CSV and upload that.");

        if (mode is not ("A" or "U" or "AU"))
            return Error("Mode must be A (add), U (update) or AU (add or update).");

        if (TransferCatalog.Find(entityKey) == null)
            return Error($"Unknown entity '{entityKey}'.");

        return null;
    }

    private static async Task<string> ReadAsync(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return await reader.ReadToEndAsync();
    }

    private static TransferJobResponse Map(DataTransferJob j) => new()
    {
        Id          = j.Id,
        JobNo       = j.JobNo,
        Direction   = j.Direction,
        EntityKey   = j.EntityKey,
        EntityLabel = j.EntityLabel,
        FileName    = j.FileName,
        Format      = j.Format,
        FileSize    = j.FileSize,
        Mode        = j.Mode,
        Status      = j.Status,
        TotalRows   = j.TotalRows,
        SuccessRows = j.SuccessRows,
        FailedRows  = j.FailedRows,
        SkippedRows = j.SkippedRows,
        ErrorLog    = j.ErrorLog,
        DurationMs  = j.DurationMs,
        CreatedAt   = j.CreatedAt,
        CompletedAt = j.CompletedAt,
    };
}

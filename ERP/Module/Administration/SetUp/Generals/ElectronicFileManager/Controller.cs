using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.ElectronicFileManager;

[ApiController]
[Route("[controller]")]
public class ElectronicFileController : ControllerBase
{
    private readonly IElectronicFileRepository _repo;
    private readonly IMapper                   _mapper;
    private readonly MyDbContext               _db;

    public ElectronicFileController(
        IElectronicFileRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /ElectronicFile ────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? category      = null,
        [FromQuery] string? linkedModule  = null,
        [FromQuery] string? linkedDocType = null,
        [FromQuery] int?    linkedDocId   = null,
        [FromQuery] string? extension     = null,
        [FromQuery] bool?   isArchived    = null,
        [FromQuery] string? uploadedBy    = null,
        [FromQuery] int     page          = 1,
        [FromQuery] int     pageSize      = 50)
    {
        var query = _repo.GetAll()
            .Where(x => category      == null || x.Category     == category)
            .Where(x => linkedModule  == null || x.LinkedModule == linkedModule)
            .Where(x => linkedDocType == null || x.LinkedDocType== linkedDocType)
            .Where(x => linkedDocId   == null || x.LinkedDocId  == linkedDocId)
            .Where(x => uploadedBy    == null || x.UploadedBy   == uploadedBy)
            .Where(x => extension     == null || x.FileExtension.ToLower() == extension.ToLower())
            .Where(x => isArchived    == null || x.IsArchived   == isArchived)
            .OrderByDescending(x => x.CreatedAt);

        var total = query.Count();
        var list  = query.Skip((page - 1) * pageSize).Take(pageSize)
            .Include(x => x.Versions.OrderByDescending(v => v.VersionNumber))
            .ToList();

        return Ok(new
        {
            total,
            page,
            pageSize,
            pages = (int)Math.Ceiling(total / (double)pageSize),
            data  = _mapper.Map<List<ElectronicFileResponse>>(list),
        });
    }

    // ── GET /ElectronicFile/{id} ───────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ElectronicFileResponse>(e));
    }

    // ── GET /ElectronicFile/ByCode/{code} ─────────────────────
    [AllowAnonymous][HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var e = _repo.GetAll()
            .Include(x => x.Versions)
            .FirstOrDefault(x => x.FileCode == code.Trim().ToUpper());
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ElectronicFileResponse>(e));
    }

    // ── GET /ElectronicFile/ForDocument?module=ARInvoice&docId=5
    [AllowAnonymous][HttpGet("ForDocument")]
    public IActionResult GetForDocument(
        [FromQuery] string linkedModule,
        [FromQuery] int    linkedDocId)
    {
        var list = _repo.GetAll()
            .Where(x => x.LinkedModule == linkedModule && x.LinkedDocId == linkedDocId && !x.IsArchived)
            .OrderBy(x => x.Category).ThenByDescending(x => x.CreatedAt)
            .ToList();
        return Ok(_mapper.Map<List<ElectronicFileResponse>>(list));
    }

    // ── GET /ElectronicFile/Search?q=contract ─────────────────
    [AllowAnonymous][HttpGet("Search")]
    public IActionResult Search([FromQuery] string q, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required");

        var term  = q.ToLower();
        var list  = _repo.GetAll()
            .Where(x => !x.IsArchived && (
                x.OriginalName.ToLower().Contains(term)          ||
                x.FileCode.ToLower().Contains(term)              ||
                (x.Description != null && x.Description.ToLower().Contains(term)) ||
                (x.Tags        != null && x.Tags.ToLower().Contains(term))        ||
                (x.LinkedDocNo != null && x.LinkedDocNo.ToLower().Contains(term))
            ))
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();
        return Ok(_mapper.Map<List<ElectronicFileResponse>>(list));
    }

    // ── GET /ElectronicFile/Summary ───────────────────────────
    [AllowAnonymous][HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        var now = DateTime.UtcNow;
        return Ok(new FileSummary
        {
            TotalFiles       = all.Count,
            TotalSizeBytes   = all.Sum(x => x.FileSizeBytes),
            TotalSizeDisplay = FormatSize(all.Sum(x => x.FileSizeBytes)),
            ActiveFiles      = all.Count(x => !x.IsArchived && x.IsActive),
            ArchivedFiles    = all.Count(x => x.IsArchived),
            ExpiredFiles     = all.Count(x => x.ExpiresAt.HasValue && x.ExpiresAt < now),
            ByCategory       = all.GroupBy(x => x.Category)
                                  .ToDictionary(g => g.Key, g => g.Count()),
            ByExtension      = all.GroupBy(x => x.FileExtension.ToLower())
                                  .ToDictionary(g => g.Key, g => g.Count()),
            SizeByCategory   = all.GroupBy(x => x.Category)
                                  .ToDictionary(g => g.Key, g => g.Sum(x => x.FileSizeBytes)),
        });
    }

    // ── POST /ElectronicFile ───────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ElectronicFileRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var safeName = SanitizeFileName(dto.OriginalName);
        var entity   = _mapper.Map<ElectronicFile>(dto);
        entity.FileCode       = GenerateCode();
        entity.FileName       = safeName;
        entity.FileExtension  = Path.GetExtension(dto.OriginalName).ToLower();
        entity.IsArchived     = false;
        entity.IsActive       = true;
        entity.UploadedAt     = DateTime.UtcNow;
        entity.CurrentVersion = 1;
        entity.CreatedAt      = DateTime.UtcNow;
        entity.InActive       = false;

        // Create version 1
        entity.Versions.Add(new FileVersion
        {
            VersionNumber = 1,
            FileName      = safeName,
            StoragePath   = dto.StoragePath,
            PublicUrl     = dto.PublicUrl,
            FileSizeBytes = dto.FileSizeBytes,
            ChangeNote    = "Initial upload",
            UploadedBy    = dto.UploadedBy,
            UploadedAt    = DateTime.UtcNow,
            IsCurrent     = true,
        });

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicFileResponse>(entity));
    }

    // ── PUT /ElectronicFile/{id} ───────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ElectronicFileRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicFileResponse>(entity));
    }

    // ── POST /ElectronicFile/{id}/NewVersion ──────────────────
    [AllowAnonymous][HttpPost("{id:int}/NewVersion")]
    public IActionResult NewVersion(int id, [FromBody] ElectronicFileRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Versions)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        // Retire all current versions
        foreach (var v in entity.Versions.Where(v => v.IsCurrent))
            v.IsCurrent = false;

        var next = entity.CurrentVersion + 1;
        entity.Versions.Add(new FileVersion
        {
            VersionNumber = next,
            FileName      = SanitizeFileName(dto.OriginalName),
            StoragePath   = dto.StoragePath,
            PublicUrl     = dto.PublicUrl,
            FileSizeBytes = dto.FileSizeBytes,
            ChangeNote    = dto.ChangeNote ?? $"Version {next}",
            UploadedBy    = dto.UploadedBy,
            UploadedAt    = DateTime.UtcNow,
            IsCurrent     = true,
        });

        entity.OriginalName    = dto.OriginalName;
        entity.FileName        = SanitizeFileName(dto.OriginalName);
        entity.StoragePath     = dto.StoragePath;
        entity.PublicUrl       = dto.PublicUrl;
        entity.FileSizeBytes   = dto.FileSizeBytes;
        entity.CurrentVersion  = next;
        entity.UpdatedAt       = DateTime.UtcNow;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicFileResponse>(entity));
    }

    // ── POST /ElectronicFile/{id}/Download ────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Download")]
    public IActionResult RecordDownload(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.DownloadCount  += 1;
        entity.LastDownloadAt  = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { downloadCount = entity.DownloadCount, fileUrl = entity.PublicUrl });
    }

    // ── POST /ElectronicFile/{id}/Archive ─────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Archive")]
    public IActionResult Archive(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsArchived = true;
        entity.UpdatedAt  = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.OriginalName}' archived" });
    }

    // ── POST /ElectronicFile/{id}/Restore ─────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Restore")]
    public IActionResult Restore(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsArchived = false;
        entity.UpdatedAt  = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.OriginalName}' restored" });
    }

    // ── PUT /ElectronicFile/BulkUpdate ─────────────────────────
    [AllowAnonymous][HttpPut("BulkUpdate")]
    public IActionResult BulkUpdate([FromBody] BulkUpdateRequest dto)
    {
        var entities = _repo.GetAll().Where(x => dto.Ids.Contains(x.Id)).ToList();
        foreach (var e in entities)
        {
            if (dto.Category   != null) e.Category   = dto.Category;
            if (dto.Tags       != null) e.Tags        = dto.Tags;
            if (dto.IsArchived != null) e.IsArchived  = dto.IsArchived.Value;
            if (dto.IsPublic   != null) e.IsPublic    = dto.IsPublic.Value;
            e.UpdatedAt = DateTime.UtcNow;
            _repo.Update(e);
        }
        _repo.Commit();
        return Ok(new { updated = entities.Count });
    }

    // ── DELETE /ElectronicFile/{id} ────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── DELETE /ElectronicFile/BulkDelete ─────────────────────
    [AllowAnonymous][HttpDelete("BulkDelete")]
    public IActionResult BulkDelete([FromBody] List<int> ids)
    {
        var entities = _repo.GetAll().Where(x => ids.Contains(x.Id)).ToList();
        foreach (var e in entities) _repo.Remove(e);
        _repo.Commit();
        return Ok(new { deleted = entities.Count });
    }

    // ─── Helpers ──────────────────────────────────────────────
    private string GenerateCode()
    {
        var year  = DateTime.UtcNow.Year;
        var count = _repo.GetAll().Count() + 1;
        return $"FILE-{year}-{count:D5}";
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)                   return $"{bytes} B";
        if (bytes < 1024 * 1024)            return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024)    return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
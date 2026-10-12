using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Attachment;

// ═══════════════════════════════════════════════════════════════════
// ATTACHMENT  (route: /Attachment)
// GET    /Attachment?ownerType=&ownerId=   → files on a record
// POST   /Attachment  (multipart: file, ownerType, ownerId, freeText,
//                      targetPath, attachmentDate)
// PUT    /Attachment/{id}                  → edit date / free text / path
// GET    /Attachment/{id}/File             → the bytes
// DELETE /Attachment/{id}                  → row and file
// 10 MB max; executable and browser-runnable types are refused.
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class AttachmentController : ControllerBase
{
    private readonly IAttachmentRepository _repository;
    private readonly AttachmentStorage     _storage;
    private readonly MyDbContext           _db;

    public AttachmentController(IAttachmentRepository repository, AttachmentStorage storage, MyDbContext db)
    {
        _repository = repository; _storage = storage; _db = db;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string ownerType, [FromQuery] int ownerId)
    {
        if (!AttachmentRules.OwnerTypes.Contains(ownerType)) return BadRequest($"Unknown owner type '{ownerType}'.");
        var rows = _repository.GetAll().Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId)
            .OrderBy(a => a.Id).ToList();
        return Ok(rows.Select(ToResponse));
    }

    [HttpPost]
    [RequestSizeLimit(AttachmentRules.MaxBytes + 1024 * 64)]
    public async Task<IActionResult> Upload([FromForm] AttachmentUploadRequest req)
    {
        if (!AttachmentRules.OwnerTypes.Contains(req.OwnerType)) return BadRequest($"Unknown owner type '{req.OwnerType}'.");
        if (!AttachmentRules.OwnerExists(_db, req.OwnerType, req.OwnerId)) return BadRequest("The record to attach to doesn't exist.");
        if (req.File == null || req.File.Length == 0) return BadRequest("Choose a file to upload.");
        if (req.File.Length > AttachmentRules.MaxBytes) return BadRequest("Files can be at most 10 MB.");

        var fileName = AttachmentRules.CleanName(req.File.FileName);
        var ext = Path.GetExtension(fileName);
        var extError = AttachmentRules.ExtensionError(ext);
        if (extError != null) return BadRequest(extError);

        string hash;
        await using (var s = req.File.OpenReadStream())
            hash = Convert.ToHexString(await SHA256.HashDataAsync(s));

        var storedName = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
        await using (var s = req.File.OpenReadStream())
            await _storage.SaveAsync(storedName, s);

        var entity = new Attachment
        {
            OwnerType = req.OwnerType, OwnerId = req.OwnerId, FileName = fileName, StoredName = storedName,
            MimeType = AttachmentRules.MimeFor(ext), SizeBytes = req.File.Length, FileHash = hash,
            AttachmentDate = (req.AttachmentDate ?? DateTime.UtcNow).Date,
            FreeText = Trim(req.FreeText, 500), TargetPath = Trim(req.TargetPath, 300),
            CreatedAt = DateTime.UtcNow, InActive = false,
        };
        try { _repository.Add(entity); _repository.Commit(); }
        catch { _storage.Delete(storedName); throw; }   // never leave a file with no row
        return Ok(ToResponse(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] AttachmentUpdateRequest req)
    {
        var e = _repository.GetSingle(a => a.Id == id);
        if (e == null) return NotFound();
        e.AttachmentDate = (req.AttachmentDate ?? e.AttachmentDate).Date;
        e.FreeText = Trim(req.FreeText, 500);
        e.TargetPath = Trim(req.TargetPath, 300);
        e.UpdatedAt = DateTime.UtcNow;
        _repository.Commit();
        return Ok(ToResponse(e));
    }

    [HttpGet("{id:int}/File")]
    public IActionResult File(int id)
    {
        var e = _repository.GetSingle(a => a.Id == id);
        if (e == null) return NotFound();
        var path = _storage.PathFor(e.StoredName);
        if (!System.IO.File.Exists(path)) return NotFound("The file is missing from storage.");
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, e.MimeType, e.FileName);   // attachment disposition with the original name
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repository.GetSingle(a => a.Id == id);
        if (e == null) return NotFound();
        _repository.Remove(e);
        _repository.Commit();
        _storage.Delete(e.StoredName);
        return NoContent();
    }

    private static string? Trim(string? s, int max) =>
        string.IsNullOrWhiteSpace(s) ? null : (s.Trim().Length > max ? s.Trim()[..max] : s.Trim());

    private static AttachmentResponse ToResponse(Attachment a) => new()
    {
        Id = a.Id, OwnerType = a.OwnerType, OwnerId = a.OwnerId, FileName = a.FileName, MimeType = a.MimeType,
        SizeBytes = a.SizeBytes, AttachmentDate = a.AttachmentDate, FreeText = a.FreeText, TargetPath = a.TargetPath,
        CanDisplayInline = AttachmentRules.IsInline(a.MimeType),
    };
}

public class AttachmentUploadRequest
{
    public IFormFile? File           { get; set; }
    public string     OwnerType      { get; set; } = null!;
    public int        OwnerId        { get; set; }
    public string?    FreeText       { get; set; }
    public string?    TargetPath     { get; set; }
    public DateTime?  AttachmentDate { get; set; }
}

public class AttachmentUpdateRequest
{
    public string?   FreeText       { get; set; }
    public string?   TargetPath     { get; set; }
    public DateTime? AttachmentDate { get; set; }
}

public class AttachmentResponse
{
    public int      Id               { get; set; }
    public string   OwnerType        { get; set; } = null!;
    public int      OwnerId          { get; set; }
    public string   FileName         { get; set; } = null!;
    public string   MimeType         { get; set; } = null!;
    public long     SizeBytes        { get; set; }
    public DateTime AttachmentDate   { get; set; }
    public string?  FreeText         { get; set; }
    public string?  TargetPath       { get; set; }
    public bool     CanDisplayInline { get; set; }
}

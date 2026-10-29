using Microsoft.EntityFrameworkCore;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Administration.Attachment;

// Where files go, what's allowed, and who can own one. Everything a stored
// name or path can be is decided here — request text never reaches storage.
public static class AttachmentRules
{
    public const long MaxBytes = 10 * 1024 * 1024;

    // Anything a browser could run as a page, or an OS could run as a program.
    private static readonly HashSet<string> Blocked = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".msi", ".scr", ".ps1", ".sh", ".vbs", ".jar",
        ".js", ".mjs", ".html", ".htm", ".xhtml", ".svg", ".xml", ".hta", ".php", ".asp", ".aspx",
    };

    // Types the browser may show inline; everything else is a plain download.
    private static readonly Dictionary<string, string> Inline = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf", [".png"] = "image/png", [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg", [".gif"] = "image/gif", [".txt"] = "text/plain",
    };

    public static string? ExtensionError(string ext) =>
        string.IsNullOrEmpty(ext) ? "The file needs an extension."
        : Blocked.Contains(ext) ? $"Files of type {ext} aren't allowed."
        : null;

    public static string MimeFor(string ext) => Inline.GetValueOrDefault(ext, "application/octet-stream");
    public static bool IsInline(string mime) => Inline.ContainsValue(mime);

    public static string CleanName(string name)
    {
        var n = Path.GetFileName(name ?? "");
        foreach (var c in Path.GetInvalidFileNameChars()) n = n.Replace(c, '_');
        n = n.Trim();
        return n.Length > 150 ? n[^150..] : n;
    }

    // Owner types that can carry attachments, and how to check one exists.
    public static bool OwnerExists(MyDbContext db, string ownerType, int ownerId) => ownerType switch
    {
        "BusinessPartner" => db.Set<BPEntity>().Any(b => b.Id == ownerId),
        _ => false,
    };
    public static readonly string[] OwnerTypes = { "BusinessPartner" };
}

// Where the bytes of an uploaded file live: a row in KSFL (see StoredFile), not the container's
// disk — Render wipes local files on every restart or redeploy, the database survives both.
// SaveAsync / Delete only queue the change on the request's DbContext; the caller's Commit()
// writes it together with the row that points at the file, so the two never drift apart.
// Files saved to disk before this (A/R invoice paths, Storage/attachments) are still read and
// removed from there, so nothing already uploaded locally is orphaned.
public class AttachmentStorage
{
    private readonly string _root;          // Storage/attachments — legacy business partner files
    private readonly string _contentRoot;
    private readonly MyDbContext _db;

    public AttachmentStorage(IWebHostEnvironment env, MyDbContext db)
    {
        _db = db;
        _contentRoot = Path.GetFullPath(env.ContentRootPath);
        _root = Path.Combine(_contentRoot, "Storage", "attachments");
    }

    public async Task SaveAsync(string key, Stream content)
    {
        using var ms = new MemoryStream();
        await content.CopyToAsync(ms);
        _db.Set<StoredFile>().Add(new StoredFile { StoredName = key, Content = ms.ToArray(), CreatedAt = DateTime.UtcNow });
    }

    public async Task<byte[]?> ReadAsync(string key)
    {
        var bytes = await _db.Set<StoredFile>().AsNoTracking()
            .Where(f => f.StoredName == key).Select(f => f.Content).FirstOrDefaultAsync();
        if (bytes != null) return bytes;

        var legacy = LegacyPath(key);
        return legacy != null && File.Exists(legacy) ? await File.ReadAllBytesAsync(legacy) : null;
    }

    public void Delete(string key)
    {
        var id = _db.Set<StoredFile>().Where(f => f.StoredName == key).Select(f => f.Id).FirstOrDefault();
        if (id != 0) _db.Set<StoredFile>().Remove(new StoredFile { Id = id });

        var legacy = LegacyPath(key);
        if (legacy != null && File.Exists(legacy)) File.Delete(legacy);
    }

    // A rooted key is an old A/R invoice path; a plain name is an old business partner file.
    // Both come from our own rows, and anything outside the app folder is ignored.
    private string? LegacyPath(string key)
    {
        var p = Path.IsPathRooted(key) ? Path.GetFullPath(key) : Path.Combine(_root, Path.GetFileName(key));
        return p.StartsWith(_contentRoot, StringComparison.Ordinal) ? p : null;
    }

    // Removes every file (and row) owned by a record — called from the owner's delete action,
    // which commits.
    public void DeleteAllFor(MyDbContext db, string ownerType, int ownerId)
    {
        var rows = db.Set<Attachment>().Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId).ToList();
        foreach (var r in rows) { Delete(r.StoredName); db.Set<Attachment>().Remove(r); }
    }
}

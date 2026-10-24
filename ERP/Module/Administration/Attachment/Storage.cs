using Microsoft.EntityFrameworkCore;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Administration.Attachment;

// Where files go, what's allowed, and who can own one. Everything a stored
// name or path can be is decided here — request text never reaches the disk.
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

public class AttachmentStorage
{
    private readonly string _root;
    public AttachmentStorage(IWebHostEnvironment env) =>
        _root = Path.Combine(env.ContentRootPath, "Storage", "attachments");

    public string PathFor(string storedName)
    {
        // storedName is always a GUID + extension we generated; refuse anything else.
        if (storedName != Path.GetFileName(storedName)) throw new InvalidOperationException("Bad stored name.");
        return Path.Combine(_root, storedName);
    }

    public async Task SaveAsync(string storedName, Stream content)
    {
        Directory.CreateDirectory(_root);
        await using var fs = File.Create(PathFor(storedName));
        await content.CopyToAsync(fs);
    }

    public void Delete(string storedName)
    {
        var p = PathFor(storedName);
        if (File.Exists(p)) File.Delete(p);
    }

    // Removes every file (and row) owned by a record — called from the owner's delete action.
    public void DeleteAllFor(MyDbContext db, string ownerType, int ownerId)
    {
        var rows = db.Set<Attachment>().Where(a => a.OwnerType == ownerType && a.OwnerId == ownerId).ToList();
        foreach (var r in rows) { Delete(r.StoredName); db.Set<Attachment>().Remove(r); }
    }
}

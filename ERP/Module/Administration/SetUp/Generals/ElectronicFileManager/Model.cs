namespace FarmingApi.Modules.Administration.ElectronicFileManager;

// ── Responses ─────────────────────────────────────────────────
public class ElectronicFileResponse
{
    public int      Id              { get; set; }
    public string   FileCode        { get; set; } = null!;
    public string   FileName        { get; set; } = null!;
    public string   OriginalName    { get; set; } = null!;
    public string   FileExtension   { get; set; } = null!;
    public string   MimeType        { get; set; } = null!;
    public long     FileSizeBytes   { get; set; }
    public string   FileSizeDisplay { get; set; } = null!; // "2.4 MB"
    public string?  FileHash        { get; set; }
    public string   StoragePath     { get; set; } = null!;
    public string?  PublicUrl       { get; set; }
    public string   StorageProvider { get; set; } = null!;
    public string   Category        { get; set; } = null!;
    public string?  SubCategory     { get; set; }
    public string?  Tags            { get; set; }
    public string?  Description     { get; set; }
    public string?  LinkedModule    { get; set; }
    public string?  LinkedDocType   { get; set; }
    public int?     LinkedDocId     { get; set; }
    public string?  LinkedDocNo     { get; set; }
    public bool     IsPublic        { get; set; }
    public string?  AllowedRoles    { get; set; }
    public string   UploadedBy      { get; set; } = null!;
    public string?  UploadedByName  { get; set; }
    public DateTime UploadedAt      { get; set; }
    public DateTime? ExpiresAt      { get; set; }
    public bool     IsArchived      { get; set; }
    public int      DownloadCount   { get; set; }
    public DateTime? LastDownloadAt { get; set; }
    public int      CurrentVersion  { get; set; }
    public bool     IsActive        { get; set; }
    public string?  Remarks         { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
    public List<FileVersionResponse> Versions { get; set; } = new();
    // Computed
    public bool     IsExpired       { get; set; }
    public bool     IsImage         { get; set; }
    public string   FileTypeIcon    { get; set; } = null!;
}

public class FileVersionResponse
{
    public int      Id            { get; set; }
    public int      VersionNumber { get; set; }
    public string   FileName      { get; set; } = null!;
    public string?  PublicUrl     { get; set; }
    public long     FileSizeBytes { get; set; }
    public string?  FileHash      { get; set; }
    public string?  ChangeNote    { get; set; }
    public string   UploadedBy    { get; set; } = null!;
    public DateTime UploadedAt    { get; set; }
    public bool     IsCurrent     { get; set; }
}

// ── Requests ──────────────────────────────────────────────────
public class ElectronicFileRequest
{
    public string   OriginalName    { get; set; } = null!;
    public string   FileExtension   { get; set; } = null!;
    public string   MimeType        { get; set; } = null!;
    public long     FileSizeBytes   { get; set; }
    public string   StoragePath     { get; set; } = null!;
    public string?  PublicUrl       { get; set; }
    public string   StorageProvider { get; set; } = "Local";
    public string   Category        { get; set; } = "Custom";
    public string?  SubCategory     { get; set; }
    public string?  Tags            { get; set; }
    public string?  Description     { get; set; }
    public string?  LinkedModule    { get; set; }
    public string?  LinkedDocType   { get; set; }
    public int?     LinkedDocId     { get; set; }
    public string?  LinkedDocNo     { get; set; }
    public bool     IsPublic        { get; set; } = false;
    public string?  AllowedRoles    { get; set; }
    public string   UploadedBy      { get; set; } = null!;
    public string?  UploadedByName  { get; set; }
    public DateTime? ExpiresAt      { get; set; }
    public string?  Remarks         { get; set; }
    // For new-version upload
    public string?  ChangeNote      { get; set; }
}

// Bulk tag/category update
public class BulkUpdateRequest
{
    public List<int> Ids        { get; set; } = new();
    public string?   Category   { get; set; }
    public string?   Tags       { get; set; }
    public bool?     IsArchived { get; set; }
    public bool?     IsPublic   { get; set; }
}

// Statistics
public class FileSummary
{
    public int     TotalFiles       { get; set; }
    public long    TotalSizeBytes   { get; set; }
    public string  TotalSizeDisplay { get; set; } = null!;
    public int     ActiveFiles      { get; set; }
    public int     ArchivedFiles    { get; set; }
    public int     ExpiredFiles     { get; set; }
    public Dictionary<string, int>  ByCategory  { get; set; } = new();
    public Dictionary<string, int>  ByExtension { get; set; } = new();
    public Dictionary<string, long> SizeByCategory { get; set; } = new();
}
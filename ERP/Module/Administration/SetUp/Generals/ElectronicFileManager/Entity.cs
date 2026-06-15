using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.ElectronicFileManager;

// ═══════════════════════════════════════════════════════════════
// ELECTRONIC FILE  (KEFM)
// Centralised file repository for all uploaded attachments
// across every ERP module. Supports versioning, tagging,
// access control and audit trails.
//
// Category:
//   Contract     → signed agreements, NDAs, SLAs
//   Invoice      → scanned paper invoices, PDFs
//   Certificate  → quality certs, origin certs, ISO
//   Photo        → product images, site photos
//   Manual       → technical manuals, spec sheets
//   Report       → exported reports, statements
//   Identity     → KYC, passports, licences
//   Payroll      → payslips, salary confirmations
//   Custom       → any user-defined category
//
// LinkedModule + LinkedDocumentId optionally pin the file
// to a specific ERP record (e.g. an ARInvoice row).
// ═══════════════════════════════════════════════════════════════
public class ElectronicFile : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   FileCode        { get; set; } = null!;  // "FILE-2026-00001"
    public string   FileName        { get; set; } = null!;  // stored name (sanitised)
    public string   OriginalName    { get; set; } = null!;  // name as uploaded
    public string   FileExtension   { get; set; } = null!;  // ".pdf", ".xlsx"
    public string   MimeType        { get; set; } = null!;  // "application/pdf"
    public long     FileSizeBytes   { get; set; }           // raw bytes
    public string?  FileHash        { get; set; }           // SHA-256 integrity check

    // ── Storage ───────────────────────────────────────────────
    public string   StoragePath     { get; set; } = null!;  // server-side path or bucket key
    public string?  PublicUrl       { get; set; }           // CDN / signed URL for download
    public string   StorageProvider { get; set; } = "Local"; // Local/S3/Azure/GCS

    // ── Classification ────────────────────────────────────────
    public string   Category        { get; set; } = "Custom";
    public string?  SubCategory     { get; set; }           // free-text sub-label
    public string?  Tags            { get; set; }           // comma-separated: "contract,2026,kh"
    public string?  Description     { get; set; }

    // ── Linked ERP Record ─────────────────────────────────────
    public string?  LinkedModule    { get; set; }           // "ARInvoice", "BusinessPartner"
    public string?  LinkedDocType   { get; set; }           // "ARInvoice"
    public int?     LinkedDocId     { get; set; }           // FK to source record (no hard FK)
    public string?  LinkedDocNo     { get; set; }           // "INV-2026-00123"

    // ── Access Control ────────────────────────────────────────
    public bool     IsPublic        { get; set; } = false;  // accessible without login
    public string?  AllowedRoles    { get; set; }           // "Admin,Sales" — null = all
    public string   UploadedBy      { get; set; } = null!;  // UserCode
    public string?  UploadedByName  { get; set; }           // display name

    // ── Lifecycle ─────────────────────────────────────────────
    public DateTime UploadedAt      { get; set; }
    public DateTime? ExpiresAt      { get; set; }           // null = never expires
    public bool     IsArchived      { get; set; } = false;
    public int      DownloadCount   { get; set; } = 0;
    public DateTime? LastDownloadAt { get; set; }

    // ── Versioning ────────────────────────────────────────────
    public int      CurrentVersion  { get; set; } = 1;
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }

    // Navigation
    public ICollection<FileVersion> Versions { get; set; }
        = new List<FileVersion>();
}

// ─────────────────────────────────────────────────────────────
// FILE VERSION  (KEFV)
// Immutable version history for each file upload.
// Version 1 is created automatically on first upload;
// subsequent uploads create new version entries.
// ─────────────────────────────────────────────────────────────
public class FileVersion
{
    public int      Id              { get; set; }
    public int      ElectronicFileId{ get; set; }
    public int      VersionNumber   { get; set; }
    public string   FileName        { get; set; } = null!;
    public string   StoragePath     { get; set; } = null!;
    public string?  PublicUrl       { get; set; }
    public long     FileSizeBytes   { get; set; }
    public string?  FileHash        { get; set; }
    public string?  ChangeNote      { get; set; }   // "Updated signature page"
    public string   UploadedBy      { get; set; } = null!;
    public DateTime UploadedAt      { get; set; }
    public bool     IsCurrent       { get; set; } = true;

    public ElectronicFile? ElectronicFile { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class ElectronicFileConfig
    : IEntityTypeConfiguration<ElectronicFile>
{
    public void Configure(EntityTypeBuilder<ElectronicFile> b)
    {
        b.ToTable("KEFM");
        b.HasKey(x => x.Id);

        b.Property(x => x.FileCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        b.Property(x => x.OriginalName).HasMaxLength(300).IsRequired();
        b.Property(x => x.FileExtension).HasMaxLength(20).IsRequired();
        b.Property(x => x.MimeType).HasMaxLength(100).IsRequired();
        b.Property(x => x.FileHash).HasMaxLength(100);
        b.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
        b.Property(x => x.PublicUrl).HasMaxLength(500);
        b.Property(x => x.StorageProvider).HasMaxLength(20);
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.SubCategory).HasMaxLength(50);
        b.Property(x => x.Tags).HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.LinkedModule).HasMaxLength(100);
        b.Property(x => x.LinkedDocType).HasMaxLength(100);
        b.Property(x => x.LinkedDocNo).HasMaxLength(100);
        b.Property(x => x.AllowedRoles).HasMaxLength(200);
        b.Property(x => x.UploadedBy).HasMaxLength(50).IsRequired();
        b.Property(x => x.UploadedByName).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.FileCode).IsUnique();
        b.HasMany(x => x.Versions)
         .WithOne(x => x.ElectronicFile)
         .HasForeignKey(x => x.ElectronicFileId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FileVersionConfig
    : IEntityTypeConfiguration<FileVersion>
{
    public void Configure(EntityTypeBuilder<FileVersion> b)
    {
        b.ToTable("KEFV");
        b.HasKey(x => x.Id);
        b.Property(x => x.FileName).HasMaxLength(300).IsRequired();
        b.Property(x => x.StoragePath).HasMaxLength(500).IsRequired();
        b.Property(x => x.PublicUrl).HasMaxLength(500);
        b.Property(x => x.FileHash).HasMaxLength(100);
        b.Property(x => x.ChangeNote).HasMaxLength(500);
        b.Property(x => x.UploadedBy).HasMaxLength(50).IsRequired();
    }
}
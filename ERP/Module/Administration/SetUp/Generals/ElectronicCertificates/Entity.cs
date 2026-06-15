using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.ElectronicCertificates;

// ═══════════════════════════════════════════════════════════════
// ELECTRONIC CERTIFICATE  (KECT)
// Centralised registry for all digital and physical certificates
// held by the organisation, its products, vendors and employees.
//
// CertType:
//   Quality      → ISO 9001, GMP, HACCP, FSSC 22000
//   Compliance   → regulatory, tax compliance, business licence
//   Origin       → certificate of origin (CO) for exports
//   Safety       → fire safety, OSH, dangerous goods
//   Environmental→ ISO 14001, carbon, waste management
//   Professional → employee licences, trade qualifications
//   Product      → product registration, halal, organic
//   Training     → staff training certificates
//   Vendor       → supplier qualification certs
//   Custom       → user-defined
//
// Status flow:
//   Pending → Active → Expired (auto by date)
//                    → Revoked  (manual)
//                    → Renewed  (links to successor)
// ═══════════════════════════════════════════════════════════════
public class ElectronicCertificate : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   CertNo          { get; set; } = null!;  // "CERT-2026-00001"
    public string   CertName        { get; set; } = null!;  // "ISO 9001:2015 Certificate"
    public string   CertType        { get; set; } = null!;  // see above
    public string?  CertStandard    { get; set; }           // "ISO 9001:2015"
    public string?  SerialNumber    { get; set; }           // official serial / ref number
    public string?  CertBody        { get; set; }           // "Bureau Veritas", "SGS"

    // ── Issued To ─────────────────────────────────────────────
    public string   HolderType      { get; set; } = "Company"; // Company/Employee/Product/Vendor
    public string?  HolderCode      { get; set; }           // e.g. employee code, item code
    public string   HolderName      { get; set; } = null!;  // display name
    public string?  HolderDept      { get; set; }           // for employee certs

    // ── Issuing Authority ─────────────────────────────────────
    public string?  IssuingAuthority{ get; set; }           // "Ministry of Commerce"
    public string?  IssuingCountry  { get; set; }           // "KH"
    public string?  AuthorityContact{ get; set; }

    // ── Dates ─────────────────────────────────────────────────
    public DateTime IssuedDate      { get; set; }
    public DateTime? ExpiryDate     { get; set; }           // null = no expiry
    public DateTime? RenewalDueDate { get; set; }           // reminder threshold
    public int?     RenewalDaysAlert{ get; set; } = 90;     // alert N days before expiry
    public DateTime? VerifiedAt     { get; set; }
    public string?  VerifiedBy      { get; set; }

    // ── Status ────────────────────────────────────────────────
    public string   Status          { get; set; } = "Pending"; // Pending/Active/Expired/Revoked/Renewed
    public string?  RevocationReason{ get; set; }
    public DateTime? RevokedAt      { get; set; }
    public int?     RenewedFromId   { get; set; }           // FK to previous cert
    public int?     RenewedToId     { get; set; }           // FK to successor cert

    // ── File / Document ───────────────────────────────────────
    public string?  FileUrl         { get; set; }           // scanned / e-cert URL
    public string?  FileName        { get; set; }
    public string?  ThumbnailUrl    { get; set; }
    public string?  QrCodeData      { get; set; }           // QR payload for verification
    public string?  VerificationUrl { get; set; }           // URL to verify online

    // ── Digital Signature ─────────────────────────────────────
    public bool     IsSigned        { get; set; } = false;
    public string?  SignatureCert   { get; set; }
    public string?  SignatureHash   { get; set; }
    public DateTime? SignedAt       { get; set; }

    // ── Linked ERP record ─────────────────────────────────────
    public string?  LinkedModule    { get; set; }
    public string?  LinkedDocType   { get; set; }
    public int?     LinkedDocId     { get; set; }
    public string?  LinkedDocNo     { get; set; }

    // ── Scope / Coverage ─────────────────────────────────────
    public string?  Scope           { get; set; }           // "Manufacturing of processed food"
    public string?  CoveredSites    { get; set; }           // JSON list of sites
    public string?  Restrictions    { get; set; }           // conditions / limitations

    // ── Metadata ──────────────────────────────────────────────
    public string?  Tags            { get; set; }
    public bool     IsPublic        { get; set; } = false;  // visible externally
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }

    // Navigation: extra attributes / fields
    public ICollection<CertificateAttribute> Attributes { get; set; }
        = new List<CertificateAttribute>();
}

// ─────────────────────────────────────────────────────────────
// CERTIFICATE ATTRIBUTE  (KECA)
// Flexible key-value pairs for cert-type-specific metadata.
// Examples:
//   ISO 9001  → { Scope, AuditDate, NextAudit, Lead Auditor }
//   Origin CO → { HSCode, ExportCountry, Destination, Qty }
//   Employee  → { LicenceCategory, Score, InstitutionName }
// ─────────────────────────────────────────────────────────────
public class CertificateAttribute
{
    public int     Id                      { get; set; }
    public int     ElectronicCertificateId { get; set; }
    public string  AttrKey                 { get; set; } = null!; // "HSCode"
    public string  AttrLabel               { get; set; } = null!; // "HS Code"
    public string? AttrValue               { get; set; }
    public string  DataType                { get; set; } = "Text"; // Text/Date/Number
    public int     SortOrder               { get; set; } = 0;

    public ElectronicCertificate? ElectronicCertificate { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class ElectronicCertificateConfig
    : IEntityTypeConfiguration<ElectronicCertificate>
{
    public void Configure(EntityTypeBuilder<ElectronicCertificate> b)
    {
        b.ToTable("KECT");
        b.HasKey(x => x.Id);

        b.Property(x => x.CertNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.CertName).HasMaxLength(300).IsRequired();
        b.Property(x => x.CertType).HasMaxLength(50).IsRequired();
        b.Property(x => x.CertStandard).HasMaxLength(100);
        b.Property(x => x.SerialNumber).HasMaxLength(100);
        b.Property(x => x.CertBody).HasMaxLength(200);
        b.Property(x => x.HolderType).HasMaxLength(20);
        b.Property(x => x.HolderCode).HasMaxLength(50);
        b.Property(x => x.HolderName).HasMaxLength(300).IsRequired();
        b.Property(x => x.HolderDept).HasMaxLength(100);
        b.Property(x => x.IssuingAuthority).HasMaxLength(300);
        b.Property(x => x.IssuingCountry).HasMaxLength(10);
        b.Property(x => x.AuthorityContact).HasMaxLength(200);
        b.Property(x => x.VerifiedBy).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.RevocationReason).HasMaxLength(500);
        b.Property(x => x.FileUrl).HasMaxLength(500);
        b.Property(x => x.FileName).HasMaxLength(300);
        b.Property(x => x.ThumbnailUrl).HasMaxLength(500);
        b.Property(x => x.QrCodeData).HasMaxLength(2000);
        b.Property(x => x.VerificationUrl).HasMaxLength(500);
        b.Property(x => x.SignatureCert).HasMaxLength(200);
        b.Property(x => x.SignatureHash).HasMaxLength(100);
        b.Property(x => x.LinkedModule).HasMaxLength(100);
        b.Property(x => x.LinkedDocType).HasMaxLength(100);
        b.Property(x => x.LinkedDocNo).HasMaxLength(100);
        b.Property(x => x.Scope).HasMaxLength(1000);
        b.Property(x => x.CoveredSites).HasMaxLength(1000);
        b.Property(x => x.Restrictions).HasMaxLength(1000);
        b.Property(x => x.Tags).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.CertNo).IsUnique();

        b.HasMany(x => x.Attributes)
         .WithOne(x => x.ElectronicCertificate)
         .HasForeignKey(x => x.ElectronicCertificateId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CertificateAttributeConfig
    : IEntityTypeConfiguration<CertificateAttribute>
{
    public void Configure(EntityTypeBuilder<CertificateAttribute> b)
    {
        b.ToTable("KECA");
        b.HasKey(x => x.Id);
        b.Property(x => x.AttrKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.AttrLabel).HasMaxLength(200).IsRequired();
        b.Property(x => x.AttrValue).HasMaxLength(1000);
        b.Property(x => x.DataType).HasMaxLength(20);
    }
}
using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.ElectronicDocuments;

// ═══════════════════════════════════════════════════════════════
// ELECTRONIC DOCUMENT  (KEDM)
// SAP B1 equivalent: Electronic Documents / eDocument
// Manages the lifecycle of digitally transmitted documents:
// e-Invoices, e-Delivery Notes, e-Credit Notes, e-Statements
// and Cambodia GDT e-invoicing submissions.
//
// Flow:
//   Base document posted → e-Doc generated (Draft)
//   → Signed / Formatted → Submitted → Accepted / Rejected
//   → Archived
//
// DocumentType:
//   EInvoice      → electronic tax invoice (GDT)
//   ECreditNote   → e-credit note
//   EDeliveryNote → electronic delivery / GRN
//   EStatement    → account statement
//   ERemittance   → payment remittance advice
//   EOrder        → electronic purchase order
//   Custom        → any bespoke e-document
//
// Channel:
//   API     → submitted via REST API (GDT portal, EDI hub)
//   Email   → sent as PDF/XML attachment
//   Portal  → uploaded to government / buyer portal
//   FTP     → SFTP / FTP drop
//   Manual  → manually exchanged (paper + scan)
// ═══════════════════════════════════════════════════════════════
public class ElectronicDocument : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   DocumentNo      { get; set; } = null!;  // "EINV-2026-00001"
    public string   DocumentType    { get; set; } = null!;  // EInvoice / ECreditNote / ...
    public string   Status          { get; set; } = "Draft";// Draft/Generated/Signed/Submitted/Accepted/Rejected/Cancelled/Archived

    // ── Source (base ERP document) ────────────────────────────
    public string   BaseDocType     { get; set; } = null!;  // "ARInvoice"
    public int?     BaseDocId       { get; set; }           // FK to source table
    public string?  BaseDocNo       { get; set; }           // "INV-2026-00123"
    public string?  BaseDocDate     { get; set; }           // "2026-01-15"

    // ── Business Partner ──────────────────────────────────────
    public string?  BPCode          { get; set; }
    public string?  BPName          { get; set; }
    public string?  BPTaxId         { get; set; }           // VAT/TIN of counterparty
    public string?  BPEmail         { get; set; }

    // ── Format & Channel ─────────────────────────────────────
    public string   Format          { get; set; } = "PDF";  // PDF/XML/JSON/UBL/EDIFACT/CSV
    public string   Channel         { get; set; } = "Email";// API/Email/Portal/FTP/Manual
    public string?  Encoding        { get; set; } = "UTF-8";
    public string?  SchemaVersion   { get; set; }           // e.g. "UBL 2.1", "GDT v2"

    // ── File ─────────────────────────────────────────────────
    public string?  FileName        { get; set; }
    public string?  FileUrl         { get; set; }           // storage path / URL
    public long?    FileSizeBytes   { get; set; }
    public string?  FileHash        { get; set; }           // SHA-256 for integrity
    public string?  MimeType        { get; set; }           // "application/pdf"

    // ── Financial ────────────────────────────────────────────
    public decimal? TotalAmount     { get; set; }
    public decimal? TaxAmount       { get; set; }
    public string?  Currency        { get; set; } = "KHR";

    // ── Submission ────────────────────────────────────────────
    public DateTime? GeneratedAt    { get; set; }
    public DateTime? SignedAt       { get; set; }
    public DateTime? SubmittedAt    { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string?   SubmittedBy    { get; set; }           // UserCode

    // ── External / Government response ────────────────────────
    public string?  ExternalRef     { get; set; }           // reference assigned by GDT / receiver
    public string?  SubmissionId    { get; set; }           // API transaction ID
    public string?  QrCodeData      { get; set; }           // QR code payload for GDT
    public string?  ResponseCode    { get; set; }           // e.g. "200", "E001"
    public string?  ResponseMessage { get; set; }
    public bool     IsValid         { get; set; } = false;  // true after acceptance

    // ── Digital Signature ─────────────────────────────────────
    public bool     IsSigned        { get; set; } = false;
    public string?  SignatureCert   { get; set; }           // thumbprint of signing cert
    public DateTime? CertExpiry     { get; set; }

    // ── Retry ────────────────────────────────────────────────
    public int      RetryCount      { get; set; } = 0;
    public DateTime? NextRetryAt    { get; set; }

    // ── Metadata ──────────────────────────────────────────────
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }

    // Navigation: status history / transmission log
    public ICollection<ElectronicDocumentLog> Logs { get; set; }
        = new List<ElectronicDocumentLog>();
}

// ─────────────────────────────────────────────────────────────
// ELECTRONIC DOCUMENT LOG  (KED1)
// Immutable audit trail of every status change and
// transmission attempt for each electronic document.
// ─────────────────────────────────────────────────────────────
public class ElectronicDocumentLog
{
    public int      Id                   { get; set; }
    public int      ElectronicDocumentId { get; set; }
    public DateTime LoggedAt             { get; set; } = DateTime.UtcNow;
    public string   EventType            { get; set; } = null!; // Generated/Signed/Submitted/Accepted/Rejected/Retried/Cancelled
    public string   FromStatus           { get; set; } = null!;
    public string   ToStatus             { get; set; } = null!;
    public string?  PerformedBy          { get; set; }  // UserCode
    public string?  Channel              { get; set; }
    public string?  ExternalRef          { get; set; }
    public string?  ResponseCode         { get; set; }
    public string?  Message              { get; set; }
    public bool     IsSuccess            { get; set; } = true;

    public ElectronicDocument? ElectronicDocument { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class ElectronicDocumentConfig
    : IEntityTypeConfiguration<ElectronicDocument>
{
    public void Configure(EntityTypeBuilder<ElectronicDocument> b)
    {
        b.ToTable("KEDM");
        b.HasKey(x => x.Id);

        b.Property(x => x.DocumentNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.DocumentType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20);
        b.Property(x => x.BaseDocType).HasMaxLength(50).IsRequired();
        b.Property(x => x.BaseDocNo).HasMaxLength(50);
        b.Property(x => x.BaseDocDate).HasMaxLength(20);
        b.Property(x => x.BPCode).HasMaxLength(50);
        b.Property(x => x.BPName).HasMaxLength(200);
        b.Property(x => x.BPTaxId).HasMaxLength(50);
        b.Property(x => x.BPEmail).HasMaxLength(200);
        b.Property(x => x.Format).HasMaxLength(20);
        b.Property(x => x.Channel).HasMaxLength(20);
        b.Property(x => x.Encoding).HasMaxLength(20);
        b.Property(x => x.SchemaVersion).HasMaxLength(50);
        b.Property(x => x.FileName).HasMaxLength(200);
        b.Property(x => x.FileUrl).HasMaxLength(500);
        b.Property(x => x.FileHash).HasMaxLength(100);
        b.Property(x => x.MimeType).HasMaxLength(100);
        b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.SubmittedBy).HasMaxLength(50);
        b.Property(x => x.ExternalRef).HasMaxLength(100);
        b.Property(x => x.SubmissionId).HasMaxLength(100);
        b.Property(x => x.QrCodeData).HasMaxLength(2000);
        b.Property(x => x.ResponseCode).HasMaxLength(50);
        b.Property(x => x.ResponseMessage).HasMaxLength(1000);
        b.Property(x => x.SignatureCert).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.DocumentNo).IsUnique();

        b.HasMany(x => x.Logs)
         .WithOne(x => x.ElectronicDocument)
         .HasForeignKey(x => x.ElectronicDocumentId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ElectronicDocumentLogConfig
    : IEntityTypeConfiguration<ElectronicDocumentLog>
{
    public void Configure(EntityTypeBuilder<ElectronicDocumentLog> b)
    {
        b.ToTable("KED1");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).HasMaxLength(30).IsRequired();
        b.Property(x => x.FromStatus).HasMaxLength(20).IsRequired();
        b.Property(x => x.ToStatus).HasMaxLength(20).IsRequired();
        b.Property(x => x.PerformedBy).HasMaxLength(50);
        b.Property(x => x.Channel).HasMaxLength(20);
        b.Property(x => x.ExternalRef).HasMaxLength(100);
        b.Property(x => x.ResponseCode).HasMaxLength(50);
        b.Property(x => x.Message).HasMaxLength(1000);
    }
}
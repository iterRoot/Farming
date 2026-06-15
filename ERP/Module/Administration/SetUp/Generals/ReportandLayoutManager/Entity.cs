using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.ReportLayoutManager;

// ═══════════════════════════════════════════════════════════════
// REPORT LAYOUT  (KRLM)
// SAP B1 equivalent: Report and Layout Manager
// Registers and manages report templates, print layouts,
// labels and certificates used across all ERP modules.
//
// LayoutType:
//   PrintLayout  → document print output (AR Invoice, PO ...)
//   Report       → data/analytical report
//   Label        → barcode / shipping label
//   Certificate  → certificates of origin, quality, etc.
//   Email        → HTML email templates
//
// LayoutEngine:
//   Crystal → SAP Crystal Reports (.rpt)
//   RDLC    → Microsoft RDLC (.rdlc)
//   HTML    → HTML template (.html)
//   Excel   → Excel report (.xlsx)
//   PDF     → static PDF form (.pdf)
// ═══════════════════════════════════════════════════════════════
public class ReportLayout : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   Code           { get; set; } = null!;  // "RPT-ARINV-001"
    public string   Name           { get; set; } = null!;  // "AR Invoice — Standard"
    public string?  Description    { get; set; }

    // ── Classification ────────────────────────────────────────
    public string   LayoutType     { get; set; } = "PrintLayout"; // PrintLayout/Report/Label/Certificate/Email
    public string   DocumentType   { get; set; } = null!;         // "ARInvoice","SalesOrder","All"
    public string   Module         { get; set; } = null!;         // "Sales","Purchasing","Inventory"
    public string   Category       { get; set; } = "General";     // "Financial","Operational","HR","Management"

    // ── Template File ─────────────────────────────────────────
    public string   LayoutEngine   { get; set; } = "Crystal";     // Crystal/RDLC/HTML/Excel/PDF
    public string?  FilePath       { get; set; }                   // server-side path to template
    public string?  FileUrl        { get; set; }                   // download/preview URL
    public string?  FileName       { get; set; }                   // e.g. "ARInvoice_Standard.rpt"
    public long?    FileSizeBytes  { get; set; }
    public string?  FileHash       { get; set; }                   // MD5 for integrity check
    public DateTime? LastUploadedAt{ get; set; }

    // ── Page Setup ────────────────────────────────────────────
    public string   PaperSize      { get; set; } = "A4";          // A4/A5/Letter/Legal/Custom
    public string   Orientation    { get; set; } = "Portrait";    // Portrait/Landscape
    public decimal? PageWidth      { get; set; }                   // mm (for custom size)
    public decimal? PageHeight     { get; set; }                   // mm
    public decimal  MarginTop      { get; set; } = 10;             // mm
    public decimal  MarginBottom   { get; set; } = 10;
    public decimal  MarginLeft     { get; set; } = 15;
    public decimal  MarginRight    { get; set; } = 10;

    // ── Output ────────────────────────────────────────────────
    public string   OutputFormat   { get; set; } = "PDF";          // PDF/Excel/Word/HTML/CSV
    public bool     AllowFormatChange { get; set; } = true;        // user can change output format
    public string   Language       { get; set; } = "en";           // en/km/zh
    public int      Copies         { get; set; } = 1;
    public bool     ShowWatermark  { get; set; } = false;
    public string?  WatermarkText  { get; set; }

    // ── Routing ───────────────────────────────────────────────
    public bool     IsDefault      { get; set; } = false;          // default layout for this DocumentType
    public bool     IsSystemLayout { get; set; } = false;          // shipped by ERP; can't be deleted
    public int      SortOrder      { get; set; } = 0;
    public string?  AllowedRoles   { get; set; }                   // comma-sep roles "Admin,Sales" or null=all

    // ── Versioning ────────────────────────────────────────────
    public string   Version        { get; set; } = "1.0";
    public string?  ChangeLog      { get; set; }

    // ── Status ────────────────────────────────────────────────
    public bool     IsActive       { get; set; } = true;
    public string?  Remarks        { get; set; }

    // Navigation
    public ICollection<ReportParameter> Parameters { get; set; } = new List<ReportParameter>();
}

// ─────────────────────────────────────────────────────────────
// REPORT PARAMETER  (KRLP)
// Named input parameters the user fills before running a report
// e.g. date range, warehouse filter, currency, etc.
// ─────────────────────────────────────────────────────────────
public class ReportParameter
{
    public int     Id             { get; set; }
    public int     ReportLayoutId { get; set; }
    public int     ParamOrder     { get; set; }
    public string  ParamKey       { get; set; } = null!;  // "DateFrom"
    public string  ParamLabel     { get; set; } = null!;  // "From Date"
    public string  DataType       { get; set; } = "String"; // String/Date/Integer/Decimal/Boolean/Select
    public string? DefaultValue   { get; set; }
    public string? Options        { get; set; }           // JSON for Select type
    public bool    IsRequired     { get; set; } = false;
    public bool    IsVisible      { get; set; } = true;

    public ReportLayout? ReportLayout { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class ReportLayoutConfig : IEntityTypeConfiguration<ReportLayout>
{
    public void Configure(EntityTypeBuilder<ReportLayout> b)
    {
        b.ToTable("KRLM");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.LayoutType).HasMaxLength(30);
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.Module).HasMaxLength(100).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.LayoutEngine).HasMaxLength(20);
        b.Property(x => x.FilePath).HasMaxLength(500);
        b.Property(x => x.FileUrl).HasMaxLength(500);
        b.Property(x => x.FileName).HasMaxLength(200);
        b.Property(x => x.FileHash).HasMaxLength(64);
        b.Property(x => x.PaperSize).HasMaxLength(20);
        b.Property(x => x.Orientation).HasMaxLength(15);
        b.Property(x => x.PageWidth).HasColumnType("decimal(8,2)");
        b.Property(x => x.PageHeight).HasColumnType("decimal(8,2)");
        b.Property(x => x.MarginTop).HasColumnType("decimal(6,2)");
        b.Property(x => x.MarginBottom).HasColumnType("decimal(6,2)");
        b.Property(x => x.MarginLeft).HasColumnType("decimal(6,2)");
        b.Property(x => x.MarginRight).HasColumnType("decimal(6,2)");
        b.Property(x => x.OutputFormat).HasMaxLength(20);
        b.Property(x => x.Language).HasMaxLength(10);
        b.Property(x => x.WatermarkText).HasMaxLength(100);
        b.Property(x => x.AllowedRoles).HasMaxLength(200);
        b.Property(x => x.Version).HasMaxLength(20);
        b.Property(x => x.ChangeLog).HasMaxLength(1000);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
        b.HasMany(x => x.Parameters)
         .WithOne(x => x.ReportLayout)
         .HasForeignKey(x => x.ReportLayoutId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReportParameterConfig : IEntityTypeConfiguration<ReportParameter>
{
    public void Configure(EntityTypeBuilder<ReportParameter> b)
    {
        b.ToTable("KRLP");
        b.HasKey(x => x.Id);
        b.Property(x => x.ParamKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.ParamLabel).HasMaxLength(200).IsRequired();
        b.Property(x => x.DataType).HasMaxLength(20);
        b.Property(x => x.DefaultValue).HasMaxLength(500);
        b.Property(x => x.Options).HasMaxLength(1000);
    }
}
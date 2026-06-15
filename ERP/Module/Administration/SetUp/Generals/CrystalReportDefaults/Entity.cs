using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.CrystalReportDefaults;

// ═══════════════════════════════════════════════════════════════
// CRYSTAL REPORT DEFAULT  (KCRD)
// SAP B1 equivalent: Default Values for SAP Crystal Reports
//
// Named elements injected into every Crystal Report at
// print-time: company info, branding, logos, footers,
// signatures, legal text, colors, fonts and format rules.
//
// Scope: DocumentType = "All"  → global default
//        DocumentType = "ARInvoice" → overrides global for that doc
//
// ElementType  / DataType pairs:
//   CompanyInfo  → Text / HTML
//   Logo         → ImageUrl / Base64
//   Header       → HTML / Text
//   Footer       → HTML / Text
//   Signature    → Text / HTML
//   Watermark    → Text
//   Color        → Color  (hex e.g. "#1e40af")
//   Font         → Text   (font-family name)
//   Legal        → HTML / Text
//   PageNumber   → Text   (format "Page {0} of {1}")
//   DateFormat   → Text   (e.g. "dd/MM/yyyy")
//   NumberFormat → JSON   ({decimal:".",thousand:",",places:2})
//   Custom       → any
// ═══════════════════════════════════════════════════════════════
public class CrystalReportDefault : AuditableEntity
{
    public string   ElementKey      { get; set; } = null!;  // "COMPANY_NAME"
    public string   ElementName     { get; set; } = null!;  // "Company Legal Name"
    public string   ElementType     { get; set; } = "Custom";
    public string   Category        { get; set; } = "General"; // Branding/Company/Page/Legal/Format
    public string   DocumentType    { get; set; } = "All";
    public string   Language        { get; set; } = "en";
    public string   DataType        { get; set; } = "Text";  // Text/Color/ImageUrl/Boolean/Number/JSON/HTML
    public string?  Value           { get; set; }
    public string?  DefaultValue    { get; set; }
    public string?  Placeholder     { get; set; }
    public string?  HelpText        { get; set; }
    public bool     IsRequired      { get; set; } = false;
    public bool     IsSystemElement { get; set; } = false;
    public string?  PreviewHint     { get; set; }
    public int      SortOrder       { get; set; } = 0;
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }
}

public class CrystalReportDefaultConfig
    : IEntityTypeConfiguration<CrystalReportDefault>
{
    public void Configure(EntityTypeBuilder<CrystalReportDefault> b)
    {
        b.ToTable("KCRP");
        b.HasKey(x => x.Id);

        b.Property(x => x.ElementKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.ElementName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ElementType).HasMaxLength(30);
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.DocumentType).HasMaxLength(100);
        b.Property(x => x.Language).HasMaxLength(10);
        b.Property(x => x.DataType).HasMaxLength(20);
        b.Property(x => x.Value).HasMaxLength(4000);
        b.Property(x => x.DefaultValue).HasMaxLength(4000);
        b.Property(x => x.Placeholder).HasMaxLength(300);
        b.Property(x => x.HelpText).HasMaxLength(500);
        b.Property(x => x.PreviewHint).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => new { x.ElementKey, x.DocumentType, x.Language }).IsUnique();
    }
}
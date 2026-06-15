using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.PredefinedText;

// ═══════════════════════════════════════════════════════════════
// PREDEFINED TEXT  (KPTX)
// SAP B1 equivalent: Predefined Texts
// Saved text snippets inserted into document remarks, notes,
// email bodies, and print layouts. Eliminates repetitive typing.
//
// Examples:
//   "Thank you for your business."
//   "Payment is due within 30 days of invoice date."
//   "Goods once sold cannot be returned."
//   "This quotation is valid for 7 days."
//
// Organised by Category + Module + Language so users can
// quickly filter relevant snippets when drafting documents.
// ═══════════════════════════════════════════════════════════════
public class PredefinedText : AuditableEntity
{
    public string   Code        { get; set; } = null!;  // e.g. "TXT-PAY30"
    public string   Name        { get; set; } = null!;  // e.g. "Payment Due 30 Days"
    public string?  Category    { get; set; }           // "Sales","Purchasing","General","Legal","HR"
    public string?  Module      { get; set; }           // "ARInvoice","PurchaseOrder","All"
    public string   Language    { get; set; } = "en";   // ISO code: en, km, zh, th
    public string   TextContent { get; set; } = null!;  // the actual snippet body
    public string?  Tags        { get; set; }           // comma-separated: "payment,terms,due"
    public int      SortOrder   { get; set; } = 0;
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }
}

public class PredefinedTextConfig : IEntityTypeConfiguration<PredefinedText>
{
    public void Configure(EntityTypeBuilder<PredefinedText> b)
    {
        b.ToTable("KPTX");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.Module).HasMaxLength(100);
        b.Property(x => x.Language).HasMaxLength(10);
        b.Property(x => x.TextContent).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Tags).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
    }
}
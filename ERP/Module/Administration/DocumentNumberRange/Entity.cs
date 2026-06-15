using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DocumentNumberRange;

// ═══════════════════════════════════════════════════════════════════
// DOCUMENT NUMBER RANGE (KDNR)
// Equivalent to SAP B1 "Administration → Document Numbering"
// Controls the auto-numbering format for every document type
//
// Format example: SO-2026-00001
//   Prefix:  SO
//   Year:    2026  (optional)
//   Seq:     00001 (5-digit padded, increments per use)
// ═══════════════════════════════════════════════════════════════════
public class DocumentNumberRange : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────────
    public string   DocumentType    { get; set; } = null!;   // e.g. "SaleOrder", "ArInvoice"
    public string   DocumentTypeCode{ get; set; } = null!;   // e.g. "SO", "ARI"
    public string   SeriesName      { get; set; } = "Primary";
    public bool     IsDefault       { get; set; } = true;
    public bool     IsLocked        { get; set; } = false;   // prevent new docs on this series

    // ── Format ────────────────────────────────────────────────────
    public string   Prefix          { get; set; } = null!;   // "SO", "ARI", etc.
    public bool     IncludeYear     { get; set; } = true;    // SO-2026-XXXXX
    public int      PadLength       { get; set; } = 5;       // zero-padding digits

    // ── Range ─────────────────────────────────────────────────────
    public int      FirstNum        { get; set; } = 1;       // starting number
    public int      NextNum         { get; set; } = 1;       // current next number to use
    public int?     LastNum         { get; set; }            // optional upper limit (null = unlimited)

    // ── Remarks ───────────────────────────────────────────────────
    public string?  Remarks         { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATION
// ═══════════════════════════════════════════════════════════════════
public class DocumentNumberRangeConfig : IEntityTypeConfiguration<DocumentNumberRange>
{
    public void Configure(EntityTypeBuilder<DocumentNumberRange> builder)
    {
        builder.ToTable("KDNR");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocumentType).HasMaxLength(50).IsRequired();
        builder.Property(m => m.DocumentTypeCode).HasMaxLength(10).IsRequired();
        builder.Property(m => m.SeriesName).HasMaxLength(50).HasDefaultValue("Primary");
        builder.Property(m => m.Prefix).HasMaxLength(20).IsRequired();
        builder.Property(m => m.PadLength).HasDefaultValue(5);
        builder.Property(m => m.FirstNum).HasDefaultValue(1);
        builder.Property(m => m.NextNum).HasDefaultValue(1);
        builder.Property(m => m.Remarks).HasMaxLength(200);

        // Unique: one series per document type
        builder.HasIndex(m => new { m.DocumentType, m.SeriesName }).IsUnique();
        builder.HasIndex(m => m.DocumentType);
        builder.HasIndex(m => m.IsDefault);
    }
}
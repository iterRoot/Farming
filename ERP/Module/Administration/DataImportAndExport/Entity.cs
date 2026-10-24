using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DataImportAndExport;

// ═══════════════════════════════════════════════════════════════════
// DATA TRANSFER JOB (KDIE)
// Equivalent to SAP B1 "Administration → Data Import/Export"
//
// One row per import or export run. Gives an audit trail of who moved
// what data, when, how many rows landed and what went wrong.
// ═══════════════════════════════════════════════════════════════════
public class DataTransferJob : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────────
    public string  JobNo      { get; set; } = null!;  // DIE-2026-00001
    public string  Direction  { get; set; } = "I";    // I = Import, E = Export
    public string  EntityKey  { get; set; } = null!;  // "BusinessPartner", "ChartOfAccount"
    public string  EntityLabel{ get; set; } = null!;  // "Business Partner Master Data"

    // ── Source / target file ──────────────────────────────────────
    public string? FileName   { get; set; }
    public string  Format     { get; set; } = "CSV";
    public long    FileSize   { get; set; }

    // ── Behaviour ─────────────────────────────────────────────────
    // A  = Add only (fail on existing key)
    // U  = Update only (fail on missing key)
    // AU = Add or Update (upsert)
    public string  Mode       { get; set; } = "A";

    // ── Outcome ───────────────────────────────────────────────────
    // P = Pending, R = Running, S = Success, W = Success with warnings, F = Failed
    public string  Status     { get; set; } = "P";
    public int     TotalRows  { get; set; }
    public int     SuccessRows{ get; set; }
    public int     FailedRows { get; set; }
    public int     SkippedRows{ get; set; }

    // ── Diagnostics ───────────────────────────────────────────────
    public string? ErrorLog   { get; set; }   // newline-joined "Row 12: reason"
    public string? Remarks    { get; set; }

    public DateTime? StartedAt   { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int       DurationMs  { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATION
// ═══════════════════════════════════════════════════════════════════
public class DataTransferJobConfig : IEntityTypeConfiguration<DataTransferJob>
{
    public void Configure(EntityTypeBuilder<DataTransferJob> builder)
    {
        builder.ToTable("KDIE");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.JobNo).HasMaxLength(30).IsRequired();
        builder.Property(m => m.Direction).HasMaxLength(1).IsRequired().HasDefaultValue("I");
        builder.Property(m => m.EntityKey).HasMaxLength(60).IsRequired();
        builder.Property(m => m.EntityLabel).HasMaxLength(120).IsRequired();
        builder.Property(m => m.FileName).HasMaxLength(260);
        builder.Property(m => m.Format).HasMaxLength(10).HasDefaultValue("CSV");
        builder.Property(m => m.Mode).HasMaxLength(2).HasDefaultValue("A");
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("P");
        builder.Property(m => m.Remarks).HasMaxLength(500);
        // ErrorLog is unbounded on purpose — a 5000-row failure needs the room.

        builder.HasIndex(m => m.JobNo).IsUnique();
        builder.HasIndex(m => new { m.Direction, m.EntityKey });
        builder.HasIndex(m => m.CreatedAt);
    }
}

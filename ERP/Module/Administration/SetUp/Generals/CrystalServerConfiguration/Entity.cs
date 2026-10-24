using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.CrystalServer;

// ═══════════════════════════════════════════════════════════════
// CRYSTAL SERVER CONFIGURATION  (KCRS)
// SAP B1 equivalent: Crystal Reports Server / Print Layout
// Stores connection & runtime settings for Crystal Reports
// integration. Single-record pattern (upsert on PUT).
//
// Sections:
//   Server   → host, port, protocol
//   Auth     → username / password (AES-encrypted at rest)
//   Database → DB connection the report engine uses
//   Runtime  → timeout, output folder, default format
//   SMTP     → optional email delivery of reports
// ═══════════════════════════════════════════════════════════════
public class CrystalServerConfig : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   ConfigName     { get; set; } = "Default";

    // ── Server Connection ─────────────────────────────────────
    public string?  Host           { get; set; }   // e.g. "192.168.1.100"
    public int      Port           { get; set; } = 6405;
    public string   Protocol       { get; set; } = "http"; // http / https
    public string?  VirtualDir     { get; set; }   // e.g. "/crystal"

    // ── Authentication ────────────────────────────────────────
    public string?  Username       { get; set; }
    public string?  Password       { get; set; }   // store encrypted
    public string   AuthType       { get; set; } = "Enterprise"; // Enterprise / Windows / LDAP

    // ── Database Source ───────────────────────────────────────
    public string?  DbHost         { get; set; }
    public int?     DbPort         { get; set; }
    public string?  DbName         { get; set; }
    public string?  DbUser         { get; set; }
    public string?  DbPassword     { get; set; }
    public string   DbProvider     { get; set; } = "PostgreSQL"; // PostgreSQL / MSSQL / MySQL

    // ── Runtime ───────────────────────────────────────────────
    public int      TimeoutSeconds { get; set; } = 60;
    public string?  ReportFolder   { get; set; }   // base path to .rpt files
    public string?  OutputFolder   { get; set; }   // exported file output
    public string   DefaultFormat  { get; set; } = "PDF"; // PDF / Excel / Word
    public int      MaxRows        { get; set; } = 50000;
    public bool     CacheEnabled   { get; set; } = true;
    public int      CacheMinutes   { get; set; } = 15;

    // ── SMTP (optional report email delivery) ─────────────────
    public string?  SmtpHost       { get; set; }
    public int?     SmtpPort       { get; set; }
    public string?  SmtpUser       { get; set; }
    public string?  SmtpPassword   { get; set; }
    public bool     SmtpUseSsl     { get; set; } = true;
    public string?  SmtpFromEmail  { get; set; }
    public string?  SmtpFromName   { get; set; }

    // ── Status ────────────────────────────────────────────────
    public bool     IsEnabled      { get; set; } = true;
    public string?  Remarks        { get; set; }
}

public class CrystalServerConfigEntityConfig
    : IEntityTypeConfiguration<CrystalServerConfig>
{
    public void Configure(EntityTypeBuilder<CrystalServerConfig> b)
    {
        b.ToTable("KCRS");
        b.HasKey(x => x.Id);

        b.Property(x => x.ConfigName).HasMaxLength(100);
        b.Property(x => x.Host).HasMaxLength(200);
        b.Property(x => x.Protocol).HasMaxLength(10);
        b.Property(x => x.VirtualDir).HasMaxLength(100);
        b.Property(x => x.Username).HasMaxLength(100);
        b.Property(x => x.Password).HasMaxLength(500);
        b.Property(x => x.AuthType).HasMaxLength(20);
        b.Property(x => x.DbHost).HasMaxLength(200);
        b.Property(x => x.DbName).HasMaxLength(100);
        b.Property(x => x.DbUser).HasMaxLength(100);
        b.Property(x => x.DbPassword).HasMaxLength(500);
        b.Property(x => x.DbProvider).HasMaxLength(20);
        b.Property(x => x.ReportFolder).HasMaxLength(500);
        b.Property(x => x.OutputFolder).HasMaxLength(500);
        b.Property(x => x.DefaultFormat).HasMaxLength(20);
        b.Property(x => x.SmtpHost).HasMaxLength(200);
        b.Property(x => x.SmtpUser).HasMaxLength(100);
        b.Property(x => x.SmtpPassword).HasMaxLength(500);
        b.Property(x => x.SmtpFromEmail).HasMaxLength(200);
        b.Property(x => x.SmtpFromName).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);
    }
}
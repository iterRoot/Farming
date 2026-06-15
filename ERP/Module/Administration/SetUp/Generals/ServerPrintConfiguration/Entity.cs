using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.ServerPrintConfig;

// ═══════════════════════════════════════════════════════════════
// SERVER PRINT CONFIGURATION  (KSPC)
// SAP B1 equivalent: Server Print configuration
// Registers physical and virtual printers available on the
// print server. Each entry defines connection details,
// paper defaults, hardware capabilities, and document routing.
//
// PrinterType:
//   Network → shared printer (UNC path or IP/port)
//   Local   → printer on the ERP application server
//   Virtual → PDF writer, XPS, email-as-print
//   Cloud   → cloud print service
//   Zebra   → label printer (ZPL/EPL)
// ═══════════════════════════════════════════════════════════════
public class ServerPrintConfig : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   PrinterCode     { get; set; } = null!;
    public string   PrinterName     { get; set; } = null!;
    public string   PrinterType     { get; set; } = "Network";
    public string?  Location        { get; set; }
    public string?  Description     { get; set; }

    // ── Connection ────────────────────────────────────────────
    public string?  NetworkPath     { get; set; }   // \\SERVER\PrinterName
    public string?  IpAddress       { get; set; }   // 192.168.1.50
    public int?     Port            { get; set; }   // 9100 JetDirect / 631 IPP / 515 LPD
    public string   Protocol        { get; set; } = "RAW"; // RAW/IPP/LPD/WSD
    public string?  ShareName       { get; set; }
    public string?  DriverName      { get; set; }
    public string?  QueueName       { get; set; }
    public string?  PrintUsername   { get; set; }
    public string?  PrintPassword   { get; set; }   // stored encrypted

    // ── Page defaults ─────────────────────────────────────────
    public string   PaperSize       { get; set; } = "A4";
    public string   Orientation     { get; set; } = "Portrait";
    public decimal? CustomWidth     { get; set; }   // mm
    public decimal? CustomHeight    { get; set; }   // mm
    public int      DefaultCopies   { get; set; } = 1;
    public string   ColorMode       { get; set; } = "Mono";  // Color/Mono/Auto
    public string   DuplexMode      { get; set; } = "None";  // None/LongEdge/ShortEdge

    // ── Capabilities ─────────────────────────────────────────
    public bool     CanColor        { get; set; } = false;
    public bool     CanDuplex       { get; set; } = false;
    public bool     CanStaple       { get; set; } = false;
    public int      MaxDpi          { get; set; } = 600;
    public string?  SupportedPapers { get; set; }   // JSON: ["A4","A5","Letter"]
    public string?  PrintLanguage   { get; set; } = "PCL"; // PCL/PostScript/ZPL/PDF

    // ── Routing ───────────────────────────────────────────────
    public bool     IsDefault       { get; set; } = false;
    public bool     IsActive        { get; set; } = true;
    public int      SortOrder       { get; set; } = 0;
    public string?  Remarks         { get; set; }

    public ICollection<PrintDocumentRoute> Routes { get; set; }
        = new List<PrintDocumentRoute>();
}

// ─────────────────────────────────────────────────────────────
// PRINT DOCUMENT ROUTE  (KSPR)
// Which document type → which printer, how many copies, which tray
// ─────────────────────────────────────────────────────────────
public class PrintDocumentRoute
{
    public int     Id                  { get; set; }
    public int     ServerPrintConfigId { get; set; }
    public string  DocumentType        { get; set; } = null!;
    public string? UserCode            { get; set; }  // null = all users
    public int     Copies              { get; set; } = 1;
    public string? PaperTray           { get; set; }  // "Tray1","AutoSelect","Bypass"
    public string? PaperSize           { get; set; }  // override printer default
    public bool    IsEnabled           { get; set; } = true;
    public int     SortOrder           { get; set; } = 0;

    public ServerPrintConfig? ServerPrintConfig { get; set; }
}

// ─── EF ──────────────────────────────────────────────────────
public class ServerPrintConfigEntityConfig
    : IEntityTypeConfiguration<ServerPrintConfig>
{
    public void Configure(EntityTypeBuilder<ServerPrintConfig> b)
    {
        b.ToTable("KSPC");
        b.HasKey(x => x.Id);
        b.Property(x => x.PrinterCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.PrinterName).HasMaxLength(200).IsRequired();
        b.Property(x => x.PrinterType).HasMaxLength(20);
        b.Property(x => x.Location).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.NetworkPath).HasMaxLength(300);
        b.Property(x => x.IpAddress).HasMaxLength(50);
        b.Property(x => x.Protocol).HasMaxLength(20);
        b.Property(x => x.ShareName).HasMaxLength(100);
        b.Property(x => x.DriverName).HasMaxLength(200);
        b.Property(x => x.QueueName).HasMaxLength(100);
        b.Property(x => x.PrintUsername).HasMaxLength(100);
        b.Property(x => x.PrintPassword).HasMaxLength(300);
        b.Property(x => x.PaperSize).HasMaxLength(20);
        b.Property(x => x.Orientation).HasMaxLength(15);
        b.Property(x => x.CustomWidth).HasColumnType("decimal(8,2)");
        b.Property(x => x.CustomHeight).HasColumnType("decimal(8,2)");
        b.Property(x => x.ColorMode).HasMaxLength(10);
        b.Property(x => x.DuplexMode).HasMaxLength(15);
        b.Property(x => x.SupportedPapers).HasMaxLength(300);
        b.Property(x => x.PrintLanguage).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => x.PrinterCode).IsUnique();
        b.HasMany(x => x.Routes)
         .WithOne(x => x.ServerPrintConfig)
         .HasForeignKey(x => x.ServerPrintConfigId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PrintDocumentRouteConfig
    : IEntityTypeConfiguration<PrintDocumentRoute>
{
    public void Configure(EntityTypeBuilder<PrintDocumentRoute> b)
    {
        b.ToTable("KSPR");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        b.Property(x => x.UserCode).HasMaxLength(50);
        b.Property(x => x.PaperTray).HasMaxLength(50);
        b.Property(x => x.PaperSize).HasMaxLength(20);
    }
}
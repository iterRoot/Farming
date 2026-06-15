using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.SAPLinks;

// ═══════════════════════════════════════════════════════════════
// SAP LINK  (KSPL)
// Centralised shortcut registry — stores URLs to external
// portals, government systems, banking platforms, partner
// sites, API endpoints and internal navigation shortcuts.
//
// Acts as a contextual bookmark manager embedded in the ERP:
//   • Quick-launch panel on the dashboard
//   • Contextual links inside document forms
//   • Module-specific portal shortcuts
//   • Admin reference links (tax office, customs, banks)
//
// Category:
//   Government  → GDT, Ministry of Commerce, Customs
//   Banking     → ABA, Acleda, Canadia, Wing portal
//   Partner     → supplier/customer web portals
//   Integration → API endpoints, webhooks, EDI hubs
//   Reference   → documentation, standards bodies, ISO
//   Social      → LinkedIn, Telegram group, company social
//   Internal    → other internal systems (HR, EPOS, WMS)
//   Finance     → stock exchanges, exchange-rate feeds
//   Custom      → any user-defined link
//
// LinkType:
//   URL         → standard hyperlink (default)
//   API         → REST/SOAP endpoint definition
//   Report      → link to a report or dashboard
//   Document    → link to a document / form
//   Navigation  → internal ERP route (/Sales/ARInvoice/List)
// ═══════════════════════════════════════════════════════════════
public class SAPLink : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   Code            { get; set; } = null!;  // "LNK-GDT-EINVOICE"
    public string   Title           { get; set; } = null!;  // "GDT e-Invoice Portal"
    public string   LinkType        { get; set; } = "URL";  // URL/API/Report/Document/Navigation
    public string   Category        { get; set; } = "Custom";
    public string?  SubCategory     { get; set; }           // free-text grouping

    // ── Target ────────────────────────────────────────────────
    public string   Url             { get; set; } = null!;  // full URL or internal path
    public string?  IconName        { get; set; }           // icon identifier / emoji / CSS class
    public string?  IconUrl         { get; set; }           // custom icon image URL
    public string?  BadgeText       { get; set; }           // "NEW", "BETA", "v2"
    public string   Target          { get; set; } = "_blank"; // _blank / _self / _parent
    public bool     IsEmbeddable    { get; set; } = false;  // can render in iframe panel

    // ── HTTP / API details (for LinkType=API) ─────────────────
    public string?  HttpMethod      { get; set; }           // GET / POST / PUT
    public string?  AuthType        { get; set; }           // None / ApiKey / Bearer / Basic
    public string?  ApiKeyHeader    { get; set; }           // e.g. "X-Api-Key"
    public string?  SamplePayload   { get; set; }           // JSON example request body
    public string?  ContentType     { get; set; }           // "application/json"

    // ── Context ───────────────────────────────────────────────
    public string?  LinkedModule    { get; set; }           // show in this module's sidebar
    public string?  LinkedDocType   { get; set; }           // show on this document form
    public string?  Description     { get; set; }
    public string?  Tags            { get; set; }           // comma-sep: "tax,gdt,einvoice"
    public string?  Notes           { get; set; }           // internal admin notes

    // ── Access ────────────────────────────────────────────────
    public bool     IsPublic        { get; set; } = true;   // visible to all users
    public string?  AllowedRoles    { get; set; }           // "Admin,Finance" — null = all
    public bool     RequiresVPN     { get; set; } = false;
    public bool     RequiresLogin   { get; set; } = false;
    public string?  LoginHint       { get; set; }           // "Use your AD credentials"

    // ── Pinning / sort ────────────────────────────────────────
    public bool     IsPinned        { get; set; } = false;  // show in quick-launch panel
    public bool     IsOnDashboard   { get; set; } = false;  // show on main dashboard
    public int      SortOrder       { get; set; } = 0;

    // ── Health / usage ────────────────────────────────────────
    public int      ClickCount      { get; set; } = 0;
    public DateTime? LastClickedAt  { get; set; }
    public string?  LastStatusCode  { get; set; }           // "200", "404", "timeout"
    public DateTime? LastCheckedAt  { get; set; }
    public bool     IsHealthy       { get; set; } = true;

    // ── Lifecycle ─────────────────────────────────────────────
    public bool     IsActive        { get; set; } = true;
    public DateTime? ExpiresAt      { get; set; }           // auto-deactivate
    public string?  Remarks         { get; set; }

    // Navigation
    public ICollection<SAPLinkUsage> Usages { get; set; }
        = new List<SAPLinkUsage>();
}

// ─────────────────────────────────────────────────────────────
// SAP LINK USAGE  (KSPU)
// Click / access audit trail per link per user.
// Stores last 1000 events per link for analytics.
// ─────────────────────────────────────────────────────────────
public class SAPLinkUsage
{
    public int      Id          { get; set; }
    public int      SAPLinkId   { get; set; }
    public DateTime ClickedAt   { get; set; } = DateTime.UtcNow;
    public string?  UserCode    { get; set; }
    public string?  UserName    { get; set; }
    public string?  IpAddress   { get; set; }
    public string?  UserAgent   { get; set; }
    public string?  Referrer    { get; set; }
    public string?  ResponseCode{ get; set; }   // for API links

    public SAPLink? SAPLink { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class SAPLinkConfig : IEntityTypeConfiguration<SAPLink>
{
    public void Configure(EntityTypeBuilder<SAPLink> b)
    {
        b.ToTable("KSPL");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.LinkType).HasMaxLength(20);
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.SubCategory).HasMaxLength(50);
        b.Property(x => x.Url).HasMaxLength(2000).IsRequired();
        b.Property(x => x.IconName).HasMaxLength(100);
        b.Property(x => x.IconUrl).HasMaxLength(500);
        b.Property(x => x.BadgeText).HasMaxLength(20);
        b.Property(x => x.Target).HasMaxLength(20);
        b.Property(x => x.HttpMethod).HasMaxLength(10);
        b.Property(x => x.AuthType).HasMaxLength(20);
        b.Property(x => x.ApiKeyHeader).HasMaxLength(100);
        b.Property(x => x.SamplePayload).HasMaxLength(2000);
        b.Property(x => x.ContentType).HasMaxLength(100);
        b.Property(x => x.LinkedModule).HasMaxLength(100);
        b.Property(x => x.LinkedDocType).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Tags).HasMaxLength(500);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.AllowedRoles).HasMaxLength(200);
        b.Property(x => x.LoginHint).HasMaxLength(300);
        b.Property(x => x.LastStatusCode).HasMaxLength(10);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();

        b.HasMany(x => x.Usages)
         .WithOne(x => x.SAPLink)
         .HasForeignKey(x => x.SAPLinkId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SAPLinkUsageConfig : IEntityTypeConfiguration<SAPLinkUsage>
{
    public void Configure(EntityTypeBuilder<SAPLinkUsage> b)
    {
        b.ToTable("KSPU");
        b.HasKey(x => x.Id);
        b.Property(x => x.UserCode).HasMaxLength(50);
        b.Property(x => x.UserName).HasMaxLength(200);
        b.Property(x => x.IpAddress).HasMaxLength(50);
        b.Property(x => x.UserAgent).HasMaxLength(500);
        b.Property(x => x.Referrer).HasMaxLength(500);
        b.Property(x => x.ResponseCode).HasMaxLength(10);
    }
}
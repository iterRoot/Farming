using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.MessagePreferences;

// ═══════════════════════════════════════════════════════════════
// MESSAGE PREFERENCE  (KMSP)
// Per-user notification preferences.
// Each row = one user + one event + one delivery channel.
//
// Example rows for user U001:
//   U001 | ARInvoice.Posted  | Email    | Immediate | ✅
//   U001 | ARInvoice.Posted  | InApp    | Immediate | ✅
//   U001 | LowStock.Alert    | Email    | Daily     | ✅
//   U001 | Login.NewDevice   | SMS      | Immediate | ✅
//   U001 | Payroll.Generated | Email    | Immediate | ❌
//
// Channels   : Email / SMS / InApp / Webhook / Push
// Frequency  : Immediate / Digest / Weekly / Never
// Priority   : Low / Medium / High / Critical
// ═══════════════════════════════════════════════════════════════
public class MessagePreference : AuditableEntity
{
    // ── Who ───────────────────────────────────────────────────
    public string   UserCode        { get; set; } = null!;  // "All" = system-wide default
    public string?  UserName        { get; set; }           // denormalised display

    // ── What event ────────────────────────────────────────────
    public string   EventKey        { get; set; } = null!;  // e.g. "ARInvoice.Posted"
    public string   EventName       { get; set; } = null!;  // e.g. "AR Invoice Posted"
    public string   Category        { get; set; } = "General"; // Document / Alert / HR / System / Security
    public string   Module          { get; set; } = null!;  // e.g. "ARInvoice"
    public string   Priority        { get; set; } = "Medium";  // Low / Medium / High / Critical

    // ── How it's sent ─────────────────────────────────────────
    public string   Channel         { get; set; } = "InApp"; // Email / SMS / InApp / Webhook / Push
    public bool     IsEnabled       { get; set; } = true;
    public string   Frequency       { get; set; } = "Immediate"; // Immediate / Digest / Weekly / Never
    public string?  DigestTime      { get; set; }           // e.g. "08:00" for daily digest
    public int?     DigestDay       { get; set; }           // 1-7 for weekly (1=Monday)

    // ── Delivery overrides ────────────────────────────────────
    public string?  EmailOverride   { get; set; }           // if null, use user's email
    public string?  PhoneOverride   { get; set; }           // if null, use user's phone
    public string?  WebhookUrl      { get; set; }           // for Webhook channel
    public string?  WebhookSecret   { get; set; }           // HMAC secret for webhook

    // ── Content ───────────────────────────────────────────────
    public string?  SubjectTemplate { get; set; }           // Email subject template
    public string?  BodyTemplate    { get; set; }           // Message body template
    public string?  ConditionFilter { get; set; }           // JSON: only fire if condition met

    // ── Metadata ──────────────────────────────────────────────
    public int      SortOrder       { get; set; } = 0;
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }
}

public class MessagePreferenceConfig
    : IEntityTypeConfiguration<MessagePreference>
{
    public void Configure(EntityTypeBuilder<MessagePreference> b)
    {
        b.ToTable("KMSP");
        b.HasKey(x => x.Id);

        b.Property(x => x.UserCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.UserName).HasMaxLength(200);
        b.Property(x => x.EventKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.EventName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.Module).HasMaxLength(100).IsRequired();
        b.Property(x => x.Priority).HasMaxLength(20);
        b.Property(x => x.Channel).HasMaxLength(20).IsRequired();
        b.Property(x => x.Frequency).HasMaxLength(20);
        b.Property(x => x.DigestTime).HasMaxLength(10);
        b.Property(x => x.EmailOverride).HasMaxLength(200);
        b.Property(x => x.PhoneOverride).HasMaxLength(50);
        b.Property(x => x.WebhookUrl).HasMaxLength(500);
        b.Property(x => x.WebhookSecret).HasMaxLength(200);
        b.Property(x => x.SubjectTemplate).HasMaxLength(300);
        b.Property(x => x.BodyTemplate).HasMaxLength(2000);
        b.Property(x => x.ConditionFilter).HasMaxLength(1000);
        b.Property(x => x.Remarks).HasMaxLength(500);

        // One preference per user + event + channel
        b.HasIndex(x => new { x.UserCode, x.EventKey, x.Channel }).IsUnique();
    }
}
using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.ExchangeRate;

// ═══════════════════════════════════════════════════════════════
// EXCHANGE RATE AUTO-UPDATE SETTINGS  (KFXS) — a single row.
// Which currencies to keep current, against which base, and when
// the daily background fetch runs (UTC hour). The last-run fields
// are maintained by the fetcher, not typed in.
// ═══════════════════════════════════════════════════════════════
public class ExchangeRateSettings : AuditableEntity
{
    public bool      AutoFetchEnabled { get; set; }
    public string    BaseCurrency     { get; set; } = "KHR";
    public string    Currencies       { get; set; } = "USD";   // comma-separated ISO codes
    public int       RunAtHourUtc     { get; set; } = 1;       // 0–23
    public DateTime? LastAttemptAt    { get; set; }
    public DateTime? LastSuccessAt    { get; set; }
    public string?   LastStatus       { get; set; }            // Success | Partial | Failed
    public string?   LastMessage      { get; set; }
}

public class ExchangeRateSettingsConfig : IEntityTypeConfiguration<ExchangeRateSettings>
{
    public void Configure(EntityTypeBuilder<ExchangeRateSettings> b)
    {
        b.ToTable("KFXS");
        b.HasKey(x => x.Id);
        b.Property(x => x.BaseCurrency).HasMaxLength(10).IsRequired();
        b.Property(x => x.Currencies).HasMaxLength(200).IsRequired();
        b.Property(x => x.LastStatus).HasMaxLength(20);
        b.Property(x => x.LastMessage).HasMaxLength(1000);
    }
}

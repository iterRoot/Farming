using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.DashboardParameters;

// ═══════════════════════════════════════════════════════════════
// DASHBOARD PARAMETER  (KDSP)
// Configurable key-value parameters that control dashboard
// behaviour: refresh rates, thresholds, display options,
// alert rules, data ranges, and visual settings.
//
// DataType values:
//   String  → free text
//   Integer → whole number (min/max enforced)
//   Decimal → decimal number (min/max enforced)
//   Boolean → true/false toggle
//   Color   → hex colour picker
//   Select  → dropdown (options stored as JSON array)
//   JSON    → raw JSON for complex config
//
// Categories:
//   Performance | Display | Alerts | Data | Appearance | Integration
// ═══════════════════════════════════════════════════════════════
public class DashboardParameter : AuditableEntity
{
    public string   Code         { get; set; } = null!;  // e.g. "REFRESH_INTERVAL"
    public string   Name         { get; set; } = null!;  // e.g. "Refresh Interval (sec)"
    public string   Category     { get; set; } = "General"; // Performance / Display / Alerts ...
    public string   DataType     { get; set; } = "String";  // String / Integer / Decimal / Boolean / Color / Select / JSON
    public string?  Value        { get; set; }           // current value (as string)
    public string?  DefaultValue { get; set; }           // factory default
    public string?  MinValue     { get; set; }           // for Integer / Decimal
    public string?  MaxValue     { get; set; }           // for Integer / Decimal
    public string?  Options      { get; set; }           // JSON array for Select type: ["opt1","opt2"]
    public string?  Unit         { get; set; }           // e.g. "seconds", "%", "rows"
    public string?  Description  { get; set; }
    public bool     IsSystem     { get; set; } = false;  // system params cannot be deleted
    public bool     IsActive     { get; set; } = true;
    public string?  Remarks      { get; set; }
}

public class DashboardParameterConfig
    : IEntityTypeConfiguration<DashboardParameter>
{
    public void Configure(EntityTypeBuilder<DashboardParameter> b)
    {
        b.ToTable("KDSP");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(100).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(50);
        b.Property(x => x.DataType).HasMaxLength(20);
        b.Property(x => x.Value).HasMaxLength(2000);
        b.Property(x => x.DefaultValue).HasMaxLength(2000);
        b.Property(x => x.MinValue).HasMaxLength(50);
        b.Property(x => x.MaxValue).HasMaxLength(50);
        b.Property(x => x.Options).HasMaxLength(2000);
        b.Property(x => x.Unit).HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
    }
}
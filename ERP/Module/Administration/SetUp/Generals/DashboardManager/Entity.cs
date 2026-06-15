using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.DashboardManager;

// ═══════════════════════════════════════════════════════════════
// DASHBOARD CONFIG  (KDSH)
// Manages named dashboard layouts. Each dashboard has a set
// of widgets (cards/charts/KPIs) with position & visibility.
//
// Flow:
//   DashboardConfig (1) ──► DashboardWidget (many)
//   Each widget maps to a React component key in the frontend.
// ═══════════════════════════════════════════════════════════════
public class DashboardConfig : AuditableEntity
{
    public string  Name        { get; set; } = null!;  // e.g. "Main Dashboard"
    public string? Description { get; set; }
    public string  RoleTarget  { get; set; } = "All";  // All / Admin / Sales / Purchasing / Inventory
    public string  LayoutType  { get; set; } = "Grid"; // Grid / List
    public int     Columns     { get; set; } = 3;      // grid columns (1-4)
    public bool    IsDefault   { get; set; } = false;
    public bool    IsActive    { get; set; } = true;
    public string? ThemeColor  { get; set; }           // optional accent color hex
    public string? Remarks     { get; set; }

    public ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
}

// ═══════════════════════════════════════════════════════════════
// DASHBOARD WIDGET  (KDS1)
// Individual widget slot on a dashboard.
// WidgetKey maps to a React component (e.g. "WarehouseCard").
// ═══════════════════════════════════════════════════════════════
public class DashboardWidget
{
    public int     Id                { get; set; }
    public int     DashboardConfigId { get; set; }
    public int     Position          { get; set; }    // display order
    public string  WidgetKey         { get; set; } = null!; // e.g. "WarehouseCard"
    public string  Title             { get; set; } = null!; // display label
    public string  Module            { get; set; } = null!; // Inventory / Financials / Sales
    public string  WidgetType        { get; set; } = "Card"; // Card / Chart / Table / KPI
    public string? Icon              { get; set; }    // emoji or icon name
    public string? Route             { get; set; }    // click-through route
    public int     ColSpan           { get; set; } = 1;   // grid column span (1-4)
    public bool    IsVisible         { get; set; } = true;
    public string? Settings          { get; set; }    // JSON config overrides

    public DashboardConfig? DashboardConfig { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class DashboardConfigEntityConfig
    : IEntityTypeConfiguration<DashboardConfig>
{
    public void Configure(EntityTypeBuilder<DashboardConfig> b)
    {
        b.ToTable("KDSH");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.RoleTarget).HasMaxLength(50);
        b.Property(x => x.LayoutType).HasMaxLength(20);
        b.Property(x => x.ThemeColor).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasMany(x => x.Widgets)
         .WithOne(x => x.DashboardConfig)
         .HasForeignKey(x => x.DashboardConfigId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DashboardWidgetEntityConfig
    : IEntityTypeConfiguration<DashboardWidget>
{
    public void Configure(EntityTypeBuilder<DashboardWidget> b)
    {
        b.ToTable("KDS1");
        b.HasKey(x => x.Id);

        b.Property(x => x.WidgetKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Module).HasMaxLength(100).IsRequired();
        b.Property(x => x.WidgetType).HasMaxLength(20);
        b.Property(x => x.Icon).HasMaxLength(50);
        b.Property(x => x.Route).HasMaxLength(300);
        b.Property(x => x.Settings).HasMaxLength(2000);
    }
}
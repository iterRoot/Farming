using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.Forecast;

// ═══════════════════════════════════════════════════════════════════
// FORECAST  (KFCS / FCS1)
// SAP B1 equivalent: MRP > Forecasts. A named demand forecast over a
// date range; each line is an expected quantity for an item on a date.
// Feeds MRP as planned demand alongside open sales orders (the MRP run
// here reads sales orders today; forecasts are the master that lets a
// planner add demand that isn't an order yet).
// ═══════════════════════════════════════════════════════════════════
public class Forecast : AuditableEntity
{
    public string   Code       { get; set; } = null!;   // auto: FC-0001
    public string   Name       { get; set; } = null!;
    public string   PeriodType { get; set; } = "Monthly"; // Daily | Weekly | Monthly
    public DateTime StartDate  { get; set; }
    public DateTime EndDate    { get; set; }
    public bool     IsActive   { get; set; } = true;
    public string?  Remarks    { get; set; }

    public ICollection<ForecastLine> Lines { get; set; } = new List<ForecastLine>();
}

public class ForecastLine
{
    public int      Id           { get; set; }
    public int      ForecastId   { get; set; }
    public int      LineNum      { get; set; }
    public string   ItemCode     { get; set; } = null!;
    public string   ItemName     { get; set; } = null!;
    public DateTime ForecastDate { get; set; }
    public decimal  Quantity     { get; set; }

    public Forecast Forecast { get; set; } = null!;
}

public class ForecastConfig : IEntityTypeConfiguration<Forecast>
{
    public void Configure(EntityTypeBuilder<Forecast> b)
    {
        b.ToTable("KFCS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.PeriodType).HasMaxLength(15);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasMany(x => x.Lines).WithOne(l => l.Forecast)
         .HasForeignKey(l => l.ForecastId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ForecastLineConfig : IEntityTypeConfiguration<ForecastLine>
{
    public void Configure(EntityTypeBuilder<ForecastLine> b)
    {
        b.ToTable("FCS1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
    }
}

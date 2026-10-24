using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.PostingPeriods;

// One accounting period. Dates are DateOnly → Postgres `date`, so no
// timezone shifting (a period boundary is a calendar date, not an instant).
public class PostingPeriod : AuditableEntity
{
    public string    PeriodCode       { get; set; } = null!;
    public string?   PeriodName       { get; set; }
    public string    PeriodStatus     { get; set; } = "Unlocked";
    public DateOnly? PostingDateFrom  { get; set; }
    public DateOnly? PostingDateTo    { get; set; }
    public DateOnly? DueDateFrom      { get; set; }
    public DateOnly? DueDateTo        { get; set; }
    public DateOnly? DocumentDateFrom { get; set; }
    public DateOnly? DocumentDateTo   { get; set; }
}

// Singleton options shown beneath the period grid.
public class PostingPeriodSetting : AuditableEntity
{
    public bool CreateNextYearDueDates { get; set; }
    public bool AutoUpdateStatus       { get; set; }
    public int  DaysAfterNewPeriod     { get; set; }
}

public class PostingPeriodConfig : IEntityTypeConfiguration<PostingPeriod>
{
    public void Configure(EntityTypeBuilder<PostingPeriod> b)
    {
        b.ToTable("KPRD");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.PeriodCode).IsUnique();
        b.Property(x => x.PeriodCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.PeriodName).HasMaxLength(100);
        b.Property(x => x.PeriodStatus).HasMaxLength(30).IsRequired();
    }
}

public class PostingPeriodSettingConfig : IEntityTypeConfiguration<PostingPeriodSetting>
{
    public void Configure(EntityTypeBuilder<PostingPeriodSetting> b)
    {
        b.ToTable("KPRDS");
        b.HasKey(x => x.Id);
    }
}

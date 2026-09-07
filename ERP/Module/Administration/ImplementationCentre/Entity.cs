using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ImplementationCentre;

// A single task in the implementation project checklist (SAP B1 "Implementation
// Center"). Tasks are grouped by phase and tracked to completion.
public class ImplementationTask : AuditableEntity
{
    public string   Phase        { get; set; } = "Getting Started";
    public string   Title        { get; set; } = null!;
    public string?  Description  { get; set; }
    public string?  Responsible  { get; set; }
    public string   Status       { get; set; } = "Open";   // Open | In Process | Completed | Skipped
    public string   Priority     { get; set; } = "Medium"; // Low | Medium | High
    public DateOnly? PlannedDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string?  LinkPath     { get; set; }             // route to the relevant setup screen
    public string?  Notes        { get; set; }
    public int      SortOrder    { get; set; }
}

public class ImplementationTaskConfig : IEntityTypeConfiguration<ImplementationTask>
{
    public void Configure(EntityTypeBuilder<ImplementationTask> b)
    {
        b.ToTable("KIMP");
        b.HasKey(x => x.Id);
        b.Property(x => x.Phase).HasMaxLength(80).IsRequired();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Priority).HasMaxLength(10).IsRequired();
    }
}

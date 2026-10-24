using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.OpportunitySetup;

// Reusable master lists for the Opportunities module. The Opportunity
// entity itself (Modules.CRM.Opportunity) stores Stage/Reason/Competitor
// as free text per row — these are the shared picklists an admin curates
// under Administration › Set Up › Opportunities, named distinctly
// (…Setup / …Master) so they never collide with those per-row classes.

public class OpportunityStageSetup : AuditableEntity
{
    public string  Name        { get; set; } = null!;   // e.g. "Qualification"
    public int     SortOrder   { get; set; } = 0;        // pipeline position, lowest first
    public decimal? Probability { get; set; }            // 0–100, typical win % at this stage
    public bool    Active      { get; set; } = true;
}

public class OpportunityReasonSetup : AuditableEntity
{
    public string  Name   { get; set; } = null!;   // e.g. "Price too high"
    public string  Type   { get; set; } = "Lost";  // "Won" | "Lost"
    public bool    Active { get; set; } = true;
}

public class CompetitorMaster : AuditableEntity
{
    public string  Name       { get; set; } = null!;
    public string? Strengths  { get; set; }
    public string? Weaknesses { get; set; }
    public bool    Active     { get; set; } = true;
}

public class OpportunityStageSetupConfig : IEntityTypeConfiguration<OpportunityStageSetup>
{
    public void Configure(EntityTypeBuilder<OpportunityStageSetup> b)
    {
        b.ToTable("KOPPSTG");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Probability).HasColumnType("decimal(5,2)");
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public class OpportunityReasonSetupConfig : IEntityTypeConfiguration<OpportunityReasonSetup>
{
    public void Configure(EntityTypeBuilder<OpportunityReasonSetup> b)
    {
        b.ToTable("KOPPRSN");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Type).HasMaxLength(10).IsRequired();
    }
}

public class CompetitorMasterConfig : IEntityTypeConfiguration<CompetitorMaster>
{
    public void Configure(EntityTypeBuilder<CompetitorMaster> b)
    {
        b.ToTable("KOPPCMP");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Strengths).HasMaxLength(500);
        b.Property(x => x.Weaknesses).HasMaxLength(500);
        b.HasIndex(x => x.Name).IsUnique();
    }
}

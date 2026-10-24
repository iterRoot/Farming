using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.ProjectManagement.Project;

// Project Management project (table KPRM).
//
// Distinct from Financials/FinancialProjects (KPRJ), which is the plain
// project-code master used for cost tagging. This is the operational
// project document — type, lifecycle, owner, BP, and a set of nested grids
// (stages, subprojects, section rows, attachments) that are kept as one
// JSON document in `Details` rather than modelled as child tables.
public class Project : AuditableEntity
{
    public string    ProjectNo        { get; set; } = null!;
    public string    ProjectName      { get; set; } = null!;
    public string    ProjectType      { get; set; } = "External";   // External | Internal
    public string    Status           { get; set; } = "Started";    // Open | Started | On Hold | Closed | Cancelled
    public string?   Series           { get; set; }

    public DateTime? StartDate        { get; set; }
    public DateTime? DueDate          { get; set; }
    public DateTime? FinishedDate     { get; set; }

    public string?   BpCode           { get; set; }
    public string?   BpName           { get; set; }
    public string?   ContactPerson    { get; set; }
    public string?   Territory        { get; set; }
    public string?   SalesEmployee    { get; set; }
    public string?   Owner            { get; set; }
    public bool      WithSubprojects  { get; set; }

    public string?   FinancialProject { get; set; }
    public string?   RiskLevel        { get; set; }
    public string?   Industry         { get; set; }
    public string?   Comments         { get; set; }
    public string?   Remarks          { get; set; }

    public int       OpenActivities   { get; set; }
    public decimal   PercentComplete  { get; set; }

    // { subprojects:[], sections:{}, attachments:[] } — stages are real rows (KPRS)
    public string    Details          { get; set; } = "{}";

    public List<ProjectStage> Stages  { get; set; } = new();
}

// One stage / task of a project (table KPRS). Progress is per stage; the
// project's PercentComplete and OpenActivities are derived from these rows.
public class ProjectStage
{
    public int       Id             { get; set; }
    public int       ProjectId      { get; set; }
    public int       LineNo         { get; set; }
    public string    Stage          { get; set; } = null!;
    public string?   Task           { get; set; }
    public string?   Description    { get; set; }
    public string?   UniqueId       { get; set; }
    public string?   Assignee       { get; set; }
    public DateTime? StartDate      { get; set; }
    public DateTime? DueDate        { get; set; }
    public DateTime? FinishedDate   { get; set; }
    public decimal   PlannedCost    { get; set; }
    public decimal   EstimatedHours { get; set; }
    public decimal   PercentComplete { get; set; }   // 0–100; 100 exactly when finished

    public Project?  Project        { get; set; }
}

public class ProjectStageConfig : IEntityTypeConfiguration<ProjectStage>
{
    public void Configure(EntityTypeBuilder<ProjectStage> b)
    {
        b.ToTable("KPRS");
        b.HasKey(x => x.Id);
        b.Property(x => x.Stage).HasMaxLength(60).IsRequired();
        b.Property(x => x.Task).HasMaxLength(60);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.UniqueId).HasMaxLength(60);
        b.Property(x => x.Assignee).HasMaxLength(120);
        b.Property(x => x.PlannedCost).HasPrecision(18, 2);
        b.Property(x => x.EstimatedHours).HasPrecision(9, 2);
        b.Property(x => x.PercentComplete).HasPrecision(5, 2);
        b.HasIndex(x => new { x.ProjectId, x.LineNo }).IsUnique();
        b.HasOne(x => x.Project).WithMany(p => p.Stages).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProjectConfig : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("KPRM");
        b.HasKey(x => x.Id);

        b.Property(m => m.ProjectNo).HasMaxLength(30).IsRequired();
        b.HasIndex(m => m.ProjectNo).IsUnique();
        b.Property(m => m.ProjectName).HasMaxLength(200).IsRequired();
        b.Property(m => m.ProjectType).HasMaxLength(20).HasDefaultValue("External");
        b.Property(m => m.Status).HasMaxLength(20).HasDefaultValue("Started");
        b.Property(m => m.Series).HasMaxLength(40);
        b.Property(m => m.BpCode).HasMaxLength(40);
        b.Property(m => m.BpName).HasMaxLength(200);
        b.Property(m => m.ContactPerson).HasMaxLength(120);
        b.Property(m => m.Territory).HasMaxLength(80);
        b.Property(m => m.SalesEmployee).HasMaxLength(120);
        b.Property(m => m.Owner).HasMaxLength(120);
        b.Property(m => m.FinancialProject).HasMaxLength(60);
        b.Property(m => m.RiskLevel).HasMaxLength(20);
        b.Property(m => m.Industry).HasMaxLength(80);
        b.Property(m => m.Comments).HasMaxLength(2000);
        b.Property(m => m.Remarks).HasMaxLength(2000);
        b.Property(m => m.PercentComplete).HasPrecision(5, 2);
        b.Property(m => m.Details).HasColumnType("jsonb").HasDefaultValue("{}");

        b.HasIndex(m => m.Status);
        b.HasIndex(m => m.ProjectType);
    }
}

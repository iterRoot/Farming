using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SetUp.ProcessChecklistTemplate;

// ═══════════════════════════════════════════════════════════════
// PROCESS CHECKLIST TEMPLATE  (KCLT / CLT1)
// A reusable list of steps for a recurring process — month-end
// close, stock count, onboarding. Starting one creates a run.
// ═══════════════════════════════════════════════════════════════
public class ChecklistTemplate : AuditableEntity
{
    public string  Code           { get; set; } = null!;
    public string  Name           { get; set; } = null!;
    public string  Category       { get; set; } = "General";
    public string? Module         { get; set; }
    public string? SubCategory    { get; set; }
    public string? Description    { get; set; }
    public string  TriggerType    { get; set; } = "Manual";  // Manual | OnDocumentPost | OnMonthEnd | OnYearEnd | Scheduled | OnEvent
    public string? TriggerEvent   { get; set; }
    public string? LinkedDocType  { get; set; }
    public string? AssignedRole   { get; set; }
    public string? AssignedTo     { get; set; }
    public bool    RequireSignOff { get; set; }
    public string? SignOffRole    { get; set; }
    public string? Tags           { get; set; }               // comma-separated
    public string? IconName       { get; set; }
    public string? Version        { get; set; }
    public bool    IsActive       { get; set; } = true;
    public int     SortOrder      { get; set; }
    public string? Remarks        { get; set; }

    public List<ChecklistTemplateStep> Steps { get; set; } = new();
}

public class ChecklistTemplateStep
{
    public int     Id                 { get; set; }
    public int     ChecklistTemplateId { get; set; }
    public int     StepNo             { get; set; }
    public string  Title              { get; set; } = null!;
    public string? Description        { get; set; }
    public string  StepType           { get; set; } = "Action"; // Action | Approval | Review | Verification | Upload | Notification | Report
    public string? GroupName          { get; set; }
    public string? AssignedRole       { get; set; }
    public string? AssignedTo         { get; set; }
    public int     EstimatedMinutes   { get; set; }
    public bool    IsRequired         { get; set; } = true;
    public bool    RequiresAttachment { get; set; }
    public bool    RequiresNote       { get; set; }
    public int?    DependsOnStepNo    { get; set; }
    public string? LinkedRoute        { get; set; }
    public string? Instructions       { get; set; }
    public string? AcceptanceCriteria { get; set; }
}

// ═══════════════════════════════════════════════════════════════
// CHECKLIST RUN  (KCLR / CLR1)
// One execution of a template. Steps are copied in when the run
// starts, so editing the template later never rewrites history.
// ChecklistTemplateId is a plain id, not an FK: deleting a template
// keeps its finished runs.
// ═══════════════════════════════════════════════════════════════
public class ChecklistRun : AuditableEntity
{
    public string    RunNo               { get; set; } = null!;   // CLR-2026-00001
    public int       ChecklistTemplateId { get; set; }
    public string    TemplateCode        { get; set; } = null!;
    public string    Name                { get; set; } = null!;
    public string?   ContextRef          { get; set; }             // e.g. "January 2026"
    public string?   StartedBy           { get; set; }
    public DateTime? DueDate             { get; set; }
    public string?   Notes               { get; set; }
    public string    Status              { get; set; } = "InProgress"; // InProgress | Completed | Cancelled
    public DateTime  StartedAt           { get; set; }
    public DateTime? CompletedAt         { get; set; }
    public bool      RequireSignOff      { get; set; }
    public string?   SignOffRole         { get; set; }
    public string?   SignedOffBy         { get; set; }
    public DateTime? SignedOffAt         { get; set; }

    public List<ChecklistRunStep> StepResults { get; set; } = new();
}

public class ChecklistRunStep
{
    public int       Id              { get; set; }
    public int       ChecklistRunId  { get; set; }
    public int       StepNo          { get; set; }
    public string    Title           { get; set; } = null!;
    public string    StepType        { get; set; } = "Action";
    public string?   GroupName       { get; set; }
    public bool      IsRequired      { get; set; } = true;
    public int?      DependsOnStepNo { get; set; }
    public string?   LinkedRoute     { get; set; }
    public string    StepStatus      { get; set; } = "Pending"; // Pending | Done | Skipped | Failed
    public string?   CompletedBy     { get; set; }
    public DateTime? CompletedAt     { get; set; }
    public string?   Note            { get; set; }
}

public class ChecklistTemplateConfig : IEntityTypeConfiguration<ChecklistTemplate>
{
    public void Configure(EntityTypeBuilder<ChecklistTemplate> b)
    {
        b.ToTable("KCLT");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Category).HasMaxLength(30).IsRequired();
        b.Property(x => x.Module).HasMaxLength(30);
        b.Property(x => x.SubCategory).HasMaxLength(50);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.TriggerType).HasMaxLength(30).IsRequired();
        b.Property(x => x.TriggerEvent).HasMaxLength(100);
        b.Property(x => x.LinkedDocType).HasMaxLength(50);
        b.Property(x => x.AssignedRole).HasMaxLength(50);
        b.Property(x => x.AssignedTo).HasMaxLength(50);
        b.Property(x => x.SignOffRole).HasMaxLength(50);
        b.Property(x => x.Tags).HasMaxLength(300);
        b.Property(x => x.IconName).HasMaxLength(20);
        b.Property(x => x.Version).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => x.Code).IsUnique();
        b.HasMany(x => x.Steps).WithOne().HasForeignKey(s => s.ChecklistTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChecklistTemplateStepConfig : IEntityTypeConfiguration<ChecklistTemplateStep>
{
    public void Configure(EntityTypeBuilder<ChecklistTemplateStep> b)
    {
        b.ToTable("CLT1");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.StepType).HasMaxLength(30).IsRequired();
        b.Property(x => x.GroupName).HasMaxLength(80);
        b.Property(x => x.AssignedRole).HasMaxLength(50);
        b.Property(x => x.AssignedTo).HasMaxLength(50);
        b.Property(x => x.LinkedRoute).HasMaxLength(200);
        b.Property(x => x.Instructions).HasMaxLength(2000);
        b.Property(x => x.AcceptanceCriteria).HasMaxLength(1000);
    }
}

public class ChecklistRunConfig : IEntityTypeConfiguration<ChecklistRun>
{
    public void Configure(EntityTypeBuilder<ChecklistRun> b)
    {
        b.ToTable("KCLR");
        b.HasKey(x => x.Id);
        b.Property(x => x.RunNo).HasMaxLength(20).IsRequired();
        b.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ContextRef).HasMaxLength(100);
        b.Property(x => x.StartedBy).HasMaxLength(100);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.SignOffRole).HasMaxLength(50);
        b.Property(x => x.SignedOffBy).HasMaxLength(100);
        b.HasIndex(x => x.RunNo).IsUnique();
        b.HasIndex(x => x.ChecklistTemplateId);
        b.HasMany(x => x.StepResults).WithOne().HasForeignKey(s => s.ChecklistRunId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChecklistRunStepConfig : IEntityTypeConfiguration<ChecklistRunStep>
{
    public void Configure(EntityTypeBuilder<ChecklistRunStep> b)
    {
        b.ToTable("CLR1");
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.StepType).HasMaxLength(30).IsRequired();
        b.Property(x => x.GroupName).HasMaxLength(80);
        b.Property(x => x.LinkedRoute).HasMaxLength(200);
        b.Property(x => x.StepStatus).HasMaxLength(20).IsRequired();
        b.Property(x => x.CompletedBy).HasMaxLength(100);
        b.Property(x => x.Note).HasMaxLength(1000);
    }
}

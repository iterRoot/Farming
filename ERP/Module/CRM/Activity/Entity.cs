using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.Activity;

// ═══════════════════════════════════════════════════════════════
// ACTIVITY  (KACT)  ·  SAP B1 equivalent: OCLG / CLG1
// CRM activity / task / call / meeting log tied to a Business
// Partner, with resource scheduling, content notes, document
// linking, and file attachments.
// ═══════════════════════════════════════════════════════════════
public class Activity : AuditableEntity
{
    // ── Header ────────────────────────────────────────────────
    public int      ActivityNo       { get; set; }            // auto sequential display number
    public string   ActivityKind     { get; set; } = "Phone Call"; // Phone Call/Meeting/Task/Note/Email/Other
    public string   Category         { get; set; } = "General";    // "Type" field — General/Sales/Service/Support/Internal
    public string?  Subject          { get; set; }
    public string?  BPCode           { get; set; }
    public string?  BPName           { get; set; }
    public string?  ContactPerson    { get; set; }
    public string?  TelephoneNo      { get; set; }
    public string   AssignedToType   { get; set; } = "User";   // User/Sales Employee/Group
    public string?  AssignedToCode   { get; set; }             // e.g. "BIZ007"
    public string?  AssignedBy       { get; set; }             // auto / read-only, set by backend
    public bool     IsPersonal       { get; set; } = false;

    // ── General tab ───────────────────────────────────────────
    public string?  Remarks          { get; set; }
    public DateTime StartDateTime    { get; set; } = DateTime.UtcNow;
    public DateTime EndDateTime      { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public int      DurationMinutes  { get; set; } = 15;       // computed from Start/End
    public string   Priority         { get; set; } = "Normal"; // Low/Normal/High
    public string?  MeetingLocation  { get; set; }
    public string   Recurrence       { get; set; } = "None";   // None/Daily/Weekly/Monthly/Yearly
    public bool     ReminderEnabled  { get; set; } = false;
    public int      ReminderMinutesBefore { get; set; } = 15;
    public bool     IsInactive       { get; set; } = false;
    public bool     IsClosed         { get; set; } = false;
    public DateTime? ClosedAt        { get; set; }

    // ── Other Details tab (Resource) ────────────────────────────
    public string?  ResourceNo           { get; set; }
    public string?  ResourceActivityType { get; set; }
    public string?  CostItem             { get; set; }   // read-only / computed from resource
    public string?  FinancialProject     { get; set; }
    public string?  ProjectNo            { get; set; }   // read-only / derived
    public string?  SubprojectNo         { get; set; }   // read-only / derived
    public string?  Stage                { get; set; }

    // ── Content tab ───────────────────────────────────────────
    public string?  Content          { get; set; }

    // ── Linked Document tab ───────────────────────────────────
    public bool     LinkDraft            { get; set; } = false;
    public string?  DocumentType         { get; set; }
    public string?  DocumentNumber       { get; set; }
    public string?  SourceObjectType     { get; set; }   // read-only / derived
    public string?  SourceObjectNo       { get; set; }   // read-only / derived
    public bool     ShowDocsRelatedToBP  { get; set; } = true;
    public int?     PreviousActivityNo   { get; set; }    // set when created via "Follow Up"

    // Navigation
    public ICollection<ActivityAttachment> Attachments { get; set; }
        = new List<ActivityAttachment>();
}

// ─── Attachments tab grid (KACA) ────────────────────────────────
public class ActivityAttachment
{
    public int       Id              { get; set; }
    public int       ActivityId      { get; set; }
    public int       LineNum         { get; set; }
    public string?   TargetPath      { get; set; }
    public string?   FileName        { get; set; }
    public DateTime? AttachmentDate  { get; set; }

    public Activity? Activity { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class ActivityConfig : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> b)
    {
        b.ToTable("KACT");
        b.HasKey(x => x.Id);

        b.Property(x => x.ActivityKind).HasMaxLength(30);
        b.Property(x => x.Category).HasMaxLength(30);
        b.Property(x => x.Subject).HasMaxLength(200);
        b.Property(x => x.BPCode).HasMaxLength(50);
        b.Property(x => x.BPName).HasMaxLength(200);
        b.Property(x => x.ContactPerson).HasMaxLength(100);
        b.Property(x => x.TelephoneNo).HasMaxLength(50);
        b.Property(x => x.AssignedToType).HasMaxLength(20);
        b.Property(x => x.AssignedToCode).HasMaxLength(50);
        b.Property(x => x.AssignedBy).HasMaxLength(50);

        b.Property(x => x.Remarks).HasMaxLength(500);
        b.Property(x => x.Priority).HasMaxLength(10);
        b.Property(x => x.MeetingLocation).HasMaxLength(100);
        b.Property(x => x.Recurrence).HasMaxLength(20);

        b.Property(x => x.ResourceNo).HasMaxLength(50);
        b.Property(x => x.ResourceActivityType).HasMaxLength(50);
        b.Property(x => x.CostItem).HasMaxLength(50);
        b.Property(x => x.FinancialProject).HasMaxLength(100);
        b.Property(x => x.ProjectNo).HasMaxLength(50);
        b.Property(x => x.SubprojectNo).HasMaxLength(50);
        b.Property(x => x.Stage).HasMaxLength(50);

        b.Property(x => x.Content).HasMaxLength(4000);

        b.Property(x => x.DocumentType).HasMaxLength(50);
        b.Property(x => x.DocumentNumber).HasMaxLength(50);
        b.Property(x => x.SourceObjectType).HasMaxLength(50);
        b.Property(x => x.SourceObjectNo).HasMaxLength(50);

        b.HasIndex(x => x.ActivityNo).IsUnique();

        b.HasMany(x => x.Attachments)
         .WithOne(x => x.Activity)
         .HasForeignKey(x => x.ActivityId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ActivityAttachmentConfig : IEntityTypeConfiguration<ActivityAttachment>
{
    public void Configure(EntityTypeBuilder<ActivityAttachment> b)
    {
        b.ToTable("KACA");
        b.HasKey(x => x.Id);
        b.Property(x => x.TargetPath).HasMaxLength(500);
        b.Property(x => x.FileName).HasMaxLength(200);
    }
}
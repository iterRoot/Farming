// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;

// namespace FarmingApi.Modules.BusinessPartner.Activity;

// // ═══════════════════════════════════════════════════════════════════
// // ACTIVITY (KACT) — matches SAP B1 Activity dialog
// // Activity types: PhoneCall, Meeting, Task, Note, Email, Campaign
// // ═══════════════════════════════════════════════════════════════════
// public class Activity : AuditableEntity
// {
//     // ── Header ────────────────────────────────────────────────────
//     public int      Number       { get; set; }              // Auto-number
//     public string   ActivityType { get; set; } = "PhoneCall"; // PhoneCall|Meeting|Task|Note|Email|Campaign
//     public string   ActivityKind { get; set; } = "General";   // General|Other
//     public string?  Subject      { get; set; }
//     public bool     Personal     { get; set; } = false;

//     // ── Assignment ────────────────────────────────────────────────
//     public string?  AssignedTo   { get; set; }  // User code e.g. BIZ007
//     public string?  AssignedToName { get; set; }
//     public string?  AssignedBy   { get; set; }

//     // ── Business Partner ──────────────────────────────────────────
//     public string?  CardCode     { get; set; }  // BP Code
//     public string?  CardName     { get; set; }  // BP Name
//     public string?  ContactPerson{ get; set; }
//     public string?  Phone        { get; set; }  // Telephone No.

//     // ── General Tab ───────────────────────────────────────────────
//     public string?  Remarks      { get; set; }
//     public DateTime StartDate    { get; set; }
//     public TimeSpan StartTime    { get; set; }
//     public DateTime EndDate      { get; set; }
//     public TimeSpan EndTime      { get; set; }
//     public string?  Duration     { get; set; }  // e.g. "15 Minutes"
//     public string   Priority     { get; set; } = "Normal";   // Low|Normal|High
//     public string?  Location     { get; set; }  // Meeting Location
//     public string   Recurrence   { get; set; } = "None";     // None|Daily|Weekly|Monthly

//     // ── Reminder ──────────────────────────────────────────────────
//     public bool     HasReminder  { get; set; } = false;
//     public string?  ReminderTime { get; set; }  // e.g. "15 Minutes"

//     // ── Content Tab ───────────────────────────────────────────────
//     public string?  Notes        { get; set; }
//     public string?  Description  { get; set; }

//     // ── Linked Document ───────────────────────────────────────────
//     public string?  LinkedDocType  { get; set; }  // KARI, KPAI, ORDR etc.
//     public int?     LinkedDocEntry { get; set; }
//     public string?  LinkedDocNo    { get; set; }

//     // ── Status ────────────────────────────────────────────────────
//     public bool     Inactive     { get; set; } = false;
//     public bool     Closed       { get; set; } = false;
//     public DateTime? ClosedDate  { get; set; }

//     // ── Follow Up ─────────────────────────────────────────────────
//     public int?     FollowUpId   { get; set; }  // linked follow-up activity
// }

// // ═══════════════════════════════════════════════════════════════════
// // EF CONFIGURATION
// // ═══════════════════════════════════════════════════════════════════
// public class ActivityConfig : IEntityTypeConfiguration<Activity>
// {
//     public void Configure(EntityTypeBuilder<Activity> builder)
//     {
//         builder.ToTable("KACT");
//         builder.HasKey(x => x.Id);

//         builder.Property(m => m.Number).ValueGeneratedOnAdd();
//         builder.HasIndex(m => m.Number).IsUnique();

//         builder.Property(m => m.ActivityType).HasMaxLength(20).HasDefaultValue("PhoneCall");
//         builder.Property(m => m.ActivityKind).HasMaxLength(20).HasDefaultValue("General");
//         builder.Property(m => m.Subject).HasMaxLength(200);
//         builder.Property(m => m.AssignedTo).HasMaxLength(50);
//         builder.Property(m => m.AssignedToName).HasMaxLength(200);
//         builder.Property(m => m.AssignedBy).HasMaxLength(50);

//         builder.Property(m => m.CardCode).HasMaxLength(50);
//         builder.Property(m => m.CardName).HasMaxLength(200);
//         builder.Property(m => m.ContactPerson).HasMaxLength(200);
//         builder.Property(m => m.Phone).HasMaxLength(50);

//         builder.Property(m => m.Remarks).HasMaxLength(500);
//         builder.Property(m => m.Duration).HasMaxLength(50);
//         builder.Property(m => m.Priority).HasMaxLength(10).HasDefaultValue("Normal");
//         builder.Property(m => m.Location).HasMaxLength(200);
//         builder.Property(m => m.Recurrence).HasMaxLength(20).HasDefaultValue("None");

//         builder.Property(m => m.ReminderTime).HasMaxLength(50);
//         builder.Property(m => m.Notes).HasMaxLength(2000);
//         builder.Property(m => m.Description).HasMaxLength(2000);

//         builder.Property(m => m.LinkedDocType).HasMaxLength(10);
//         builder.Property(m => m.LinkedDocNo).HasMaxLength(50);

//         builder.HasIndex(m => m.CardCode);
//         builder.HasIndex(m => m.AssignedTo);
//         builder.HasIndex(m => m.StartDate);
//         builder.HasIndex(m => m.Closed);
//         builder.HasIndex(m => m.ActivityType);
//     }
// }
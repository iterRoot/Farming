namespace FarmingApi.Modules.CRM.Activity;

// ══════════════════ RESPONSE ══════════════════
public class ActivityResponse
{
    public int      Id                { get; set; }
    public int      ActivityNo        { get; set; }
    public string   ActivityKind      { get; set; } = null!;
    public string   Category          { get; set; } = null!;
    public string?  Subject           { get; set; }
    public string?  BPCode            { get; set; }
    public string?  BPName            { get; set; }
    public string?  ContactPerson     { get; set; }
    public string?  TelephoneNo       { get; set; }
    public string   AssignedToType    { get; set; } = null!;
    public string?  AssignedToCode    { get; set; }
    public string?  AssignedBy        { get; set; }
    public bool     IsPersonal        { get; set; }

    public string?  Remarks           { get; set; }
    public DateTime StartDateTime     { get; set; }
    public DateTime EndDateTime       { get; set; }
    public int      DurationMinutes   { get; set; }
    public string   DurationDisplay   { get; set; } = null!;  // "15 Minutes" / "1h 30m"
    public string   Priority          { get; set; } = null!;
    public string?  MeetingLocation   { get; set; }
    public string   Recurrence        { get; set; } = null!;
    public bool     ReminderEnabled   { get; set; }
    public int      ReminderMinutesBefore { get; set; }
    public bool     IsInactive        { get; set; }
    public bool     IsClosed          { get; set; }
    public DateTime? ClosedAt         { get; set; }
    public string   Status            { get; set; } = null!;  // Open/Closed/Inactive — derived

    public string?  ResourceNo            { get; set; }
    public string?  ResourceActivityType  { get; set; }
    public string?  CostItem              { get; set; }
    public string?  FinancialProject      { get; set; }
    public string?  ProjectNo             { get; set; }
    public string?  SubprojectNo          { get; set; }
    public string?  Stage                 { get; set; }

    public string?  Content           { get; set; }

    public bool     LinkDraft             { get; set; }
    public string?  DocumentType          { get; set; }
    public string?  DocumentNumber        { get; set; }
    public string?  SourceObjectType      { get; set; }
    public string?  SourceObjectNo        { get; set; }
    public bool     ShowDocsRelatedToBP   { get; set; }
    public int?     PreviousActivityNo    { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<ActivityAttachmentDto> Attachments { get; set; } = new();
}

public class ActivityAttachmentDto
{
    public int       Id             { get; set; }
    public int       LineNum        { get; set; }
    public string?   TargetPath     { get; set; }
    public string?   FileName       { get; set; }
    public DateTime? AttachmentDate { get; set; }
}

// ══════════════════ REQUEST ══════════════════
public class ActivityRequest
{
    public string   ActivityKind      { get; set; } = "Phone Call";
    public string   Category          { get; set; } = "General";
    public string?  Subject           { get; set; }
    public string?  BPCode            { get; set; }
    public string?  BPName            { get; set; }
    public string?  ContactPerson     { get; set; }
    public string?  TelephoneNo       { get; set; }
    public string   AssignedToType    { get; set; } = "User";
    public string?  AssignedToCode    { get; set; }
    public bool     IsPersonal        { get; set; }

    public string?  Remarks           { get; set; }
    public DateTime StartDateTime     { get; set; } = DateTime.UtcNow;
    public DateTime EndDateTime       { get; set; } = DateTime.UtcNow.AddMinutes(15);
    public string   Priority          { get; set; } = "Normal";
    public string?  MeetingLocation   { get; set; }
    public string   Recurrence        { get; set; } = "None";
    public bool     ReminderEnabled   { get; set; }
    public int      ReminderMinutesBefore { get; set; } = 15;
    public bool     IsInactive        { get; set; }
    public bool     IsClosed          { get; set; }

    public string?  ResourceNo            { get; set; }
    public string?  ResourceActivityType  { get; set; }
    public string?  FinancialProject      { get; set; }
    public string?  Stage                 { get; set; }

    public string?  Content           { get; set; }

    public bool     LinkDraft             { get; set; }
    public string?  DocumentType          { get; set; }
    public string?  DocumentNumber        { get; set; }
    public bool     ShowDocsRelatedToBP   { get; set; } = true;

    public List<ActivityAttachmentRequest> Attachments { get; set; } = new();
}

public class ActivityAttachmentRequest
{
    public int       LineNum        { get; set; }
    public string?   TargetPath     { get; set; }
    public string?   FileName       { get; set; }
    public DateTime? AttachmentDate { get; set; }
}

// Close / reopen
public class ActivityCloseRequest
{
    public bool Closed { get; set; } = true;
}

// Follow-up — creates a new activity linked to this one
public class FollowUpRequest
{
    public string   ActivityKind   { get; set; } = "Phone Call";
    public string?  Subject        { get; set; }
    public DateTime StartDateTime  { get; set; } = DateTime.UtcNow.AddDays(1);
    public DateTime EndDateTime    { get; set; } = DateTime.UtcNow.AddDays(1).AddMinutes(15);
    public string?  Remarks        { get; set; }
}

// Dashboard summary
public class ActivitySummary
{
    public int Total      { get; set; }
    public int Open       { get; set; }
    public int Closed     { get; set; }
    public int Overdue    { get; set; }   // open & EndDateTime < now
    public int Today      { get; set; }   // StartDateTime is today
    public Dictionary<string,int> ByKind { get; set; } = new();
}
// namespace FarmingApi.Modules.BusinessPartner.Activity;

// // ═══════════════════════════════════════════════════════════════════
// // RESPONSE
// // ═══════════════════════════════════════════════════════════════════
// public class ActivityResponse
// {
//     public int       Id             { get; set; }
//     public int       Number         { get; set; }
//     public string    ActivityType   { get; set; } = null!;
//     public string    ActivityKind   { get; set; } = null!;
//     public string?   Subject        { get; set; }
//     public bool      Personal       { get; set; }

//     // Assignment
//     public string?   AssignedTo     { get; set; }
//     public string?   AssignedToName { get; set; }
//     public string?   AssignedBy     { get; set; }

//     // Business Partner
//     public string?   CardCode       { get; set; }
//     public string?   CardName       { get; set; }
//     public string?   ContactPerson  { get; set; }
//     public string?   Phone          { get; set; }

//     // General Tab
//     public string?   Remarks        { get; set; }
//     public DateTime  StartDate      { get; set; }
//     public string    StartTime      { get; set; } = null!;   // "HH:mm"
//     public DateTime  EndDate        { get; set; }
//     public string    EndTime        { get; set; } = null!;   // "HH:mm"
//     public string?   Duration       { get; set; }
//     public string    Priority       { get; set; } = null!;
//     public string?   Location       { get; set; }
//     public string    Recurrence     { get; set; } = null!;

//     // Reminder
//     public bool      HasReminder    { get; set; }
//     public string?   ReminderTime   { get; set; }

//     // Content Tab
//     public string?   Notes          { get; set; }
//     public string?   Description    { get; set; }

//     // Linked Document
//     public string?   LinkedDocType  { get; set; }
//     public int?      LinkedDocEntry { get; set; }
//     public string?   LinkedDocNo    { get; set; }

//     // Status
//     public bool      Inactive       { get; set; }
//     public bool      Closed         { get; set; }
//     public DateTime? ClosedDate     { get; set; }
//     public int?      FollowUpId     { get; set; }

//     public DateTime  CreatedAt      { get; set; }
//     public DateTime? UpdatedAt      { get; set; }
// }

// // ═══════════════════════════════════════════════════════════════════
// // CREATE REQUEST
// // ═══════════════════════════════════════════════════════════════════
// public class ActivityCreateRequest
// {
//     public string    ActivityType   { get; set; } = "PhoneCall";
//     public string    ActivityKind   { get; set; } = "General";
//     public string?   Subject        { get; set; }
//     public bool      Personal       { get; set; } = false;

//     public string?   AssignedTo     { get; set; }
//     public string?   AssignedToName { get; set; }
//     public string?   AssignedBy     { get; set; }

//     public string?   CardCode       { get; set; }
//     public string?   CardName       { get; set; }
//     public string?   ContactPerson  { get; set; }
//     public string?   Phone          { get; set; }

//     public string?   Remarks        { get; set; }
//     public DateTime  StartDate      { get; set; }
//     public string    StartTime      { get; set; } = "00:00";  // "HH:mm"
//     public DateTime  EndDate        { get; set; }
//     public string    EndTime        { get; set; } = "00:00";
//     public string?   Duration       { get; set; }
//     public string    Priority       { get; set; } = "Normal";
//     public string?   Location       { get; set; }
//     public string    Recurrence     { get; set; } = "None";

//     public bool      HasReminder    { get; set; } = false;
//     public string?   ReminderTime   { get; set; }

//     public string?   Notes          { get; set; }
//     public string?   Description    { get; set; }

//     public string?   LinkedDocType  { get; set; }
//     public int?      LinkedDocEntry { get; set; }
//     public string?   LinkedDocNo    { get; set; }
// }

// // ═══════════════════════════════════════════════════════════════════
// // UPDATE REQUEST
// // ═══════════════════════════════════════════════════════════════════
// public class ActivityUpdateRequest
// {
//     public string    ActivityType   { get; set; } = "PhoneCall";
//     public string    ActivityKind   { get; set; } = "General";
//     public string?   Subject        { get; set; }
//     public bool      Personal       { get; set; } = false;

//     public string?   AssignedTo     { get; set; }
//     public string?   AssignedToName { get; set; }
//     public string?   AssignedBy     { get; set; }

//     public string?   CardCode       { get; set; }
//     public string?   CardName       { get; set; }
//     public string?   ContactPerson  { get; set; }
//     public string?   Phone          { get; set; }

//     public string?   Remarks        { get; set; }
//     public DateTime  StartDate      { get; set; }
//     public string    StartTime      { get; set; } = "00:00";
//     public DateTime  EndDate        { get; set; }
//     public string    EndTime        { get; set; } = "00:00";
//     public string?   Duration       { get; set; }
//     public string    Priority       { get; set; } = "Normal";
//     public string?   Location       { get; set; }
//     public string    Recurrence     { get; set; } = "None";

//     public bool      HasReminder    { get; set; } = false;
//     public string?   ReminderTime   { get; set; }

//     public string?   Notes          { get; set; }
//     public string?   Description    { get; set; }

//     public string?   LinkedDocType  { get; set; }
//     public int?      LinkedDocEntry { get; set; }
//     public string?   LinkedDocNo    { get; set; }

//     public bool      Inactive       { get; set; } = false;
//     public bool      Closed         { get; set; } = false;
// }
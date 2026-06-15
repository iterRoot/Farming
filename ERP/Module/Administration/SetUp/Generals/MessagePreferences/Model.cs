namespace FarmingApi.Modules.Administration.MessagePreferences;

public class MessagePreferenceResponse
{
    public int      Id              { get; set; }
    public string   UserCode        { get; set; } = null!;
    public string?  UserName        { get; set; }
    public string   EventKey        { get; set; } = null!;
    public string   EventName       { get; set; } = null!;
    public string   Category        { get; set; } = null!;
    public string   Module          { get; set; } = null!;
    public string   Priority        { get; set; } = null!;
    public string   Channel         { get; set; } = null!;
    public bool     IsEnabled       { get; set; }
    public string   Frequency       { get; set; } = null!;
    public string?  DigestTime      { get; set; }
    public int?     DigestDay       { get; set; }
    public string?  EmailOverride   { get; set; }
    public string?  PhoneOverride   { get; set; }
    public string?  WebhookUrl      { get; set; }
    public string?  WebhookSecret   { get; set; }
    public string?  SubjectTemplate { get; set; }
    public string?  BodyTemplate    { get; set; }
    public string?  ConditionFilter { get; set; }
    public int      SortOrder       { get; set; }
    public bool     IsActive        { get; set; }
    public string?  Remarks         { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
}

public class MessagePreferenceRequest
{
    public string   UserCode        { get; set; } = null!;
    public string?  UserName        { get; set; }
    public string   EventKey        { get; set; } = null!;
    public string   EventName       { get; set; } = null!;
    public string   Category        { get; set; } = "General";
    public string   Module          { get; set; } = null!;
    public string   Priority        { get; set; } = "Medium";
    public string   Channel         { get; set; } = "InApp";
    public bool     IsEnabled       { get; set; } = true;
    public string   Frequency       { get; set; } = "Immediate";
    public string?  DigestTime      { get; set; }
    public int?     DigestDay       { get; set; }
    public string?  EmailOverride   { get; set; }
    public string?  PhoneOverride   { get; set; }
    public string?  WebhookUrl      { get; set; }
    public string?  WebhookSecret   { get; set; }
    public string?  SubjectTemplate { get; set; }
    public string?  BodyTemplate    { get; set; }
    public string?  ConditionFilter { get; set; }
    public int      SortOrder       { get; set; } = 0;
    public bool     IsActive        { get; set; } = true;
    public string?  Remarks         { get; set; }
}

// Lightweight toggle for the settings-page bulk enable/disable
public class BulkToggleRequest
{
    public List<int>  Ids       { get; set; } = new();
    public bool       IsEnabled { get; set; }
}

// Copy all prefs from one user to another
public class CopyToUserRequest
{
    public string TargetUserCode { get; set; } = null!;
    public string? TargetUserName { get; set; }
    public bool   OverwriteExisting { get; set; } = false;
}
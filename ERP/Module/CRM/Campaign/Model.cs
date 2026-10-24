namespace FarmingApi.Modules.CRM.Campaign;

public static class CampaignOptions
{
    public static readonly string[] Types    = { "Email", "Event", "Print", "Social", "Phone", "Other" };
    public static readonly string[] Statuses = { "Planned", "Active", "Completed", "Cancelled" };
    public static readonly string[] TargetStatuses = { "NotContacted", "Contacted", "Responded", "Converted", "Declined" };
}

public class CampaignTargetResponse
{
    public int      Id            { get; set; }
    public string   BPCode        { get; set; } = null!;
    public string   BPName        { get; set; } = null!;
    public string   Status        { get; set; } = null!;
    public DateTime? ResponseDate { get; set; }
    public string?  ResponseNotes { get; set; }
}

public class CampaignResponse
{
    public int      Id              { get; set; }
    public string   CampaignNo      { get; set; } = null!;
    public string   CampaignName    { get; set; } = null!;
    public string   CampaignType    { get; set; } = null!;
    public string   Status          { get; set; } = null!;
    public DateTime StartDate       { get; set; }
    public DateTime? EndDate        { get; set; }
    public decimal  Budget          { get; set; }
    public decimal  ActualCost      { get; set; }
    public decimal  ExpectedRevenue { get; set; }
    public string?  Owner           { get; set; }
    public string?  Remarks         { get; set; }
    public int      TargetCount     { get; set; }
    public int      RespondedCount  { get; set; }
    public int      ConvertedCount  { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime? UpdatedAt      { get; set; }
    public List<CampaignTargetResponse> Targets { get; set; } = new();
}

public class CampaignTargetRequest
{
    public string BPCode { get; set; } = null!;
}

public class CampaignRequest
{
    public string   CampaignName    { get; set; } = null!;
    public string   CampaignType    { get; set; } = "Email";
    public string   Status          { get; set; } = "Planned";
    public DateTime StartDate       { get; set; }
    public DateTime? EndDate        { get; set; }
    public decimal  Budget          { get; set; }
    public decimal  ActualCost      { get; set; }
    public decimal  ExpectedRevenue { get; set; }
    public string?  Owner           { get; set; }
    public string?  Remarks         { get; set; }
    public List<CampaignTargetRequest> Targets { get; set; } = new();
}

public class CampaignTargetStatusRequest
{
    public string  Status        { get; set; } = null!;
    public string? ResponseNotes { get; set; }
}

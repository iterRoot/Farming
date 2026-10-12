namespace FarmingApi.Modules.CRM.OpportunitySetup;

public class OpportunityStageDto
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = null!;
    public int      SortOrder   { get; set; }
    public decimal? Probability { get; set; }
    public bool     Active      { get; set; } = true;
}

public class OpportunityReasonDto
{
    public int    Id     { get; set; }
    public string Name   { get; set; } = null!;
    public string Type   { get; set; } = "Lost";
    public bool   Active { get; set; } = true;
}

public class CompetitorDto
{
    public int     Id         { get; set; }
    public string  Name       { get; set; } = null!;
    public string? Strengths  { get; set; }
    public string? Weaknesses { get; set; }
    public bool    Active     { get; set; } = true;
}

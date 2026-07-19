namespace FarmingApi.Modules.CRM.Opportunity;

// ══════════════════ RESPONSE ══════════════════
public class OpportunityResponse
{
    public int      Id                  { get; set; }
    public string   OpportunityNo       { get; set; } = null!;
    public string   OpportunityName     { get; set; } = null!;
    public string   OpportunityType     { get; set; } = null!;
    public string   Status              { get; set; } = null!;

    public string?  BPCode              { get; set; }
    public string?  BPName              { get; set; }
    public string?  ContactPerson       { get; set; }
    public decimal  TotalAmountInvoiced { get; set; }
    public string?  BPTerritory         { get; set; }
    public string?  SalesEmployee       { get; set; }
    public string?  Owner               { get; set; }
    public bool     DisplayInSystemCurrency { get; set; }

    public DateTime StartDate           { get; set; }
    public DateTime? ClosingDate        { get; set; }
    public int      OpenActivities      { get; set; }
    public int      ClosingPercent      { get; set; }

    public int?     PredictedClosingIn    { get; set; }
    public string?  PredictedClosingUnit  { get; set; }
    public DateTime? PredictedClosingDate { get; set; }
    public decimal  PotentialAmount       { get; set; }
    public decimal  WeightedAmount        { get; set; }
    public decimal  GrossProfitPercent    { get; set; }
    public decimal  GrossProfitTotal      { get; set; }
    public string?  LevelOfInterest       { get; set; }

    public string?  BPChannelCode    { get; set; }
    public string?  BPChannelName    { get; set; }
    public string?  BPChannelContact { get; set; }
    public string?  BPProject        { get; set; }
    public string?  InformationSource{ get; set; }
    public string?  Industry         { get; set; }
    public string?  Remarks          { get; set; }

    public string?  DocumentType        { get; set; }
    public string?  DocumentNo          { get; set; }
    public bool     ShowDocsRelatedToBP { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<OpportunityInterestDto>   InterestRanges { get; set; } = new();
    public List<OpportunityStageDto>      Stages         { get; set; } = new();
    public List<OpportunityPartnerDto>    Partners       { get; set; } = new();
    public List<OpportunityCompetitorDto> Competitors    { get; set; } = new();
    public List<OpportunityReasonDto>     Reasons        { get; set; } = new();
    public List<OpportunityAttachmentDto> Attachments    { get; set; } = new();
}

public class OpportunityInterestDto
{
    public int     Id          { get; set; }
    public int     LineNum     { get; set; }
    public string? Description { get; set; }
    public bool    IsPrimary   { get; set; }
}

public class OpportunityStageDto
{
    public int       Id              { get; set; }
    public int       LineNum         { get; set; }
    public DateTime  StartDate       { get; set; }
    public DateTime? ClosingDate     { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   Stage           { get; set; }
    public decimal   Percent         { get; set; }
    public decimal   PotentialAmount { get; set; }
    public decimal   WeightedAmount  { get; set; }
    public bool      ShowBPsDocs     { get; set; }
    public string?   DocumentType    { get; set; }
    public string?   DocNo           { get; set; }
    public string?   Owner           { get; set; }
}

public class OpportunityPartnerDto
{
    public int     Id           { get; set; }
    public int     LineNum      { get; set; }
    public string? Name         { get; set; }
    public string? Relationship { get; set; }
    public string? RelatedBP    { get; set; }
    public string? Remarks      { get; set; }
}

public class OpportunityCompetitorDto
{
    public int     Id          { get; set; }
    public int     LineNum     { get; set; }
    public string? Name        { get; set; }
    public string? ThreatLevel { get; set; }
    public string? Remarks     { get; set; }
    public bool    Won         { get; set; }
}

public class OpportunityReasonDto
{
    public int     Id          { get; set; }
    public int     LineNum     { get; set; }
    public string? Description { get; set; }
}

public class OpportunityAttachmentDto
{
    public int       Id             { get; set; }
    public int       LineNum        { get; set; }
    public string?   TargetPath     { get; set; }
    public string?   FileName       { get; set; }
    public DateTime? AttachmentDate { get; set; }
    public string?   FreeText       { get; set; }
}

// ══════════════════ REQUEST ══════════════════
public class OpportunityRequest
{
    public string   OpportunityName     { get; set; } = null!;
    public string   OpportunityType     { get; set; } = "Sales";

    public string?  BPCode              { get; set; }
    public string?  BPName              { get; set; }
    public string?  ContactPerson       { get; set; }
    public string?  BPTerritory         { get; set; }
    public string?  SalesEmployee       { get; set; }
    public string?  Owner               { get; set; }
    public bool     DisplayInSystemCurrency { get; set; }

    public DateTime StartDate           { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ClosingDate        { get; set; }
    public int      OpenActivities      { get; set; }
    public int      ClosingPercent      { get; set; }

    public int?     PredictedClosingIn    { get; set; }
    public string?  PredictedClosingUnit  { get; set; } = "Days";
    public DateTime? PredictedClosingDate { get; set; }
    public decimal  PotentialAmount       { get; set; }
    public decimal  GrossProfitPercent    { get; set; }
    public string?  LevelOfInterest       { get; set; }

    public string?  BPChannelCode    { get; set; }
    public string?  BPChannelName    { get; set; }
    public string?  BPChannelContact { get; set; }
    public string?  BPProject        { get; set; }
    public string?  InformationSource{ get; set; }
    public string?  Industry         { get; set; }
    public string?  Remarks          { get; set; }

    public string?  DocumentType        { get; set; }
    public string?  DocumentNo          { get; set; }
    public bool     ShowDocsRelatedToBP { get; set; }

    public List<OpportunityInterestRequest>   InterestRanges { get; set; } = new();
    public List<OpportunityStageRequest>      Stages         { get; set; } = new();
    public List<OpportunityPartnerRequest>    Partners       { get; set; } = new();
    public List<OpportunityCompetitorRequest> Competitors    { get; set; } = new();
    public List<OpportunityReasonRequest>     Reasons        { get; set; } = new();
    public List<OpportunityAttachmentRequest> Attachments    { get; set; } = new();
}

public class OpportunityInterestRequest
{
    public int     LineNum     { get; set; }
    public string? Description { get; set; }
    public bool    IsPrimary   { get; set; }
}

public class OpportunityStageRequest
{
    public int       LineNum         { get; set; }
    public DateTime  StartDate       { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ClosingDate     { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   Stage           { get; set; }
    public decimal   Percent         { get; set; }
    public decimal   PotentialAmount { get; set; }
    public bool      ShowBPsDocs     { get; set; }
    public string?   DocumentType    { get; set; }
    public string?   DocNo           { get; set; }
    public string?   Owner           { get; set; }
}

public class OpportunityPartnerRequest
{
    public int     LineNum      { get; set; }
    public string? Name         { get; set; }
    public string? Relationship { get; set; }
    public string? RelatedBP    { get; set; }
    public string? Remarks      { get; set; }
}

public class OpportunityCompetitorRequest
{
    public int     LineNum     { get; set; }
    public string? Name        { get; set; }
    public string? ThreatLevel { get; set; }
    public string? Remarks     { get; set; }
    public bool    Won         { get; set; }
}

public class OpportunityReasonRequest
{
    public int     LineNum     { get; set; }
    public string? Description { get; set; }
}

public class OpportunityAttachmentRequest
{
    public int       LineNum        { get; set; }
    public string?   TargetPath     { get; set; }
    public string?   FileName       { get; set; }
    public DateTime? AttachmentDate { get; set; }
    public string?   FreeText       { get; set; }
}

// Status transition (Open / Won / Lost)
public class OpportunityStatusRequest
{
    public string Status { get; set; } = null!;
}

// Dashboard summary
public class OpportunitySummary
{
    public int     Total            { get; set; }
    public int     Open             { get; set; }
    public int     Won              { get; set; }
    public int     Lost             { get; set; }
    public decimal TotalPotential   { get; set; }
    public decimal TotalWeighted    { get; set; }
    public decimal WonValue         { get; set; }
}
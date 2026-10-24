namespace FarmingApi.Modules.Administration.ApprovalRequest;

public class ApprovalRequestResponse
{
    public int       Id                   { get; set; }
    public string    DocumentType         { get; set; } = "";
    public int       DocumentId           { get; set; }
    public string    DocNum               { get; set; } = "";
    public decimal   Total                { get; set; }
    public int       ApprovalTemplateId   { get; set; }
    public string    ApprovalTemplateName { get; set; } = "";
    public string    Status               { get; set; } = "";
    public int       RequestedBy          { get; set; }
    public string    RequestedByName      { get; set; } = "";
    public DateTime  RequestedAt          { get; set; }
    public int?      DecidedBy            { get; set; }
    public string?   DecidedByName        { get; set; }
    public DateTime? DecidedAt            { get; set; }
    public string?   Remarks              { get; set; }
}

// Body accepted by every document controller's {id}/Reject action.
public class ApprovalDecisionRequest
{
    public string? Remarks { get; set; }
}

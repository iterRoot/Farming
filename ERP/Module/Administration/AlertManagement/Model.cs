namespace FarmingApi.Modules.Administration.AlertManagement;

public class AlertResponse
{
    public int       Id             { get; set; }
    public string    Type           { get; set; } = "";
    public string    Title          { get; set; } = "";
    public string    Body           { get; set; } = "";
    public string?   Link           { get; set; }
    public bool      IsRead         { get; set; }
    public int?      SenderUserId   { get; set; }
    public string?   SenderName     { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public DateTime? ReadAt         { get; set; }
}

public class SendMessageRequest
{
    public int    RecipientUserId { get; set; }
    public string Title           { get; set; } = "";
    public string Body            { get; set; } = "";
}

public class RunChecksResponse
{
    public string Message { get; set; } = "";
    public int    Raised  { get; set; }
}

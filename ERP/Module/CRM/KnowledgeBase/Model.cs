namespace FarmingApi.Modules.CRM.KnowledgeBase;

public class KnowledgeBaseArticleRequest
{
    public string  Title    { get; set; } = null!;
    public string? Category { get; set; }
    public string? ItemCode { get; set; }
    public string? Symptom  { get; set; }
    public string? Cause    { get; set; }
    public string  Solution { get; set; } = null!;
    public string  Status   { get; set; } = "Draft";
    public string? Tags     { get; set; }
}

public class KnowledgeBaseArticleResponse
{
    public int       Id        { get; set; }
    public string    Code      { get; set; } = null!;
    public string    Title     { get; set; } = null!;
    public string?   Category  { get; set; }
    public string?   ItemCode  { get; set; }
    public string?   Symptom   { get; set; }
    public string?   Cause     { get; set; }
    public string    Solution  { get; set; } = null!;
    public string    Status    { get; set; } = "Draft";
    public string?   Tags      { get; set; }
    public int       ViewCount { get; set; }
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

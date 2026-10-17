namespace FarmingApi.Modules.Financials.PostingTemplate;

public class PostingTemplateLineRequest
{
    public string  AccountCode     { get; set; } = null!;
    public string  Side            { get; set; } = "D";   // "D" | "C"
    public decimal Percentage      { get; set; }
    public string? LineDescription { get; set; }
}

public class PostingTemplateRequest
{
    public string  Description { get; set; } = null!;
    public string? Remarks     { get; set; }
    public bool    IsActive    { get; set; } = true;
    public List<PostingTemplateLineRequest> Lines { get; set; } = new();
}

public class PostingTemplateLineResponse
{
    public int     Id              { get; set; }
    public int     LineNum         { get; set; }
    public string  AccountCode     { get; set; } = null!;
    public string  AccountName     { get; set; } = null!;
    public string  Side            { get; set; } = "D";
    public decimal Percentage      { get; set; }
    public string? LineDescription { get; set; }
}

public class PostingTemplateResponse
{
    public int       Id          { get; set; }
    public string    Code        { get; set; } = null!;
    public string    Description { get; set; } = null!;
    public string?   Remarks     { get; set; }
    public bool      IsActive    { get; set; }
    public decimal   DebitPct    { get; set; }   // sum of Debit-side percentages (convenience for the list)
    public decimal   CreditPct   { get; set; }   // sum of Credit-side percentages
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
    public List<PostingTemplateLineResponse> Lines { get; set; } = new();
}

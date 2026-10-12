namespace FarmingApi.Modules.SaleAR.Dunning;

// ── Wizard: candidates ──────────────────────────────────────────
public class DunningCandidateInvoice
{
    public int       InvoiceId   { get; set; }
    public string    DocNum      { get; set; } = null!;
    public DateTime? DueDate     { get; set; }
    public int       DaysOverdue { get; set; }
    public decimal   Balance     { get; set; }
}

public class DunningCandidate
{
    public string CustomerCode        { get; set; } = null!;
    public string CustomerName        { get; set; } = null!;
    public string DunningTermCode     { get; set; } = null!;
    public string DunningTermName     { get; set; } = null!;
    public int    RecommendedLevel    { get; set; }
    public string RecommendedText     { get; set; } = null!;
    public decimal TotalOverdue       { get; set; }
    public List<DunningCandidateInvoice> Invoices { get; set; } = new();
}

public class DunningCandidatesResponse
{
    public List<DunningCandidate> Candidates { get; set; } = new();
    public List<string> SkippedNoTerm { get; set; } = new();   // overdue customers with no dunning term assigned
    public List<string> SkippedBlocked { get; set; } = new();  // overdue customers with BlockDunningLetters set
}

// ── Wizard: run ─────────────────────────────────────────────────
public class DunningRunRequest
{
    public DateTime      RunDate       { get; set; } = DateTime.UtcNow.Date;
    public List<string>  CustomerCodes { get; set; } = new(); // subset of that day's candidates to actually letter
}

public class DunningRunResultLine
{
    public string  CustomerCode { get; set; } = null!;
    public string  CustomerName { get; set; } = null!;
    public bool    Success      { get; set; }
    public int?    LetterId     { get; set; }
    public string? LetterNo     { get; set; }
    public int?    Level        { get; set; }
    public decimal Amount       { get; set; }
    public string? Error        { get; set; }
}

public class DunningRunResponse
{
    public int LettersCreated { get; set; }
    public List<DunningRunResultLine> Results { get; set; } = new();
}

// ── Letter history ───────────────────────────────────────────────
public class DunningLetterLineResponse
{
    public int       InvoiceId   { get; set; }
    public string    DocNum      { get; set; } = null!;
    public DateTime? DueDate     { get; set; }
    public int       DaysOverdue { get; set; }
    public decimal   Balance     { get; set; }
}

public class DunningLetterResponse
{
    public int       Id              { get; set; }
    public string    LetterNo        { get; set; } = null!;
    public string    CustomerCode    { get; set; } = null!;
    public string    CustomerName    { get; set; } = null!;
    public string    DunningTermCode { get; set; } = null!;
    public int       Level           { get; set; }
    public string    LetterText      { get; set; } = null!;
    public DateTime  RunDate         { get; set; }
    public decimal   TotalOverdue    { get; set; }
    public string?   Remarks         { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public List<DunningLetterLineResponse> Lines { get; set; } = new();
}

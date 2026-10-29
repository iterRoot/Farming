namespace FarmingApi.Modules.Financials.PeriodEndClosing;

public class PeriodEndClosingResponse
{
    public int       Id               { get; set; }
    public string    DocNum           { get; set; } = null!;
    public DateTime  FromDate         { get; set; }
    public DateTime  ToDate           { get; set; }
    public DateTime  PostingDate      { get; set; }
    public string    RetainedEarnings { get; set; } = null!;
    public decimal   TotalRevenue     { get; set; }
    public decimal   TotalExpense     { get; set; }
    public decimal   NetProfit        { get; set; }
    public int       AccountCount     { get; set; }
    public string?   Remarks          { get; set; }
    public string    Status           { get; set; } = null!;
    public int?      JournalEntryId   { get; set; }
    public string?   JournalNo        { get; set; }
    public DateTime  CreatedAt        { get; set; }
    public List<PeriodEndClosingLineDto> Lines { get; set; } = new();
}

public class PeriodEndClosingLineDto
{
    public string  AccountCode { get; set; } = null!;
    public string  AccountName { get; set; } = null!;
    public string  AccountType { get; set; } = null!;
    public decimal Balance     { get; set; }
    public string  ClosedTo    { get; set; } = null!;
}

// What the run would do, before anything is posted.
public class PeriodEndClosingPreview
{
    public DateTime FromDate         { get; set; }
    public DateTime ToDate           { get; set; }
    public string?  RetainedEarnings { get; set; }
    public string?  RetainedEarningsName { get; set; }
    public decimal  TotalRevenue     { get; set; }
    public decimal  TotalExpense     { get; set; }
    public decimal  NetProfit        { get; set; }
    public bool     AlreadyClosed    { get; set; }
    public string?  ClosedBy         { get; set; }   // DocNum of the run that already closed this period
    public List<string> Warnings     { get; set; } = new();
    public List<PeriodEndClosingLineDto> Lines { get; set; } = new();
}

// Posting date is named PostingDate so PostingPeriodGuardFilter checks it.
public class PeriodEndClosingRequest
{
    public DateTime FromDate         { get; set; }
    public DateTime ToDate           { get; set; }
    public DateTime PostingDate      { get; set; }
    public string?  RetainedEarnings { get; set; }   // Defaults to GL Account Determination
    public string?  Remarks          { get; set; }
}

namespace FarmingApi.Modules.Financials.RecurringPosting;

public class RecurringPostingLineDto
{
    public int      LineNum     { get; set; }
    public string   AccountCode { get; set; } = "";
    public string?  AccountName { get; set; }
    public decimal  Debit       { get; set; }
    public decimal  Credit      { get; set; }
    public string?  LineMemo    { get; set; }
}

public class RecurringPostingRequest
{
    public string    Description { get; set; } = "";
    public string    Frequency   { get; set; } = "Monthly";
    public DateTime   StartDate  { get; set; }
    public DateTime?  ValidUntil { get; set; }
    public string     Status     { get; set; } = "Active";
    public string     Currency   { get; set; } = "USD";
    public string?    Reference  { get; set; }
    public string?    Memo       { get; set; }
    public List<RecurringPostingLineDto> Lines { get; set; } = new();
}

public class RecurringPostingUpdateRequest : RecurringPostingRequest { }

public class RecurringPostingResponse
{
    public int        Id            { get; set; }
    public string     Code          { get; set; } = "";
    public string     Description   { get; set; } = "";
    public string     Frequency     { get; set; } = "";
    public DateTime    StartDate     { get; set; }
    public DateTime    NextExecution { get; set; }
    public DateTime?   ValidUntil    { get; set; }
    public string      Status        { get; set; } = "";
    public string      Currency      { get; set; } = "";
    public string?     Reference     { get; set; }
    public string?     Memo          { get; set; }
    public int         TimesExecuted { get; set; }
    public decimal     TotalDebit    { get; set; }
    public decimal     TotalCredit   { get; set; }
    public DateTime    CreatedAt     { get; set; }
    public List<RecurringPostingLineDto> Lines { get; set; } = new();
}

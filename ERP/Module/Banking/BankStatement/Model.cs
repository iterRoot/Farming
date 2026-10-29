namespace FarmingApi.Modules.Banking.BankStatement;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE
// ═══════════════════════════════════════════════════════════════════
public class BankStatementResponse
{
    public int       Id             { get; set; }
    public string    StatementNo    { get; set; } = null!;
    public string    AccountCode    { get; set; } = null!;
    public string    AccountName    { get; set; } = null!;
    public string?   BankName       { get; set; }
    public string?   BankAccountNo  { get; set; }
    public string    Currency       { get; set; } = null!;
    public DateTime  StatementDate  { get; set; }
    public DateTime  FromDate       { get; set; }
    public DateTime  ToDate         { get; set; }
    public decimal   OpeningBalance { get; set; }
    public decimal   ClosingBalance { get; set; }
    public string    Status         { get; set; } = null!;
    public string?   Remarks        { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public DateTime? UpdatedAt      { get; set; }

    // Computed
    public decimal   TotalIn         { get; set; }
    public decimal   TotalOut        { get; set; }
    public decimal   ComputedClosing { get; set; }  // Opening + In − Out
    public decimal   Difference      { get; set; }  // ComputedClosing − ClosingBalance, 0 when the rows are complete
    public int       TotalLines      { get; set; }
    public int       MatchedLines    { get; set; }
    public int       PostedLines     { get; set; }
    public int       UnmatchedLines  { get; set; }

    public List<BankStatementLineResponse> Lines { get; set; } = new();
}

public class BankStatementLineResponse
{
    public int       Id             { get; set; }
    public int       LineNum        { get; set; }
    public DateTime  ValueDate      { get; set; }
    public string?   Reference      { get; set; }
    public string?   Description    { get; set; }
    public decimal   AmountIn       { get; set; }
    public decimal   AmountOut      { get; set; }
    public decimal   Balance        { get; set; }  // Running balance from the opening balance
    public string    Status         { get; set; } = null!;
    public int?      JournalEntryId { get; set; }
    public string?   JournalNo      { get; set; }
    public string?   Remarks        { get; set; }
}

// A journal entry that moved the statement's bank account and can be matched to a row
public class BankStatementCandidate
{
    public int      JournalEntryId { get; set; }
    public string   JournalNo      { get; set; } = null!;
    public DateTime TransDate      { get; set; }
    public string   Source         { get; set; } = null!;  // Incoming Payment, Deposit, Journal Entry…
    public string?  RefNo          { get; set; }
    public string?  Memo           { get; set; }
    public decimal  AmountIn       { get; set; }
    public decimal  AmountOut      { get; set; }
    public int      DaysApart      { get; set; }
    public bool     ReferenceMatch { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE / UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BankStatementRequest
{
    public string   AccountCode    { get; set; } = null!;  // Ignored on update
    public string?  BankName       { get; set; }
    public string?  BankAccountNo  { get; set; }
    public string   Currency       { get; set; } = "USD";
    public DateTime StatementDate  { get; set; }
    public DateTime FromDate       { get; set; }
    public DateTime ToDate         { get; set; }
    public decimal  OpeningBalance { get; set; }
    public decimal  ClosingBalance { get; set; }
    public string?  Remarks        { get; set; }
    public List<BankStatementLineRequest> Lines { get; set; } = new();
}

public class BankStatementLineRequest
{
    public int?     Id          { get; set; }  // Existing line on update; matched/posted lines are kept as they are
    public DateTime ValueDate   { get; set; }
    public string?  Reference   { get; set; }
    public string?  Description { get; set; }
    public decimal  AmountIn    { get; set; }
    public decimal  AmountOut   { get; set; }
    public string?  Remarks     { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// LINE ACTIONS
// ═══════════════════════════════════════════════════════════════════
public class BankStatementMatchRequest
{
    public int JournalEntryId { get; set; }
}

// Posting date is named PostingDate so PostingPeriodGuardFilter checks it
public class BankStatementPostRequest
{
    public DateTime PostingDate       { get; set; }
    public string   OffsetAccountCode { get; set; } = null!;  // e.g. bank charges expense, interest income
    public string?  Memo              { get; set; }
}

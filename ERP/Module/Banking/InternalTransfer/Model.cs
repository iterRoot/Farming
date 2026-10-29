namespace FarmingApi.Modules.Banking.InternalTransfer;

public class InternalTransferResponse
{
    public int       Id              { get; set; }
    public string    DocNum          { get; set; } = null!;
    public DateTime  PostingDate     { get; set; }
    public string    FromAccount     { get; set; } = null!;
    public string    FromAccountName { get; set; } = null!;
    public string    ToAccount       { get; set; } = null!;
    public string    ToAccountName   { get; set; } = null!;
    public decimal   Amount          { get; set; }
    public decimal   BankCharge      { get; set; }
    public string?   ChargeAccount   { get; set; }
    public string    Currency        { get; set; } = null!;
    public string?   Reference       { get; set; }
    public string?   Memo            { get; set; }
    public string    Status          { get; set; } = null!;
    public int?      JournalEntryId  { get; set; }
    public string?   JournalNo       { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public DateTime? UpdatedAt       { get; set; }
}

// Posting date is named PostingDate so PostingPeriodGuardFilter checks it.
public class InternalTransferRequest
{
    public DateTime PostingDate   { get; set; }
    public string   FromAccount   { get; set; } = null!;
    public string   ToAccount     { get; set; } = null!;
    public decimal  Amount        { get; set; }
    public decimal  BankCharge    { get; set; }
    public string?  ChargeAccount { get; set; }
    public string?  Currency      { get; set; }
    public string?  Reference     { get; set; }
    public string?  Memo          { get; set; }
}

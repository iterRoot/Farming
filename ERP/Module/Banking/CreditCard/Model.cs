namespace FarmingApi.Modules.Banking.CreditCard;

public class CreditCardSetupDto
{
    public int     Id                { get; set; }
    public string  Code              { get; set; } = null!;
    public string  Name              { get; set; } = null!;
    public string  ClearingAccount   { get; set; } = null!;
    public string? SettlementAccount { get; set; }
    public decimal FeePercent        { get; set; }
    public string? FeeAccount        { get; set; }
    public bool    IsActive          { get; set; } = true;

    // Read-only, filled on GET
    public decimal UnsettledAmount   { get; set; }
    public int     UnsettledCount    { get; set; }
}

// A card receipt waiting in the clearing account.
public class CreditCardCandidate
{
    public int      IncomingPaymentId { get; set; }
    public string   DocNum            { get; set; } = null!;
    public DateTime DocDate           { get; set; }
    public string   CustomerCode      { get; set; } = null!;
    public string?  CustomerName      { get; set; }
    public string?  Reference         { get; set; }
    public decimal  Amount            { get; set; }
}

public class CreditCardSettlementResponse
{
    public int       Id              { get; set; }
    public string    DocNum          { get; set; } = null!;
    public string    CardCode        { get; set; } = null!;
    public string?   CardName        { get; set; }
    public DateTime  PostingDate     { get; set; }
    public string    BankAccount     { get; set; } = null!;
    public string    ClearingAccount { get; set; } = null!;
    public decimal   GrossAmount     { get; set; }
    public decimal   FeeAmount       { get; set; }
    public decimal   NetAmount       { get; set; }
    public string?   FeeAccount      { get; set; }
    public string?   Reference       { get; set; }
    public string    Status          { get; set; } = null!;
    public int?      JournalEntryId  { get; set; }
    public string?   JournalNo       { get; set; }
    public List<CreditCardSettlementLineDto> Lines { get; set; } = new();
}

public class CreditCardSettlementLineDto
{
    public int     IncomingPaymentId { get; set; }
    public string  PaymentDocNum     { get; set; } = null!;
    public string? CustomerName      { get; set; }
    public decimal Amount            { get; set; }
}

// Posting date is named PostingDate so PostingPeriodGuardFilter checks it.
public class CreditCardSettlementRequest
{
    public string    CardCode    { get; set; } = null!;
    public DateTime  PostingDate { get; set; }
    public string?   BankAccount { get; set; }     // defaults to the card's settlement account
    public decimal?  FeeAmount   { get; set; }     // defaults to gross × fee %
    public string?   Reference   { get; set; }
    public List<int> PaymentIds  { get; set; } = new();
}

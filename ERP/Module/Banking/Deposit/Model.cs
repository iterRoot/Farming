namespace FarmingApi.Modules.Banking.Deposit;

public class DepositCandidateResponse
{
    public int      IncomingPaymentId { get; set; }
    public string   DocNum            { get; set; } = null!;
    public DateTime DocDate           { get; set; }
    public string   CardName          { get; set; } = null!;
    public string   PaymentMethod     { get; set; } = null!;
    public string?  CheckNumber       { get; set; }
    public string   HoldingAccount    { get; set; } = null!;
    public decimal  Amount            { get; set; }
    public string   Currency          { get; set; } = null!;
}

public class DepositLineResponse
{
    public int     Id                { get; set; }
    public int     LineNum           { get; set; }
    public int     IncomingPaymentId { get; set; }
    public string  PaymentDocNum     { get; set; } = null!;
    public string  CardName          { get; set; } = null!;
    public string  HoldingAccount    { get; set; } = null!;
    public decimal Amount            { get; set; }
}

public class DepositResponse
{
    public int      Id             { get; set; }
    public string   DocNum         { get; set; } = null!;
    public DateTime DepositDate    { get; set; }
    public string   BankAccount    { get; set; } = null!;
    public string?  Memo           { get; set; }
    public string   Status         { get; set; } = null!;
    public decimal  TotalAmount    { get; set; }
    public string   Currency       { get; set; } = null!;
    public int?     JournalEntryId { get; set; }
    public DateTime CreatedAt      { get; set; }
    public DateTime? UpdatedAt     { get; set; }
    public List<DepositLineResponse> Lines { get; set; } = new();
}

public class DepositRequest
{
    public DateTime    DepositDate       { get; set; }
    public string      BankAccount       { get; set; } = null!;
    public string?     Memo              { get; set; }
    public List<int>   IncomingPaymentIds{ get; set; } = new();
}

namespace FarmingApi.Modules.Banking.PaymentRun;

public class PaymentRunCriteria
{
    public string    BpType       { get; set; } = "Vendor"; // "Vendor" (pay AP) | "Customer" (collect AR)
    public DateTime? DueDateFrom  { get; set; }
    public DateTime? DueDateTo    { get; set; }
    public string?   BpCode       { get; set; }              // optional — limit to one BP
}

public class PaymentRunCandidateInvoice
{
    public int      InvoiceId { get; set; }
    public string   DocNum    { get; set; } = null!;
    public DateTime? DueDate  { get; set; }
    public decimal  Balance   { get; set; }
}

public class PaymentRunCandidate
{
    public string  CardCode { get; set; } = null!;
    public string  CardName { get; set; } = null!;
    public decimal TotalDue { get; set; }
    public List<PaymentRunCandidateInvoice> Invoices { get; set; } = new();
}

public class PaymentRunPreviewResponse
{
    public List<PaymentRunCandidate> Candidates { get; set; } = new();
}

public class PaymentRunExecuteRequest : PaymentRunCriteria
{
    public List<string> CardCodes     { get; set; } = new();  // which of the previewed BPs to actually pay
    public string        PaymentMethod{ get; set; } = "Transfer";
    public string?        BankAccount { get; set; }
    public DateTime        PaymentDate{ get; set; }
}

public class PaymentRunResultLine
{
    public string   CardCode { get; set; } = null!;
    public string   CardName { get; set; } = null!;
    public bool     Success  { get; set; }
    public string?  Error    { get; set; }
    public int?     PaymentId { get; set; }
    public string?  DocNum    { get; set; }
    public decimal  Amount    { get; set; }
    public int      InvoicesClosed { get; set; }
}

public class PaymentRunExecuteResponse
{
    public int     PaymentsCreated { get; set; }
    public decimal TotalAmount     { get; set; }
    public List<PaymentRunResultLine> Results { get; set; } = new();
}

namespace FarmingApi.Modules.Banking.CheckRegister;

public class CheckRegisterRow
{
    public string   Direction    { get; set; } = null!;  // "Incoming" | "Outgoing"
    public int      PaymentId    { get; set; }
    public string   DocNum       { get; set; } = null!;
    public DateTime DocDate      { get; set; }
    public string   CardCode     { get; set; } = null!;
    public string   CardName     { get; set; } = null!;
    public string?  CheckNumber  { get; set; }
    public DateTime? CheckDate   { get; set; }
    public string?  CheckStatus  { get; set; }
    public string?  BankAccount  { get; set; }
    public string?  BankName     { get; set; }
    public decimal  Amount       { get; set; }
    public string   Currency     { get; set; } = null!;
    public string   PaymentStatus{ get; set; } = null!;   // the underlying payment's O/C/V
    public bool     Deposited    { get; set; }            // Incoming only; always false for Outgoing
}

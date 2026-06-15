namespace FarmingApi.Modules.Financials.GLAccountDetermination;

public class GLAccountDeterminationResponse
{
    public int     Id                 { get; set; }
    public string? AccountsReceivable { get; set; }
    public string? SalesRevenue       { get; set; }
    public string? SalesDiscount      { get; set; }
    public string? SalesTax           { get; set; }
    public string? CustomerDeposit    { get; set; }
    public string? AccountsPayable    { get; set; }
    public string? PurchaseExpense    { get; set; }
    public string? PurchaseDiscount   { get; set; }
    public string? PurchaseTax        { get; set; }
    public string? VendorDeposit      { get; set; }
    public string? InventoryAccount   { get; set; }
    public string? CostOfGoodsSold    { get; set; }
    public string? InventoryOffset    { get; set; }
    public string? CashAccount        { get; set; }
    public string? BankAccount        { get; set; }
    public string? ExchangeGainLoss   { get; set; }
    public string? RetainedEarnings   { get; set; }
    public string? OpeningBalance     { get; set; }
    public string? Notes              { get; set; }
    public DateTime UpdatedAt         { get; set; }
}

public class GLAccountDeterminationRequest
{
    public string? AccountsReceivable { get; set; }
    public string? SalesRevenue       { get; set; }
    public string? SalesDiscount      { get; set; }
    public string? SalesTax           { get; set; }
    public string? CustomerDeposit    { get; set; }
    public string? AccountsPayable    { get; set; }
    public string? PurchaseExpense    { get; set; }
    public string? PurchaseDiscount   { get; set; }
    public string? PurchaseTax        { get; set; }
    public string? VendorDeposit      { get; set; }
    public string? InventoryAccount   { get; set; }
    public string? CostOfGoodsSold    { get; set; }
    public string? InventoryOffset    { get; set; }
    public string? CashAccount        { get; set; }
    public string? BankAccount        { get; set; }
    public string? ExchangeGainLoss   { get; set; }
    public string? RetainedEarnings   { get; set; }
    public string? OpeningBalance     { get; set; }
    public string? Notes              { get; set; }
}
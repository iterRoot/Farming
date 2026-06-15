using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.GLAccountDetermination;

// ═══════════════════════════════════════════════════════════════
// GL ACCOUNT DETERMINATION (KGLD)
// Maps document types → Chart of Accounts
// SAP B1 equivalent: T030 / Account Determination setup
// Only ONE record — acts as a global settings table
// ═══════════════════════════════════════════════════════════════
public class GLAccountDetermination : AuditableEntity
{
    // ── Sale-AR ───────────────────────────────────────────────
    public string? AccountsReceivable { get; set; }  // Debit on AR Invoice   e.g. 1200
    public string? SalesRevenue       { get; set; }  // Credit on AR Invoice  e.g. 4000
    public string? SalesDiscount      { get; set; }  // Debit on discount     e.g. 4100
    public string? SalesTax           { get; set; }  // Credit on tax         e.g. 2300
    public string? CustomerDeposit    { get; set; }  // Down Payment received e.g. 2100

    // ── Purchase-AP ───────────────────────────────────────────
    public string? AccountsPayable    { get; set; }  // Credit on AP Invoice  e.g. 2000
    public string? PurchaseExpense    { get; set; }  // Debit on AP Invoice   e.g. 5000
    public string? PurchaseDiscount   { get; set; }  // Credit on discount    e.g. 5100
    public string? PurchaseTax        { get; set; }  // Debit on tax          e.g. 1300
    public string? VendorDeposit      { get; set; }  // Down Payment paid     e.g. 1400

    // ── Inventory ─────────────────────────────────────────────
    public string? InventoryAccount   { get; set; }  // Debit on GR           e.g. 1300
    public string? CostOfGoodsSold    { get; set; }  // Debit on GI           e.g. 5200
    public string? InventoryOffset    { get; set; }  // Credit on GR          e.g. 2000

    // ── Banking ───────────────────────────────────────────────
    public string? CashAccount        { get; set; }  // Debit on payment      e.g. 1000
    public string? BankAccount        { get; set; }  // Debit on bank         e.g. 1010
    public string? ExchangeGainLoss   { get; set; }  // FX differences        e.g. 6000

    // ── General ───────────────────────────────────────────────
    public string? RetainedEarnings   { get; set; }  // Year-end              e.g. 3100
    public string? OpeningBalance     { get; set; }  // Opening balance       e.g. 3000

    public string? Notes              { get; set; }
}

public class GLAccountDeterminationConfig
    : IEntityTypeConfiguration<GLAccountDetermination>
{
    public void Configure(EntityTypeBuilder<GLAccountDetermination> builder)
    {
        builder.ToTable("KGLD");
        builder.HasKey(x => x.Id);

        foreach (var p in new[]
        {
            nameof(GLAccountDetermination.AccountsReceivable),
            nameof(GLAccountDetermination.SalesRevenue),
            nameof(GLAccountDetermination.SalesDiscount),
            nameof(GLAccountDetermination.SalesTax),
            nameof(GLAccountDetermination.CustomerDeposit),
            nameof(GLAccountDetermination.AccountsPayable),
            nameof(GLAccountDetermination.PurchaseExpense),
            nameof(GLAccountDetermination.PurchaseDiscount),
            nameof(GLAccountDetermination.PurchaseTax),
            nameof(GLAccountDetermination.VendorDeposit),
            nameof(GLAccountDetermination.InventoryAccount),
            nameof(GLAccountDetermination.CostOfGoodsSold),
            nameof(GLAccountDetermination.InventoryOffset),
            nameof(GLAccountDetermination.CashAccount),
            nameof(GLAccountDetermination.BankAccount),
            nameof(GLAccountDetermination.ExchangeGainLoss),
            nameof(GLAccountDetermination.RetainedEarnings),
            nameof(GLAccountDetermination.OpeningBalance),
        })
        {
            builder.Property(p).HasMaxLength(50);
        }

        builder.Property(x => x.Notes).HasMaxLength(500);
    }
}
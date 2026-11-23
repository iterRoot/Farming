using KamnttraApi.Modules.Financials.GLAccountDetermination;
using KamnttraApi.Modules.Financials.JournalEntry;
using KamnttraApi.Modules.Inventory.ItemsMaster;
using KamnttraApi.Modules.InventoryManagement.InventoryJournal;

namespace KamnttraApi.Modules.PurchaseAP;

// ═══════════════════════════════════════════════════════════════
// PURCHASE LINE ACCOUNTS
//
// AP documents used to post their whole net amount to one account,
// Purchase Expense. That account can't be right for every line:
//   • a line copied from a Goods Receipt PO (or a Goods Return) has
//     already moved stock, and its value is sitting in GR/IR clearing
//     (Inventory Offset). The invoice has to clear that account.
//   • a stock item invoiced directly brings goods into stock, so its
//     value belongs in Inventory, not an expense.
//   • only services and non-stock items are a real purchase expense.
// This works out the account for each line, then turns the
// document's net amount into one journal amount per account.
// ═══════════════════════════════════════════════════════════════
public enum PurchaseLineRole
{
    /// <summary>Clears GR/IR (Inventory Offset): goods received or returned on another document.</summary>
    Clearing,
    /// <summary>Stock item whose goods move on this document: Inventory, at standard for standard-cost items.</summary>
    Inventory,
    /// <summary>Service or non-stock item: Purchase Expense.</summary>
    Expense,
}

/// <summary>One document line as the posting sees it.</summary>
public record PurchaseLine(string? ItemCode, decimal Quantity, decimal Price, decimal Total, PurchaseLineRole StockRole);

/// <summary>One amount to post. <c>Name</c> is the journal line's AccountName.</summary>
public record PurchaseAmount(string Name, string Account, decimal Amount);

public class PurchaseLineAccounts
{
    public const string ClearingName   = "GR/IR Clearing";
    public const string InventoryName  = "Inventory";
    public const string ExpenseName    = "Purchase Expense";
    public const string PriceDiffName  = "Price Difference";

    private readonly MyDbContext _db;
    private readonly GLAccountResolver _resolver;

    public PurchaseLineAccounts(MyDbContext db, GLAccountResolver resolver)
    {
        _db = db;
        _resolver = resolver;
    }

    /// <summary>
    /// True when the item is carried in stock. Service items (Item Type "Service")
    /// and items with "Stock Item" switched off are bought as an expense.
    /// </summary>
    public static bool IsStocked(ItemsMaster? item) =>
        item != null && item.StockItem && !string.Equals(item.ItemType, "Service", StringComparison.OrdinalIgnoreCase);

    public Dictionary<string, ItemsMaster> Items(IEnumerable<string?> itemCodes)
    {
        var codes = itemCodes.Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
        return _db.Set<ItemsMaster>().Where(i => codes.Contains(i.ItemCode)).ToDictionary(i => i.ItemCode);
    }

    /// <summary>
    /// The role a line plays. <paramref name="fromGoodsDocument"/> is true for a line copied from a
    /// Goods Receipt PO / Goods Return; <paramref name="stockRole"/> is what a stock item does on
    /// this document when it isn't: Inventory where the document moves the stock (AP Invoice),
    /// Clearing where the goods arrive later on a receipt (AP Reserve Invoice).
    /// </summary>
    public static PurchaseLineRole RoleOf(ItemsMaster? item, bool fromGoodsDocument, PurchaseLineRole stockRole)
    {
        if (fromGoodsDocument) return PurchaseLineRole.Clearing;
        return IsStocked(item) ? stockRole : PurchaseLineRole.Expense;
    }

    /// <summary>
    /// Splits <paramref name="net"/> (document total less tax, so header discounts are already in it)
    /// across the lines' roles in proportion to their line totals, then into accounts: GR/IR clearing,
    /// Inventory (per item-group override, with any standard-cost difference to Price Difference) and
    /// Purchase Expense (per item-group override). Throws when a needed account isn't set.
    /// </summary>
    public List<PurchaseAmount> Split(GLAccountDetermination gl, decimal net, IReadOnlyList<PurchaseLine> lines)
    {
        var weighted = lines.Select(l => (Line: l, Weight: l.Total != 0 ? l.Total : l.Quantity * l.Price))
                            .Where(x => x.Weight > 0).ToList();

        // Nothing to weigh by (no lines, or all zero): post it as before, to Purchase Expense.
        var shares = weighted.Count == 0
            ? new Dictionary<PurchaseLineRole, decimal> { [PurchaseLineRole.Expense] = net }
            : Allocate(net, weighted.GroupBy(x => x.Line.StockRole).ToDictionary(g => g.Key, g => g.Sum(x => x.Weight)));

        var result = new List<PurchaseAmount>();

        if (shares.TryGetValue(PurchaseLineRole.Clearing, out var clearing) && clearing != 0)
        {
            var code = Need(gl.InventoryOffset, "Inventory Offset (GR/IR clearing)", "lines copied from a goods receipt or return, or stock items on a reserve invoice");
            result.Add(new PurchaseAmount(ClearingName, code, clearing));
        }

        if (shares.TryGetValue(PurchaseLineRole.Inventory, out var inventory) && inventory != 0)
        {
            var code = Need(gl.InventoryAccount, "Inventory", "stock items invoiced without a goods receipt");
            var stockLines = weighted.Where(x => x.Line.StockRole == PurchaseLineRole.Inventory).ToList();

            // Standard-cost items are carried at their standard (the stock ledger does the same):
            // what was paid above or below it is a price difference, as on a Goods Receipt PO.
            var items = Items(stockLines.Select(x => x.Line.ItemCode));
            var variance = Math.Round(stockLines.Sum(x =>
                x.Line.ItemCode != null && items.TryGetValue(x.Line.ItemCode, out var it)
                && Valuation.Of(it.ValuationMethod) == Valuation.Standard && it.ItemCost > 0
                    ? (x.Line.Price - it.ItemCost) * x.Line.Quantity
                    : 0m), 2);
            if (variance != 0)
            {
                var diff = Need(gl.PriceDifference, "Price Difference", "standard-cost items invoiced without a goods receipt");
                result.Add(new PurchaseAmount(PriceDiffName, diff, variance));
            }

            foreach (var (account, amount) in _resolver.SplitByItemGroup("InventoryAccount", code, inventory - variance,
                         stockLines.Select(x => (x.Line.ItemCode, x.Weight))))
                result.Add(new PurchaseAmount(InventoryName, account, amount));
        }

        if (shares.TryGetValue(PurchaseLineRole.Expense, out var expense) && expense != 0)
        {
            var code = Need(gl.PurchaseExpense, "Purchase Expense", "services and non-stock items");
            var expenseLines = weighted.Where(x => x.Line.StockRole == PurchaseLineRole.Expense);
            foreach (var (account, amount) in _resolver.SplitByItemGroup("PurchaseExpense", code, expense,
                         expenseLines.Select(x => (x.Line.ItemCode, x.Weight))))
                result.Add(new PurchaseAmount(ExpenseName, account, amount));
        }

        return result.Where(r => r.Amount != 0).ToList();
    }

    /// <summary>
    /// Adds one journal line per amount, on the debit side (invoices) or the credit side (credit notes),
    /// numbering from <paramref name="lineNum"/>.
    /// </summary>
    public static void AddLines(JournalEntry je, IEnumerable<PurchaseAmount> amounts, bool debit, ref int lineNum, string memo)
    {
        foreach (var a in amounts)
        {
            // A negative price difference sits on the other side.
            var onDebit = a.Amount >= 0 ? debit : !debit;
            var amt = Math.Abs(a.Amount);
            je.Lines.Add(new JournalEntryLine
            {
                LineNum     = lineNum++,
                AccountCode = a.Account,
                AccountName = a.Name,
                CardCode    = null,
                CardName    = null,
                Debit       = onDebit ? amt : 0,
                Credit      = onDebit ? 0 : amt,
                DebitLC     = onDebit ? amt : 0,
                CreditLC    = onDebit ? 0 : amt,
                DebitFC     = 0,
                CreditFC    = 0,
                LineMemo    = $"{a.Name} — {memo}",
                TaxAmount   = 0,
                CreatedAt   = DateTime.UtcNow,
                InActive    = false,
            });
        }
    }

    /// <summary>Shares <paramref name="total"/> by weight; the rounding remainder goes to the largest share.</summary>
    private static Dictionary<PurchaseLineRole, decimal> Allocate(decimal total, Dictionary<PurchaseLineRole, decimal> weights)
    {
        var sum = weights.Values.Sum();
        var shares = weights.ToDictionary(w => w.Key, w => Math.Round(total * w.Value / sum, 2));
        var diff = total - shares.Values.Sum();
        if (diff != 0)
        {
            var largest = shares.OrderByDescending(s => s.Value).First().Key;
            shares[largest] += diff;
        }
        return shares;
    }

    private static string Need(string? code, string field, string why) =>
        !string.IsNullOrWhiteSpace(code)
            ? code.Trim()
            : throw new InvalidOperationException(
                $"The '{field}' account is not set in GL Account Determination (KGLD). It is needed for {why}.");
}

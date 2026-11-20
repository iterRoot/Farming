using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// STOPS A DOCUMENT FROM BEING SAVED WITH A TOTAL ITS LINES DON'T ADD UP TO
//
// The invoice screens work out the totals in the browser and send them. The server
// used to save and post whatever arrived, so an invoice for 100.00 could be saved
// as 1.00 and the journal entry booked 1.00. This checks the numbers on every
// save of an A/R or A/P invoice or credit memo, with the same arithmetic the
// screens use:
//
//     subtotal  = Σ quantity × price
//     tax       = (subtotal − discount) × tax% / 100
//     total     = subtotal − discount + tax
//
// A mismatch beyond a couple of cents is refused with a plain 400. It also refuses
// a discount above the subtotal, a tax rate outside 0–100%, and a line discount or
// VAT rate outside 0–100%. Drafts (Status "D") are not checked, and a save that
// doesn't touch the lines or the totals (closing a document, say) is left alone.
//
// Wired in from MyDbContext.SaveChanges, next to DocumentLedgerGuard.
// ═══════════════════════════════════════════════════════════════════════════
public static class DocumentTotalsGuard
{
    private static readonly Dictionary<string, string> Documents = new()
    {
        ["ARInvoice"]    = "A/R Invoice",
        ["ARCreditNote"] = "A/R Credit Memo",
        ["APInvoice"]    = "A/P Invoice",
        ["APCreditNote"] = "A/P Credit Memo",
    };

    private const decimal Tolerance = 0.02m;

    public static void Apply(DbContext db)
    {
        db.ChangeTracker.DetectChanges();

        foreach (var e in db.ChangeTracker.Entries().ToList())
        {
            if (!Documents.TryGetValue(e.Metadata.ClrType.Name, out var label)) continue;
            if (e.State is not (EntityState.Added or EntityState.Modified)) continue;
            if (e.State == EntityState.Modified && !TouchesAmounts(e)) continue;

            var status = e.Property("Status").CurrentValue as string;
            if (status is "D" or "X") continue;

            var items = (e.Entity.GetType().GetProperty("Items")?.GetValue(e.Entity) as IEnumerable)?.Cast<object>().ToList();
            if (items == null || (items.Count == 0 && e.State == EntityState.Modified)) continue;   // lines not loaded: nothing to compare

            var name = $"{label} {(e.Property("DocNum").CurrentValue as string is { Length: > 0 } n ? n : "")}".Trim();
            var discount = Dec(e.Entity, "Discount");
            var taxRate  = Dec(e.Entity, "Tax");
            var total    = Dec(e.Entity, "Total");

            decimal subtotal = 0;
            foreach (var line in items)
            {
                var qty = Dec(line, "Quantity"); var price = Dec(line, "Price");
                if (qty < 0 || price < 0)
                    throw new DocumentRuleException($"{name}: a line has a negative quantity or price. Use a credit memo or return to reverse a sale.");
                foreach (var pct in new[] { "Discount", "Vat" })
                {
                    var v = Dec(line, pct);
                    if (v < 0 || v > 100) throw new DocumentRuleException($"{name}: a line's {(pct == "Vat" ? "VAT" : "discount")} must be between 0% and 100%.");
                }
                subtotal += qty * price;
            }

            if (taxRate < 0 || taxRate > 100)
                throw new DocumentRuleException($"{name}: the tax rate must be between 0% and 100%.");
            if (discount < 0 || discount > subtotal)
                throw new DocumentRuleException($"{name}: the discount ({discount:0.##}) can't be negative or more than the lines add up to ({subtotal:0.##}).");

            var expectedTax   = (subtotal - discount) * taxRate / 100m;
            var expectedTotal = subtotal - discount + expectedTax;
            if (Math.Abs(total - expectedTotal) > Tolerance)
                throw new DocumentRuleException(
                    $"{name}: the total ({total:0.00}) doesn't match its lines ({expectedTotal:0.00}). " +
                    "Quantity × price for every line, less the discount, plus tax, must equal the total.");
            var taxAmount = Dec(e.Entity, "TaxAmount");
            if (Math.Abs(taxAmount - expectedTax) > Tolerance)
                throw new DocumentRuleException($"{name}: the tax amount ({taxAmount:0.00}) doesn't match the tax rate ({expectedTax:0.00}).");
        }
    }

    // On an update, only check when the amounts or the lines are part of this save.
    private static bool TouchesAmounts(EntityEntry e) =>
        e.Properties.Any(p => p.IsModified && p.Metadata.Name is "Total" or "Discount" or "Tax" or "TaxAmount")
        || e.Collections.Any(c => c.Metadata.Name == "Items" && c.IsModified)
        || e.Context.ChangeTracker.Entries().Any(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
               && x.Metadata.ClrType.Name.EndsWith("Line") && x.Metadata.ClrType.Name.StartsWith(e.Metadata.ClrType.Name));

    private static decimal Dec(object o, string prop) =>
        o.GetType().GetProperty(prop)?.GetValue(o) is decimal d ? d : 0m;
}

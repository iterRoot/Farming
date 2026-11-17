using Microsoft.EntityFrameworkCore;
using Je          = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using StockLedger = FarmingApi.Modules.InventoryManagement.InventoryJournal.InventoryJournal;

namespace FarmingApi.Core;

// A rule a request broke. DocumentRuleFilter turns it into a plain 400 carrying the message —
// as a filter rather than in the error middleware so it behaves the same in every environment
// (in Development the developer error page would otherwise answer 500 with a stack trace).
public class DocumentRuleException : Exception
{
    public DocumentRuleException(string message) : base(message) { }
}

public class DocumentRuleFilter : Microsoft.AspNetCore.Mvc.Filters.IExceptionFilter
{
    public void OnException(Microsoft.AspNetCore.Mvc.Filters.ExceptionContext context)
    {
        if (context.Exception is not DocumentRuleException ex) return;
        context.Result = new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(ex.Message);
        context.ExceptionHandled = true;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// KEEPS THE BOOKS HONEST WHEN A POSTED DOCUMENT GOES AWAY
//
// Posting an invoice, credit memo, delivery, receipt, return or payment writes
// an automatic journal entry (and, for stock documents, stock-ledger rows).
// Deleting that document — or voiding a payment — used to leave all of that
// behind: the entry stayed Open and its amounts stayed in the accounts, with
// nothing in the document list to explain them.
//
// Wired in centrally, from MyDbContext.SaveChanges, next to GLBalanceSync and
// for the same reason: it reads what THIS save is about to do, so every delete
// and void path is covered without editing each controller.
//
//   • Deleting a document voids its open journal entry in the same save;
//     GLBalanceSync then reverses the account balances (Open → Void).
//   • A document that has posted stock can't be deleted: putting stock back
//     correctly (moving-average cost, FIFO layers, batches) is not something a
//     delete can do, and silently leaving stock and books disagreeing is worse
//     than refusing. Reverse it with a return or a credit memo instead.
//   • If the entry was already closed, the delete is refused too — a closed
//     entry has to be reversed on its own first.
//   • Voiding a payment voids its journal entry.
// Draft documents have neither entry nor stock, so deleting them is unaffected.
// ═══════════════════════════════════════════════════════════════════════════
public static class DocumentLedgerGuard
{
    // Entity name → what it's called, and the journal TransType its automatic entry carries.
    // Payments are found through their JournalEntryId instead (their entry has no BaseDocEntry).
    private static readonly Dictionary<string, (string Label, string? TransType)> Documents = new()
    {
        ["ARInvoice"]            = ("A/R Invoice", "AR"),
        ["ARCreditNote"]         = ("A/R Credit Memo", "ACRN"),
        ["ARDownPaymentInvoice"] = ("A/R Down Payment Invoice", "ADPI"),
        ["ARReserveInvoice"]     = ("A/R Reserve Invoice", "ARRI"),
        ["Delivery"]             = ("Delivery", "DLN"),
        ["Return"]               = ("Return", null),
        ["APInvoice"]            = ("A/P Invoice", "APIN"),
        ["APCreditNote"]         = ("A/P Credit Memo", "APCN"),
        ["APDownPaymentInvoice"] = ("A/P Down Payment Invoice", "APDP"),
        ["APReserveInvoice"]     = ("A/P Reserve Invoice", "APRI"),
        ["GoodsReceiptPO"]       = ("Goods Receipt PO", "GRPO"),
        ["GoodsReturn"]          = ("Goods Return", "GRTN"),
        ["GoodsReceipt"]         = ("Goods Receipt", null),    // stock only: no journal entry, but its stock must not be orphaned
        ["GoodsIssue"]           = ("Goods Issue", null),
        ["IncomingPayment"]      = ("Incoming Payment", null),
        ["OutgoingPayment"]      = ("Outgoing Payment", null),
    };

    public static void Apply(MyDbContext db)
    {
        db.ChangeTracker.DetectChanges();

        var work = new List<(string Name, string Label, string? Trans, int Id, string? Num, int? LinkedJe, bool Deleting)>();
        foreach (var e in db.ChangeTracker.Entries())
        {
            if (!Documents.TryGetValue(e.Metadata.ClrType.Name, out var doc)) continue;

            var deleting = e.State == EntityState.Deleted;
            var voiding  = e.State == EntityState.Modified
                           && e.Metadata.FindProperty("Status") != null
                           && (string?)e.Property("Status").OriginalValue != "V"
                           && (string?)e.Property("Status").CurrentValue == "V";
            if (!deleting && !voiding) continue;

            var id     = ((Entity)e.Entity).Id;
            var num    = e.Metadata.FindProperty("DocNum") != null ? e.Property("DocNum").CurrentValue as string : null;
            var linked = e.Metadata.FindProperty("JournalEntryId") != null ? e.Property("JournalEntryId").CurrentValue as int? : null;
            work.Add((e.Metadata.ClrType.Name, doc.Label, doc.TransType, id, num, linked, deleting));
        }
        if (work.Count == 0) return;

        foreach (var w in work)
        {
            var name = $"{w.Label} {(string.IsNullOrWhiteSpace(w.Num) ? "#" + w.Id : w.Num)}";   // some documents are saved without a number

            if (w.Deleting)
            {
                var upper = w.Name.ToUpper();
                var hasStock = db.Set<StockLedger>().AsNoTracking()
                    .Any(s => s.BaseDocEntry == w.Id && s.BaseDocType != null && s.BaseDocType.ToUpper() == upper);
                if (hasStock)
                    throw new DocumentRuleException(
                        $"{name} has posted stock movements, so it can't be deleted — stock would not be put back. " +
                        "Reverse it with a return or credit memo instead.");
            }

            var entries = db.Set<Je>()
                .Where(j => j.Status != "V" &&
                            ((w.Trans != null && j.TransType == w.Trans && j.BaseDocEntry == w.Id) ||
                             (w.LinkedJe != null && j.Id == w.LinkedJe)))
                .ToList();

            if (w.Deleting && entries.Any(j => j.Status == "C"))
                throw new DocumentRuleException(
                    $"{name} can't be deleted: its journal entry {entries.First(j => j.Status == "C").JrnlNo} is closed. " +
                    "Reverse that entry first.");

            foreach (var j in entries)
            {
                if (j.Status != "O") continue;       // only an Open entry is reversed by Open → Void
                j.Status    = "V";
                j.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}

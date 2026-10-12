using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using FarmingApi.Modules.Financials.ChartOfAccounts;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.Financials.CostAccounting;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// Keeps ChartOfAccounts.Balance/DebitBalance/CreditBalance, and (when a line
// carries one) a CostCenter's own TotalDebit/TotalCredit/Balance, in step
// with the Journal Entries that actually move them. Before this, nothing in
// the codebase ever wrote to either — Trial Balance / Balance Sheet / P&L
// summed a ChartOfAccounts column stuck at its default of 0, and
// CostCenter.Balance carried a comment admitting as much: "Roll-ups,
// maintained by whatever posts to the centre (nothing yet)."
//
// A JournalEntry's Status ("O"=Open, "C"=Closed, "V"=Void — see Entity.cs)
// is GL-effective the moment it becomes "O": every *JournalService class in
// the app (18+ call sites — AR/AP Invoice, Credit Note, Down Payment,
// Reserve Invoice, GRPO, Goods Return, both Payment types, plus the manual
// JournalEntryController itself) constructs an already-"O" entry directly;
// there's no separate Draft state for the JE itself. "C" (Closed, via the
// existing /Post action) is a later locking step with no further GL impact
// of its own. "V" (Void) reverses the original posting.
//
// Wired in centrally — MyDbContext.SaveChanges/SaveChangesAsync calls this
// once before persisting — rather than edited into each of those 18+ call
// sites, the same "one central piece of wiring" approach already used for
// SubjectAuthorizationFilter and PostingPeriodGuardFilter. It reads
// whatever JournalEntry/JournalEntryLine changes are already in THIS
// SaveChanges call's ChangeTracker and folds the resulting Chart of
// Accounts deltas into the same atomic write — no second transaction.
// ═══════════════════════════════════════════════════════════════════════════
public static class GLBalanceSync
{
    public static void ApplyPendingDeltas(MyDbContext db)
    {
        db.ChangeTracker.DetectChanges();

        // Raw gross debit/credit deltas only — NOT yet netted into a signed
        // Balance, because that requires knowing each account's normal side
        // (debit-normal Assets/Expenses vs. credit-normal Liabilities/
        // Equity/Revenue), which we only learn once the affected accounts
        // are loaded below.
        var deltas = new Dictionary<string, (decimal Debit, decimal Credit)>();
        var costCenterDeltas = new Dictionary<string, (decimal Debit, decimal Credit)>();

        void Add(string? code, decimal debit, decimal credit, int sign)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            deltas.TryGetValue(code, out var cur);
            cur.Debit  += sign * debit;
            cur.Credit += sign * credit;
            deltas[code] = cur;
        }

        void AddCostCenter(string? code, decimal debit, decimal credit, int sign)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            costCenterDeltas.TryGetValue(code, out var cur);
            cur.Debit  += sign * debit;
            cur.Credit += sign * credit;
            costCenterDeltas[code] = cur;
        }

        // ── 1. Lines added/removed this call ────────────────────────────
        // Covers: a new GL-effective entry being created (Create, AutoCreate,
        // Reverse, or any of the 18+ *JournalService classes), and an
        // Update that clears and re-adds an existing Open entry's lines
        // (old lines come back as State.Deleted, new ones as State.Added —
        // both resolved against the SAME tracked parent, so an edit nets to
        // "reverse the old amounts, apply the new ones").
        foreach (var lineEntry in db.ChangeTracker.Entries<JournalEntryLine>())
        {
            if (lineEntry.State is not (EntityState.Added or EntityState.Deleted)) continue;

            var line = lineEntry.Entity;
            var status = ResolveParentStatus(db, lineEntry);
            if (status != "O") continue;

            var sign = lineEntry.State == EntityState.Added ? +1 : -1;
            Add(line.AccountCode, line.Debit, line.Credit, sign);
            AddCostCenter(line.CostCenter, line.Debit, line.Credit, sign);
        }

        // ── 2. Journal Entry header transitions ─────────────────────────
        foreach (var jeEntry in db.ChangeTracker.Entries<JournalEntry>())
        {
            if (jeEntry.State == EntityState.Modified)
            {
                var statusProp = jeEntry.Property(e => e.Status);
                var oldStatus = (string?)statusProp.OriginalValue;
                var newStatus = (string?)statusProp.CurrentValue;
                if (oldStatus == newStatus) continue;

                // O -> V (Void): the original posting is reversed. Void's
                // controller action doesn't load .Lines, so they aren't in
                // the tracker — read them straight from the database.
                if (oldStatus == "O" && newStatus == "V")
                    foreach (var l in LoadLines(db, jeEntry.Entity.Id))
                    {
                        Add(l.AccountCode, l.Debit, l.Credit, -1);
                        AddCostCenter(l.CostCenter, l.Debit, l.Credit, -1);
                    }

                // O -> C (Post/Close) is a locking step only — the GL
                // impact already happened when the entry became "O".
            }
            else if (jeEntry.State == EntityState.Deleted)
            {
                // Delete is only reachable via the API while Status == "O"
                // (Closed entries must be Voided instead) — but check
                // defensively rather than assume the caller enforced it,
                // and never double-reverse an already-Voided entry.
                if (jeEntry.Entity.Status == "O")
                    foreach (var l in LoadLines(db, jeEntry.Entity.Id))
                    {
                        Add(l.AccountCode, l.Debit, l.Credit, -1);
                        AddCostCenter(l.CostCenter, l.Debit, l.Credit, -1);
                    }
            }
        }

        if (costCenterDeltas.Count > 0)
        {
            var ccCodes = costCenterDeltas.Keys.ToList();
            var centers = db.Set<CostCenter>()
                .Where(c => ccCodes.Contains(c.CostCenterCode))
                .ToList();

            foreach (var cc in centers)
            {
                if (!costCenterDeltas.TryGetValue(cc.CostCenterCode, out var d)) continue;

                // Cost centers have no debit/credit-normal distinction the
                // way a chart-of-accounts type does — they're a single
                // spend dimension, so net debit-credit is the natural
                // "cost so far" figure.
                cc.TotalDebit  += d.Debit;
                cc.TotalCredit += d.Credit;
                cc.Balance     += d.Debit - d.Credit;
                cc.UpdatedAt    = DateTime.UtcNow;
            }
        }

        if (deltas.Count == 0) return;

        var codes = deltas.Keys.ToList();
        var accounts = db.Set<ChartOfAccounts>()
            .Where(a => codes.Contains(a.AcctCode))
            .ToList();

        foreach (var acct in accounts)
        {
            if (!deltas.TryGetValue(acct.AcctCode, out var d)) continue;

            // DebitBalance/CreditBalance are gross totals, sign-agnostic —
            // what Trial Balance sums to check Dr = Cr. Balance is netted
            // in the account's OWN normal-balance direction, matching how
            // the Balance Sheet / P&L tiles already read it (Assets show
            // positive when debited; Liabilities/Equity/Revenue show
            // positive when CREDITED — otherwise Assets = Liabilities +
            // Equity would never hold for a real, balanced posting).
            var isDebitNormal = IsDebitNormal(acct.AcctType);
            var balanceDelta = isDebitNormal ? (d.Debit - d.Credit) : (d.Credit - d.Debit);

            acct.Balance       += balanceDelta;
            acct.DebitBalance  += d.Debit;
            acct.CreditBalance += d.Credit;
            acct.UpdatedAt      = DateTime.UtcNow;
        }
    }

    // A=Assets, C=CostOfSales, X=OperatingCosts, N=NonOperating,
    // T=TaxationExtraordinary are debit-normal (expense-like); L=Liabilities,
    // E=Capital&Reserves, I=Turnover are credit-normal — same grouping the
    // P&L tile already uses to sum C/X/N/T as "expenses" (Reports/Finacial/
    // PNL/PNL.jsx).
    private static bool IsDebitNormal(string acctType) => acctType is "A" or "C" or "X" or "N" or "T";

    // Prefers the tracked parent already in this SaveChanges call (correct
    // even for a brand-new entry whose Id hasn't been assigned yet); falls
    // back to a DB read for a line added/removed against an otherwise-
    // untouched existing header (shouldn't normally occur — a line never
    // changes without its header being touched too — but fail safe rather
    // than silently skip a real posting).
    private static string? ResolveParentStatus(MyDbContext db, EntityEntry<JournalEntryLine> lineEntry)
    {
        var parent = lineEntry.Reference(l => l.JournalEntry).TargetEntry;
        if (parent != null) return (string?)parent.CurrentValues["Status"];

        return db.Set<JournalEntry>()
            .Where(j => j.Id == lineEntry.Entity.JournalEntryId)
            .Select(j => j.Status)
            .FirstOrDefault();
    }

    private static List<JournalEntryLine> LoadLines(MyDbContext db, int journalEntryId)
        => db.Set<JournalEntryLine>().Where(l => l.JournalEntryId == journalEntryId).ToList();
}

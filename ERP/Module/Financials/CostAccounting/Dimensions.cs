using FarmingApi.Core;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using ProjectEntity    = FarmingApi.Modules.Financials.FinancialProjects.FinancialProject;

namespace FarmingApi.Modules.Financials.CostAccounting;

// ═══════════════════════════════════════════════════════════════════
// DIMENSIONS — the project and cost-centre tags on postings.
// A journal line's cost-centre field holds either a cost centre or a
// distribution rule. A rule splits the line across its centres by the
// rule's percentages (the last centre takes the rounding remainder), so
// every resulting line carries a real cost centre and rolls up to it.
// Codes must exist, be active, and be valid on the posting date; a
// header's project/cost centre is the default for lines that have none.
// ═══════════════════════════════════════════════════════════════════
public static class Dimensions
{
    private static bool Blank(string? s) => string.IsNullOrWhiteSpace(s);

    public static string? CheckProject(MyDbContext db, string? code, DateTime date, out string? canonical)
    {
        canonical = null;
        if (Blank(code)) return null;
        var c = code!.Trim().ToUpperInvariant();
        var p = db.Set<ProjectEntity>().FirstOrDefault(x => x.ProjectCode.ToUpper() == c);
        if (p == null) return $"Project '{code.Trim()}' doesn't exist.";
        if (p.InActive == true) return $"Project '{p.ProjectCode}' is inactive.";
        var d = DateOnly.FromDateTime(date);
        if (p.ValidFrom != null && d < p.ValidFrom) return $"Project '{p.ProjectCode}' isn't valid until {p.ValidFrom:yyyy-MM-dd}.";
        if (p.ValidTo != null && d > p.ValidTo) return $"Project '{p.ProjectCode}' expired on {p.ValidTo:yyyy-MM-dd}.";
        canonical = p.ProjectCode;
        return null;
    }

    public static string? CheckCostCenter(MyDbContext db, string? code, DateTime date, out CostCenter? center)
    {
        center = null;
        if (Blank(code)) return null;
        var c = code!.Trim().ToUpperInvariant();
        center = db.Set<CostCenter>().FirstOrDefault(x => x.CostCenterCode == c);
        if (center == null) return $"Cost centre '{code.Trim()}' doesn't exist.";
        if (!center.IsActive || center.InActive == true) return $"Cost centre '{center.CostCenterCode}' is inactive.";
        if (center.StartDate != null && date.Date < center.StartDate.Value.Date) return $"Cost centre '{center.CostCenterCode}' isn't valid until {center.StartDate:yyyy-MM-dd}.";
        if (center.EndDate != null && date.Date > center.EndDate.Value.Date) return $"Cost centre '{center.CostCenterCode}' ended on {center.EndDate:yyyy-MM-dd}.";
        return null;
    }

    // Validates a manual journal entry's dimensions and expands distribution rules. Returns an error, or null.
    public static string? ApplyToEntry(MyDbContext db, JournalEntryDoc entry)
    {
        var date = entry.TransDate;

        var err = CheckProject(db, entry.ProjectCode, date, out var headerProject);
        if (err != null) return err;
        entry.ProjectCode = headerProject;

        var headerCc = entry.CostCenter;
        if (!Blank(headerCc)) headerCc = headerCc!.Trim().ToUpperInvariant();
        entry.CostCenter = Blank(headerCc) ? null : headerCc;

        var expanded = new List<JournalEntryLine>();
        foreach (var line in entry.Lines.OrderBy(l => l.LineNum))
        {
            err = CheckProject(db, line.Project, date, out var project);
            if (err != null) return $"Line {line.LineNum}: {err}";
            line.Project = project ?? headerProject;

            var dim = Blank(line.CostCenter) ? entry.CostCenter : line.CostCenter!.Trim().ToUpperInvariant();
            if (Blank(dim)) { line.CostCenter = null; expanded.Add(line); continue; }

            var rule = db.Set<DistributionRule>().FirstOrDefault(r => r.RuleCode == dim);
            if (rule == null)
            {
                err = CheckCostCenter(db, dim, date, out var center);
                if (err != null) return $"Line {line.LineNum}: {err}";
                line.CostCenter = center!.CostCenterCode;
                expanded.Add(line);
                continue;
            }

            // A distribution rule: split the line by percentage across the rule's centres.
            if (!rule.IsActive || rule.InActive == true) return $"Line {line.LineNum}: distribution rule '{rule.RuleCode}' is inactive.";
            var ruleLines = db.Set<DistributionRuleLine>().Where(l => l.DistributionRuleId == rule.Id).OrderBy(l => l.LineNum).ToList();
            if (ruleLines.Count == 0) return $"Line {line.LineNum}: distribution rule '{rule.RuleCode}' has no lines.";
            foreach (var rl in ruleLines)
            {
                err = CheckCostCenter(db, rl.CostCenterCode, date, out _);
                if (err != null) return $"Line {line.LineNum}: rule '{rule.RuleCode}' — {err}";
            }
            expanded.AddRange(Split(line, ruleLines, rule.RuleCode));
            rule.InUse = true;
            rule.UpdatedAt = DateTime.UtcNow;
        }

        entry.Lines.Clear();
        var n = 1;
        foreach (var l in expanded) { l.LineNum = n++; entry.Lines.Add(l); }
        return null;
    }

    private static List<JournalEntryLine> Split(JournalEntryLine src, List<DistributionRuleLine> rule, string ruleCode)
    {
        var parts = new List<JournalEntryLine>();
        decimal debitLeft = src.Debit, creditLeft = src.Credit, taxLeft = src.TaxAmount, debitFcLeft = src.DebitFC, creditFcLeft = src.CreditFC;
        for (var i = 0; i < rule.Count; i++)
        {
            var last = i == rule.Count - 1;
            decimal Share(decimal total, ref decimal left)
            {
                var v = last ? left : Math.Round(total * rule[i].Percentage / 100m, 2);
                left -= v;
                return v;
            }
            var debit = Share(src.Debit, ref debitLeft);
            var credit = Share(src.Credit, ref creditLeft);
            parts.Add(new JournalEntryLine
            {
                AccountCode = src.AccountCode, AccountName = src.AccountName, CardCode = src.CardCode, CardName = src.CardName,
                Debit = debit, Credit = credit, DebitLC = debit, CreditLC = credit,
                DebitFC = Share(src.DebitFC, ref debitFcLeft), CreditFC = Share(src.CreditFC, ref creditFcLeft),
                LineMemo = $"{src.LineMemo}{(string.IsNullOrWhiteSpace(src.LineMemo) ? "" : " ")}[{ruleCode} {rule[i].Percentage:0.##}%]".Trim(),
                CostCenter = rule[i].CostCenterCode, Project = src.Project, TaxCode = src.TaxCode,
                TaxAmount = Share(src.TaxAmount, ref taxLeft), Reference = src.Reference, DueDate = src.DueDate,
                CreatedAt = DateTime.UtcNow, InActive = false,
            });
        }
        return parts;
    }
}

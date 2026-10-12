using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using BudgetLineDoc    = FarmingApi.Modules.Financials.Budget.BudgetLine;
using CoaEntity        = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;

namespace FarmingApi.Modules.Financials.FinancialProjects;

// Budget vs actual for one financial project — shared by the project setup
// screen and Project Management.
// Budget: lines tagged with the project on Approved or Closed budgets.
// Actual: journal lines tagged with it (or on a header tagged with it and no
// line tag), on entries that aren't void. "Actual" is the account's
// natural-side amount; variance is favourable when positive.
public static class ProjectSummaryBuilder
{
    public static ProjectSummaryResponse Build(MyDbContext db, FinancialProject project, int? year)
    {
        var code = project.ProjectCode.ToUpper();

        var budgetRows = db.Set<BudgetLineDoc>().Include(l => l.Budget)
            .Where(l => l.Project != null && l.Project.ToUpper() == code && (l.Budget.Status == "A" || l.Budget.Status == "C")
                        && (year == null || l.Budget.FiscalYear == year))
            .ToList();

        var entries = db.Set<JournalEntryDoc>().Include(j => j.Lines)
            .Where(j => j.Status != "V" && (year == null || j.TransDate.Year == year)
                        && (j.ProjectCode != null && j.ProjectCode.ToUpper() == code || j.Lines.Any(l => l.Project != null && l.Project.ToUpper() == code)))
            .ToList();
        var actualLines = entries.SelectMany(j => j.Lines.Where(l =>
            (l.Project != null && l.Project.ToUpper() == code) ||
            (string.IsNullOrEmpty(l.Project) && j.ProjectCode != null && j.ProjectCode.ToUpper() == code))).ToList();

        var accounts = db.Set<CoaEntity>().ToDictionary(a => a.AcctCode, a => (a.AcctName, a.AcctType));
        var codes = budgetRows.Select(b => b.AcctCode).Concat(actualLines.Select(l => l.AccountCode)).Distinct().OrderBy(c => c).ToList();

        var rows = codes.Select(ac =>
        {
            var name = accounts.TryGetValue(ac, out var a) ? a.AcctName : ac;
            var type = accounts.TryGetValue(ac, out var a2) ? a2.AcctType : "";
            var debit = actualLines.Where(l => l.AccountCode == ac).Sum(l => l.Debit);
            var credit = actualLines.Where(l => l.AccountCode == ac).Sum(l => l.Credit);
            var creditNormal = type is "L" or "E" or "I" || (type == "N" && credit > debit);
            var actual = creditNormal ? credit - debit : debit - credit;
            var budget = budgetRows.Where(b => b.AcctCode == ac).Sum(b => b.Annual);
            return new ProjectSummaryRow
            {
                AcctCode = ac, AcctName = name, AcctType = type, Budget = budget, Debit = debit, Credit = credit, Actual = actual,
                Variance = creditNormal ? actual - budget : budget - actual,
            };
        }).ToList();

        // Result = what came in less what went out, on P&L accounts only.
        decimal Sum(Func<ProjectSummaryRow, bool> f, Func<ProjectSummaryRow, decimal> v) => rows.Where(f).Sum(v);
        bool Income(ProjectSummaryRow r) => r.AcctType == "I";
        bool Cost(ProjectSummaryRow r) => r.AcctType is "C" or "X" or "T";

        return new ProjectSummaryResponse
        {
            ProjectCode = project.ProjectCode, ProjectName = project.ProjectName, Year = year,
            IncomeBudget = Sum(Income, r => r.Budget), IncomeActual = Sum(Income, r => r.Actual),
            CostBudget = Sum(Cost, r => r.Budget), CostActual = Sum(Cost, r => r.Actual),
            ResultBudget = Sum(Income, r => r.Budget) - Sum(Cost, r => r.Budget),
            ResultActual = Sum(Income, r => r.Actual) - Sum(Cost, r => r.Actual),
            Rows = rows,
        };
    }
}

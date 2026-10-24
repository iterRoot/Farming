using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using CompanyEntity = FarmingApi.Modules.Company.Company;
using EmployeeEntity = FarmingApi.Modules.HumanResources.EmployeeMaster.EmployeeMaster;

namespace FarmingApi.Modules.HumanResources.Payroll;

// ═══════════════════════════════════════════════════════════════
// PAYSLIP  (ប័ណ្ណបៀវត្សរ៍)
//
// Shows the statutory lines the way a Cambodian employee expects to
// see them: earnings, then Tax on Salary and the 2% NSSF pension
// separately (not lumped as "deductions"), then net. Employer NSSF
// is printed as information — it is a company cost, not a deduction.
// ═══════════════════════════════════════════════════════════════
public class PayslipPdfDocument : IDocument
{
    private readonly PayrollResponse _p;
    private readonly EmployeeEntity? _employee;
    private readonly CompanyEntity?  _company;

    private static readonly string[] MonthNames =
    {
        "", "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
    };

    public PayslipPdfDocument(PayrollResponse payslip,
                              EmployeeEntity? employee,
                              CompanyEntity? company)
    {
        _p        = payslip;
        _employee = employee;
        _company  = company;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Column(col =>
            {
                col.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text(
                        "This payslip is computer generated. Amounts in USD; " +
                        $"statutory figures computed at {Num(_p.ExchangeRate, 0)} KHR/USD.")
                        .FontSize(7).FontColor(Colors.Grey.Darken1);
                    row.ConstantItem(60).AlignRight().Text(x =>
                    {
                        x.CurrentPageNumber().FontSize(7);
                        x.Span(" / ").FontSize(7);
                        x.TotalPages().FontSize(7);
                    });
                });
            });
        });
    }

    // ── Header ────────────────────────────────────────────────
    private void ComposeHeader(IContainer container)
    {
        container.Column(outer =>
        {
            outer.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(_company?.LocalCompanyName ?? _company?.Name ?? "Company Name")
                        .FontSize(15).Bold();

                    var addr = string.Join(", ", new[]
                    {
                        _company?.LocalStreet, _company?.LocalCity, _company?.LocalCountryRegion,
                    }.Where(s => !string.IsNullOrWhiteSpace(s)));

                    if (addr.Length > 0)
                        col.Item().Text(addr).FontSize(8).FontColor(Colors.Grey.Darken1);

                    if (!string.IsNullOrWhiteSpace(_company?.VatNumber1))
                        col.Item().Text($"VAT/TIN: {_company!.VatNumber1}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(190).Column(col =>
                {
                    col.Item().AlignRight().Text("PAYSLIP")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().AlignRight().Text("ប័ណ្ណបៀវត្សរ៍")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignRight().Text(
                        $"{MonthNames[_p.PeriodMonth]} {_p.PeriodYear}").FontSize(11).Bold();
                    col.Item().AlignRight().Text(_p.Status)
                        .FontSize(9)
                        .FontColor(_p.Status == "Paid" ? Colors.Green.Darken2 : Colors.Grey.Darken1);
                });
            });

            outer.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingTop(14).Column(col =>
        {
            col.Item().Element(ComposeEmployeeBlock);
            col.Item().PaddingTop(14).Element(ComposeEarningsAndDeductions);
            col.Item().PaddingTop(12).Element(ComposeNetPay);
            col.Item().PaddingTop(12).Element(ComposeEmployerBlock);
            col.Item().PaddingTop(24).Element(ComposeSignatures);
        });
    }

    // ── Employee details ──────────────────────────────────────
    private void ComposeEmployeeBlock(IContainer container)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                Pair(col, "Employee", _p.EmployeeName ?? _p.EmployeeCode);
                Pair(col, "Code", _p.EmployeeCode);
                Pair(col, "Position", _employee?.JobTitle ?? "—");
            });

            row.RelativeItem().Column(col =>
            {
                Pair(col, "Department", _employee?.Department ?? "—");
                Pair(col, "Branch", _employee?.Branch ?? "—");
                Pair(col, "Hire date",
                    _employee?.HireDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "—");
            });

            row.RelativeItem().Column(col =>
            {
                Pair(col, "Bank", _employee?.BankName ?? "—");
                Pair(col, "Account", _employee?.BankAccountNo ?? "—");
                Pair(col, "Dependents", _p.Dependents.ToString(CultureInfo.InvariantCulture));
            });
        });
    }

    // ── Earnings | Deductions, side by side ───────────────────
    private void ComposeEarningsAndDeductions(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().PaddingRight(6).Column(col =>
            {
                col.Item().Element(c => SectionTitle(c, "EARNINGS", Colors.Green.Darken2));

                Line(col, "Basic salary", _p.BasicSalary);
                if (_p.TaxableAllowance is > 0) Line(col, "Taxable allowance", _p.TaxableAllowance!.Value);
                if (_p.ExemptAllowance is > 0)  Line(col, "Exempt allowance", _p.ExemptAllowance!.Value);
                if (_p.OvertimePay is > 0)      Line(col, "Overtime", _p.OvertimePay!.Value);
                if (_p.Bonus is > 0)            Line(col, "Bonus", _p.Bonus!.Value);

                col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                Line(col, "Gross pay", _p.GrossPay, bold: true);
            });

            row.RelativeItem().PaddingLeft(6).Column(col =>
            {
                col.Item().Element(c => SectionTitle(c, "DEDUCTIONS", Colors.Red.Darken2));

                Line(col, "Tax on Salary (ToS)", _p.TaxOnSalary ?? 0m);
                Line(col, "NSSF pension 2%", _p.NssfEmployee ?? 0m);
                if (_p.UnpaidLeaveDeduction is > 0)
                    Line(col, "Unpaid leave", _p.UnpaidLeaveDeduction!.Value);
                if (_p.OtherDeductions is > 0)
                    Line(col, "Other deductions", _p.OtherDeductions!.Value);

                col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                Line(col, "Total deductions", _p.TotalDeductions, bold: true);

                col.Item().PaddingTop(6).Text(
                        $"Taxable income {Num(_p.TaxableIncomeKhr, 0)} KHR" +
                        (_p.Dependents > 0 ? $" (after {_p.Dependents} × 150,000 KHR relief)" : ""))
                    .FontSize(7).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    // ── Net pay banner ────────────────────────────────────────
    private void ComposeNetPay(IContainer container)
    {
        container.Background(Colors.Blue.Darken2).Padding(10).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("NET PAY").FontSize(11).Bold().FontColor(Colors.White);
                col.Item().Text("ប្រាក់បៀវត្សរ៍សុទ្ធ").FontSize(8).FontColor(Colors.Grey.Lighten3);
            });

            row.ConstantItem(220).AlignRight().Column(col =>
            {
                col.Item().AlignRight().Text($"USD {Num(_p.NetPay, 2)}")
                    .FontSize(18).Bold().FontColor(Colors.White);
                col.Item().AlignRight()
                    .Text($"≈ {Num(Math.Round(_p.NetPay * _p.ExchangeRate, 0), 0)} KHR")
                    .FontSize(9).FontColor(Colors.Grey.Lighten3);
            });
        });
    }

    // ── Employer cost (information only) ──────────────────────
    private void ComposeEmployerBlock(IContainer container)
    {
        container.Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(8).Column(col =>
        {
            col.Item().Text("EMPLOYER CONTRIBUTIONS — not deducted from the employee")
                .FontSize(7).Bold().FontColor(Colors.Grey.Darken2);

            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text("NSSF employer (pension, health care, occupational risk)")
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
                row.ConstantItem(80).AlignRight()
                    .Text(Num(_p.NssfEmployer ?? 0m, 2)).FontSize(8);
            });

            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Total cost to employer").FontSize(8).Bold();
                row.ConstantItem(80).AlignRight().Text(Num(_p.EmployerCost, 2)).FontSize(8).Bold();
            });
        });
    }

    private void ComposeSignatures(IContainer container)
    {
        container.Row(row =>
        {
            foreach (var label in new[] { "Prepared by", "Approved by", "Received by (employee)" })
            {
                row.RelativeItem().PaddingRight(14).Column(col =>
                {
                    col.Item().PaddingTop(22).LineHorizontal(0.5f).LineColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(3).Text(label)
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().Text("Date: ____________")
                        .FontSize(7).FontColor(Colors.Grey.Medium);
                });
            }
        });
    }

    // ── small helpers ─────────────────────────────────────────
    private static void SectionTitle(IContainer c, string text, string color) =>
        c.BorderBottom(1).BorderColor(color).PaddingBottom(3)
         .Text(text).FontSize(9).Bold().FontColor(color);

    private static void Line(ColumnDescriptor col, string label, decimal value, bool bold = false)
    {
        col.Item().PaddingTop(3).Row(row =>
        {
            var l = row.RelativeItem().Text(label).FontSize(9);
            if (bold) l.Bold();

            var v = row.ConstantItem(80).AlignRight().Text(Num(value, 2)).FontSize(9);
            if (bold) v.Bold();
        });
    }

    private static void Pair(ColumnDescriptor col, string label, string value)
    {
        col.Item().PaddingBottom(2).Row(row =>
        {
            row.ConstantItem(70).Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().Text(value).FontSize(9).Bold();
        });
    }

    private static string Num(decimal v, int dp) =>
        v.ToString("N" + dp, CultureInfo.InvariantCulture);
}

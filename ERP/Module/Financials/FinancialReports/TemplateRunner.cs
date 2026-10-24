using FarmingApi.Modules.Financials.FinancialReportTemplates;

namespace FarmingApi.Modules.Financials.FinancialReports;

public class RunLine
{
    public int LineNum { get; set; }
    public string RowCode { get; set; } = "";
    public string Description { get; set; } = "";
    public string RowType { get; set; } = "";
    public int Level { get; set; }
    public decimal? Value { get; set; }
    public bool Bold { get; set; }
    public bool Underline { get; set; }
    public bool Hidden { get; set; }        // a zero line the template says not to print
    public string? Note { get; set; }
    public string? AccountFrom { get; set; }
    public string? AccountTo { get; set; }
}

// Fills a report template with numbers: Account rows take the balance of their account range,
// Formula rows do arithmetic over other rows by row code, Title rows total the rows under them.
public static class TemplateRunner
{
    public static (List<RunLine>? Lines, string? Error) Run(FinancialReportTemplate t, ReportEngine engine, Dictionary<string, decimal> net)
    {
        var rows = t.Rows.OrderBy(r => r.LineNum).ToList();
        var byCode = rows.ToDictionary(r => r.RowCode.ToUpperInvariant());
        var vals = new Dictionary<string, decimal>(); var notes = new Dictionary<string, string>();
        var busy = new HashSet<string>();
        string? error = null;

        decimal Account(FinancialReportTemplateRow r)
        {
            var from = (r.AccountFrom ?? "").Trim(); var to = string.IsNullOrWhiteSpace(r.AccountTo) ? from : r.AccountTo!.Trim();
            if (from.Length == 0) return 0m;
            var sum = 0m;
            foreach (var a in engine.Accounts)
                if (a.Type != "M" && string.CompareOrdinal(a.Code, from) >= 0 && string.CompareOrdinal(a.Code, to) <= 0)
                    sum += net.GetValueOrDefault(a.Code);
            return sum * (r.Sign == -1 ? -1 : 1);
        }

        decimal Eval(string code)
        {
            code = code.ToUpperInvariant();
            if (vals.TryGetValue(code, out var v)) return v;
            if (!byCode.TryGetValue(code, out var r)) { error ??= $"A formula refers to '{code}', which isn't a row of this template."; return 0m; }
            if (!busy.Add(code)) { error ??= $"Row {code} depends on itself."; return 0m; }
            decimal res = 0m;
            switch (r.RowType)
            {
                case "Account": res = Account(r); break;
                case "Formula":
                    var p = new Parser(r.Formula ?? "", Eval);
                    res = p.Parse(out var perr);
                    if (perr != null) error ??= $"Row {r.RowCode}: {perr}";
                    if (p.DivideByZero) notes[code] = "Division by zero — shown as 0.";
                    break;
                case "Title":
                    res = rows.Where(c => string.Equals(c.ParentRowCode, r.RowCode, StringComparison.OrdinalIgnoreCase) && c.RowType is "Account" or "Formula")
                              .Sum(c => Eval(c.RowCode));
                    break;
            }
            busy.Remove(code);
            return vals[code] = Math.Round(res, 2);
        }

        var lines = new List<RunLine>();
        foreach (var r in rows)
        {
            var hasValue = r.RowType is "Account" or "Formula" or "Title";
            decimal? v = hasValue ? Eval(r.RowCode) : null;
            lines.Add(new RunLine
            {
                LineNum = r.LineNum, RowCode = r.RowCode, Description = r.Description, RowType = r.RowType, Level = r.Level, Value = v,
                Bold = r.Bold, Underline = r.Underline, Hidden = v == 0 && !r.PrintZero && r.RowType != "Title",
                Note = notes.GetValueOrDefault(r.RowCode.ToUpperInvariant()), AccountFrom = r.AccountFrom, AccountTo = r.AccountTo,
            });
        }
        return error != null ? (null, error) : (lines, null);
    }

    // + - * / ( ) numbers and row codes, with unary minus. Never evaluates code.
    private class Parser
    {
        private readonly string _s; private int _i; private readonly Func<string, decimal> _lookup;
        public bool DivideByZero;
        public Parser(string s, Func<string, decimal> lookup) { _s = s; _lookup = lookup; }

        public decimal Parse(out string? error)
        {
            error = null;
            try
            {
                var v = Expr();
                Skip();
                if (_i < _s.Length) error = $"unexpected '{_s[_i]}' in the formula";
                return v;
            }
            catch (FormatException ex) { error = ex.Message; return 0m; }
        }

        private void Skip() { while (_i < _s.Length && char.IsWhiteSpace(_s[_i])) _i++; }
        private decimal Expr()
        {
            var v = Term();
            for (;;)
            {
                Skip();
                if (_i < _s.Length && _s[_i] == '+') { _i++; v += Term(); }
                else if (_i < _s.Length && _s[_i] == '-') { _i++; v -= Term(); }
                else return v;
            }
        }
        private decimal Term()
        {
            var v = Factor();
            for (;;)
            {
                Skip();
                if (_i < _s.Length && _s[_i] == '*') { _i++; v *= Factor(); }
                else if (_i < _s.Length && _s[_i] == '/') { _i++; var d = Factor(); if (d == 0) { DivideByZero = true; v = 0; } else v /= d; }
                else return v;
            }
        }
        private decimal Factor()
        {
            Skip();
            if (_i >= _s.Length) throw new FormatException("the formula ends unexpectedly");
            var c = _s[_i];
            if (c == '-') { _i++; return -Factor(); }
            if (c == '+') { _i++; return Factor(); }
            if (c == '(') { _i++; var v = Expr(); Skip(); if (_i >= _s.Length || _s[_i] != ')') throw new FormatException("a bracket isn't closed"); _i++; return v; }
            if (char.IsDigit(c) || c == '.')
            {
                var st = _i; while (_i < _s.Length && (char.IsDigit(_s[_i]) || _s[_i] == '.')) _i++;
                if (!decimal.TryParse(_s[st.._i], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var n)) throw new FormatException($"'{_s[st.._i]}' isn't a number");
                return n;
            }
            if (char.IsLetter(c))
            {
                var st = _i; while (_i < _s.Length && (char.IsLetterOrDigit(_s[_i]) || _s[_i] == '_' || _s[_i] == '-') && !(_s[_i] == '-' && _i + 1 < _s.Length && !char.IsLetterOrDigit(_s[_i + 1]))) _i++;
                return _lookup(_s[st.._i]);
            }
            throw new FormatException($"unexpected '{c}' in the formula");
        }
    }
}

using System.Globalization;

namespace FarmingApi.Modules.Administration.UserTables;

// One rulebook for a user-defined value, shared by user-defined fields (values on documents
// and master data) and user-defined tables (values in their rows).
public static class UserValue
{
    public static readonly string[] Types = { "Text", "Number", "Date", "Checkbox", "Dropdown" };
    public const int MaxText = 2000;

    /// <summary>Returns an error message, or null when acceptable; <paramref name="value"/> is the canonical text to store ("" = empty).</summary>
    public static string? Check(string label, string type, int? length, bool required, IReadOnlyCollection<string>? valid, string? raw, out string value)
    {
        value = "";
        var v = (raw ?? "").Trim();
        if (v.Length == 0) return required ? $"{label} is required." : null;

        switch (type)
        {
            case "Text":
                var max = Math.Min(length is > 0 ? length.Value : MaxText, MaxText);
                if (v.Length > max) return $"{label} can't be longer than {max} characters.";
                if (v.Any(char.IsControl) && !v.All(c => c == '\n' || c == '\r' || c == '\t' || !char.IsControl(c))) return $"{label} has characters that aren't allowed.";
                value = v; return null;
            case "Number":
                if (!decimal.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var n)) return $"{label} must be a number.";
                if (Math.Abs(n) > 1_000_000_000_000m) return $"{label} is too large.";
                value = n.ToString(CultureInfo.InvariantCulture); return null;
            case "Date":
                if (!DateOnly.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return $"{label} must be a date (yyyy-mm-dd).";
                value = d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture); return null;
            case "Checkbox":
                switch (v.ToLowerInvariant())
                {
                    case "true": case "1": case "yes": case "y": value = "true"; return null;
                    case "false": case "0": case "no": case "n": value = "false"; return null;
                    default: return $"{label} must be yes or no.";
                }
            case "Dropdown":
                var hit = (valid ?? Array.Empty<string>()).FirstOrDefault(x => string.Equals(x, v, StringComparison.OrdinalIgnoreCase));
                if (hit == null) return $"'{v}' isn't one of the allowed values for {label}.";
                value = hit; return null;
            default:
                return $"{label} has an unknown type '{type}'.";
        }
    }
}

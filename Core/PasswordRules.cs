namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// WHAT MAKES A NEW PASSWORD ACCEPTABLE
//
// Composition rules alone ("one capital, one digit, one symbol") let through "Password1!" and turn away a
// long passphrase. So, on top of the administrator's Password Administration settings:
//   • a hard floor of 10 characters, whatever the setting says;
//   • 16 or more characters is a passphrase: the composition rules are waived;
//   • a common password — or one with a common word and only a few characters of decoration around it
//     ("Password1!", "P@ssw0rd2026", "Welcome123") — is refused;
//   • it can't contain the user's own ID or e-mail name, be one repeated character, or a run like 123456789012.
// ═══════════════════════════════════════════════════════════════════════════
public static class PasswordRules
{
    public const int HardMinimum = 10;
    public const int PassphraseLength = 16;

    // Words people build passwords from. Matched after undoing the usual look-alike swaps (@→a, 0→o, 3→e, 1→l/i, $→s, 5→s, 7→t).
    private static readonly string[] Common =
    {
        "password", "passw0rd", "passwd", "qwerty", "qwertyuiop", "asdfgh", "zxcvbn", "welcome", "letmein", "administrator", "admin",
        "iloveyou", "monkey", "dragon", "football", "baseball", "basketball", "master", "login", "abc123", "changeme", "secret",
        "trustno", "sunshine", "princess", "starwars", "whatever", "freedom", "shadow", "superman", "batman", "michael", "jordan",
        "harley", "ranger", "hunter", "buster", "soccer", "hockey", "killer", "george", "computer", "internet", "windows", "google",
        "facebook", "summer", "winter", "spring", "autumn", "january", "february", "march", "april", "august", "september", "october",
        "november", "december", "cambodia", "phnompenh", "khmer", "kamnttra", "komnaktra", "manager", "default", "guest", "temp",
        "company", "chicken", "cattle", "farming", "qazwsx", "1q2w3e", "pa55word", "p4ssw0rd",
    };

    /// <summary>Null when the password is acceptable, otherwise a plain explanation.</summary>
    public static string? Check(string? password, PasswordPolicyView policy, string? userId = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(password)) return "Password is required.";

        var min = Math.Max(policy.MinLength, HardMinimum);
        if (password.Length < min) return $"Password must be at least {min} characters. A longer passphrase works well.";

        var passphrase = password.Length >= PassphraseLength;
        if (!passphrase)
        {
            if (policy.RequireUppercase && !password.Any(char.IsUpper)) return Describe(policy, min);
            if (policy.RequireLowercase && !password.Any(char.IsLower)) return Describe(policy, min);
            if (policy.RequireDigit && !password.Any(char.IsDigit)) return Describe(policy, min);
            if (policy.RequireSpecial && !password.Any(c => "@$!%*?&".Contains(c))) return Describe(policy, min);
        }

        var lower = password.ToLowerInvariant();
        if (lower.Distinct().Count() == 1) return "Password can't be one repeated character.";
        if (IsRun(lower)) return "Password can't be a simple run like 123456789012 or abcdefghijkl.";

        if (!string.IsNullOrWhiteSpace(userId) && userId.Trim().Length >= 3 && lower.Contains(userId.Trim().ToLowerInvariant()))
            return "Password can't contain your user ID.";
        var local = email?.Split('@')[0].Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(local) && local.Length >= 4 && lower.Contains(local))
            return "Password can't contain your e-mail name.";

        foreach (var variant in Variants(lower))
            foreach (var word in Common)
            {
                var at = variant.IndexOf(word, StringComparison.Ordinal);
                if (at < 0) continue;
                // a common word with only a little around it is still a common password
                if (variant.Length - word.Length < 8) return "That password is too common or too easy to guess. Choose something longer and less predictable.";
            }
        return null;
    }

    public static string Describe(PasswordPolicyView p, int? min = null)
    {
        var parts = new List<string>();
        if (p.RequireUppercase) parts.Add("an uppercase letter");
        if (p.RequireLowercase) parts.Add("a lowercase letter");
        if (p.RequireDigit) parts.Add("a number");
        if (p.RequireSpecial) parts.Add("a special character (@$!%*?&)");
        var m = min ?? Math.Max(p.MinLength, HardMinimum);
        return $"Password must have at least {m} characters" + (parts.Count > 0 ? $" including {string.Join(", ", parts)}" : "")
             + $", or be a passphrase of {PassphraseLength}+ characters. Common passwords aren't allowed.";
    }

    private static IEnumerable<string> Variants(string s)
    {
        string Map(string x, char one) => x.Replace('@', 'a').Replace('4', 'a').Replace('$', 's').Replace('5', 's').Replace('0', 'o')
                                           .Replace('3', 'e').Replace('7', 't').Replace('!', 'i').Replace('1', one);
        yield return Map(s, 'l');
        yield return Map(s, 'i');
    }

    private static bool IsRun(string s)
    {
        if (s.Length < 8) return false;
        var up = true; var down = true;
        for (var i = 1; i < s.Length; i++)
        {
            if (s[i] != s[i - 1] + 1) up = false;
            if (s[i] != s[i - 1] - 1) down = false;
        }
        return up || down;
    }
}

// The few policy values the rules need (the DTO lives with the Password Administration screen).
public record PasswordPolicyView(int MinLength, bool RequireUppercase, bool RequireLowercase, bool RequireDigit, bool RequireSpecial);

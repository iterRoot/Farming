using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// PASSWORD HASHING
//
// New hashes use ASP.NET Core's PasswordHasher (PBKDF2 with a random salt and a
// work factor) instead of one unsalted SHA-256, so a leaked database can't be
// cracked with a table of common passwords and identical passwords no longer
// share a hash.
//
// Existing accounts keep working: a stored value that is a bare 44-character
// SHA-256 Base64 is still accepted, and Verify reports needsUpgrade so the
// caller can store a new hash right after a successful sign-in. No password
// has to be reset.
//
// Two-factor recovery codes are long random values, not human passwords, so they
// stay on SHA-256 (Hash256) — a slow hash would add cost without adding safety.
// ═══════════════════════════════════════════════════════════════════════════
public static class PasswordHashing
{
    private static readonly PasswordHasher<object> Hasher = new();

    public static string Hash(string password) => Hasher.HashPassword(null!, password);

    // A bare SHA-256 digest in Base64 is exactly 44 characters; PasswordHasher output is longer.
    public static bool IsLegacy(string? hash) => hash is { Length: 44 };

    public static bool Verify(string password, string? hash, out bool needsUpgrade)
    {
        needsUpgrade = false;
        if (string.IsNullOrEmpty(hash)) return false;

        if (IsLegacy(hash))
        {
            var ok = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(Hash256(password)), Encoding.UTF8.GetBytes(hash));
            needsUpgrade = ok;
            return ok;
        }

        var result = Hasher.VerifyHashedPassword(null!, hash, password);
        needsUpgrade = result == PasswordVerificationResult.SuccessRehashNeeded;
        return result != PasswordVerificationResult.Failed;
    }

    // Plain SHA-256 (Base64). Only for values that are already high-entropy, e.g. recovery codes.
    public static string Hash256(string value)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
    }
}

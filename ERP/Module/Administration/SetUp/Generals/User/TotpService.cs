using System.Security.Cryptography;
using System.Text;

namespace FarmingApi.Modules.Administration.SetUp.User;

/// <summary>
/// RFC 6238 TOTP (Time-based One-Time Password) — compatible with
/// Microsoft Authenticator, Google Authenticator, Authy, 1Password, etc.
/// Dependency-free: HMAC-SHA1, 30-second step, 6 digits (the defaults
/// every authenticator app assumes).
/// </summary>
public interface ITotpService
{
    string GenerateSecret();
    string BuildOtpAuthUri(string secret, string account, string issuer);
    bool   VerifyCode(string secret, string code, int windowSteps = 1);
    List<string> GenerateRecoveryCodes(int count = 8);
}

public class TotpService : ITotpService
{
    private const int    StepSeconds = 30;
    private const int    Digits      = 6;
    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    // ── Secret generation (Base32, 160-bit) ────────────────────────
    public string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20); // 160 bits
        return Base32Encode(bytes);
    }

    // ── otpauth:// URI that the authenticator app scans ────────────
    public string BuildOtpAuthUri(string secret, string account, string issuer)
    {
        var iss = Uri.EscapeDataString(issuer);
        var acc = Uri.EscapeDataString(account);
        return $"otpauth://totp/{iss}:{acc}?secret={secret}&issuer={iss}"
             + $"&algorithm=SHA1&digits={Digits}&period={StepSeconds}";
    }

    // ── Verification ───────────────────────────────────────────────
    // windowSteps allows +/- N steps of clock drift (1 => ±30s).
    public bool VerifyCode(string secret, string code, int windowSteps = 1)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        code = code.Trim().Replace(" ", "");
        if (code.Length != Digits || !code.All(char.IsDigit)) return false;

        var key  = Base32Decode(secret);
        var step = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / StepSeconds;

        for (var offset = -windowSteps; offset <= windowSteps; offset++)
        {
            var candidate = ComputeCode(key, step + offset);
            // Fixed-time compare so we don't leak timing information
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.ASCII.GetBytes(candidate),
                    Encoding.ASCII.GetBytes(code)))
                return true;
        }
        return false;
    }

    public List<string> GenerateRecoveryCodes(int count = 8)
    {
        var codes = new List<string>(count);
        for (var i = 0; i < count; i++)
        {
            // 10 hex chars, formatted xxxxx-xxxxx
            var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToLowerInvariant();
            codes.Add($"{raw[..5]}-{raw[5..]}");
        }
        return codes;
    }

    // ── Core HOTP computation ──────────────────────────────────────
    private static string ComputeCode(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);

        // Dynamic truncation (RFC 4226 §5.4)
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset]     & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   |  (hash[offset + 3] & 0xFF);

        var otp = binary % (int)Math.Pow(10, Digits);
        return otp.ToString().PadLeft(Digits, '0');
    }

    // ── Base32 (RFC 4648) ──────────────────────────────────────────
    private static string Base32Encode(byte[] data)
    {
        var sb = new StringBuilder();
        int bits = 0, value = 0;
        foreach (var b in data)
        {
            value = (value << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                sb.Append(Base32Chars[(value >> (bits - 5)) & 31]);
                bits -= 5;
            }
        }
        if (bits > 0) sb.Append(Base32Chars[(value << (5 - bits)) & 31]);
        return sb.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        input = input.TrimEnd('=').ToUpperInvariant().Replace(" ", "");
        var bytes = new List<byte>(input.Length * 5 / 8);
        int bits = 0, value = 0;

        foreach (var c in input)
        {
            var idx = Base32Chars.IndexOf(c);
            if (idx < 0) continue; // skip anything unexpected
            value = (value << 5) | idx;
            bits += 5;
            if (bits >= 8)
            {
                bytes.Add((byte)((value >> (bits - 8)) & 0xFF));
                bits -= 8;
            }
        }
        return bytes.ToArray();
    }
}

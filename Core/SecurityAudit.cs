using System.Collections.Concurrent;
using System.Security.Claims;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// SECURITY EVENT LOG
//
// One log line per security-relevant event — sign-ins, lockouts, refused
// tokens, permission denials, and changes to users, passwords, two-factor and
// permissions — written to the "Security" log category so it can be filtered
// out of the rest of the app's output (Render's log stream, or any provider
// it forwards to). Every line starts with "SECURITY" and carries the event,
// outcome, who did it, who it was done to, the client IP and the time.
//
// Alerts: a burst of failures from one IP, repeated failures on one account,
// repeated permission denials for one user, or an account being locked writes
// one extra line at Error level starting with "SECURITY ALERT" — point a log
// alert at that text. Each alert fires at most once per window per key so a
// sustained attack doesn't flood the log.
//
// Never logs passwords, tokens, 2FA codes or secrets. Text that came from the
// request (a typed user ID, a path) is stripped of control characters and
// shortened, so it can't forge extra log lines.
// ═══════════════════════════════════════════════════════════════════════════
public class SecurityAudit
{
    // Thresholds for SECURITY ALERT lines, all within Window.
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(5);
    public const int FailedLoginsPerIp      = 10;
    public const int FailedLoginsPerAccount = 5;
    public const int DenialsPerUser         = 20;

    private readonly ILogger _log;
    private readonly IHttpContextAccessor _http;
    private readonly ConcurrentDictionary<string, Queue<DateTime>> _hits = new();
    private readonly ConcurrentDictionary<string, DateTime> _alerted = new();

    public SecurityAudit(ILoggerFactory loggers, IHttpContextAccessor http)
    {
        _log  = loggers.CreateLogger("Security");
        _http = http;
    }

    // ── Sign-in ───────────────────────────────────────────────────
    public void LoginSucceeded(string userId, bool usedRecoveryCode = false) =>
        Write(LogLevel.Information, "login", "success", actor: userId, detail: usedRecoveryCode ? "recovery code used" : null);

    // reason: unknown-user, wrong-password, locked, inactive, wrong-2fa-code
    public void LoginFailed(string typedUserId, string reason, int? attemptsLeft = null)
    {
        var ip = ClientIp();
        Write(LogLevel.Warning, "login", "failure", actor: typedUserId, ip: ip,
              detail: attemptsLeft is int n ? $"{reason}; {n} attempt(s) left" : reason);

        if (Count($"ip:{ip}") >= FailedLoginsPerIp)
            Alert($"ip:{ip}", "login-burst-ip", $"{FailedLoginsPerIp}+ failed sign-ins from {ip} within {Window.TotalMinutes:0} min");
        var account = Clean(typedUserId).ToLowerInvariant();
        if (Count($"acct:{account}") >= FailedLoginsPerAccount)
            Alert($"acct:{account}", "login-burst-account", $"{FailedLoginsPerAccount}+ failed sign-ins for '{Clean(typedUserId)}' within {Window.TotalMinutes:0} min");
    }

    public void AccountLocked(string userId, string via) =>
        Alert($"locked:{Clean(userId).ToLowerInvariant()}", "account-locked",
              $"account '{Clean(userId)}' locked after repeated failures ({via})", once: false);

    public void TwoFactorChallenge(string userId) =>
        Write(LogLevel.Information, "login.2fa-required", "challenge", actor: userId);

    // A request carried a token that failed validation (expired, bad signature, wrong issuer...).
    public void TokenRejected(string reason) =>
        Write(LogLevel.Warning, "token", "rejected", detail: reason);

    // A request was refused by a rate limit (usually sign-in guessing or a flood from one IP).
    public void RateLimited(string request)
    {
        var ip = ClientIp();
        Write(LogLevel.Warning, "rate-limit", "rejected", ip: ip, detail: request);
        if (Count($"rl:{ip}") >= 5)
            Alert($"rl:{ip}", "rate-limit-burst", $"{ip} keeps hitting the rate limit ({Clean(request)})");
    }

    // ── Permissions ───────────────────────────────────────────────
    public void AccessDenied(string? subject, string method, string path)
    {
        var actor = CurrentUser();
        Write(LogLevel.Warning, "access", "denied", target: subject ?? "(unmapped)", detail: $"{method} {path}");
        if (Count($"deny:{actor}") >= DenialsPerUser)
            Alert($"deny:{actor}", "access-denied-burst", $"{DenialsPerUser}+ permission denials for {actor} within {Window.TotalMinutes:0} min");
    }

    // ── Account and permission changes (always by the signed-in user) ─
    public void Changed(string evt, string target, string? detail = null) =>
        Write(LogLevel.Information, evt, "success", target: target, detail: detail);

    public void ChangeRefused(string evt, string target, string reason) =>
        Write(LogLevel.Warning, evt, "failure", target: target, detail: reason);

    // ── Plumbing ──────────────────────────────────────────────────
    private void Write(LogLevel level, string evt, string outcome, string? actor = null, string? target = null,
                       string? ip = null, string? detail = null)
    {
        _log.Log(level,
            "SECURITY {Event} outcome={Outcome} actor={Actor} target={Target} ip={Ip} at={At:o} {Detail}",
            evt, outcome, Clean(actor ?? CurrentUser()), Clean(target ?? "-"), ip ?? ClientIp(),
            DateTime.UtcNow, Clean(detail ?? ""));
    }

    private void Alert(string key, string kind, string message, bool once = true)
    {
        var now = DateTime.UtcNow;
        if (once && _alerted.TryGetValue(key, out var last) && now - last < Window) return;
        _alerted[key] = now;
        _log.LogError("SECURITY ALERT {Kind} ip={Ip} at={At:o} {Message}", kind, ClientIp(), now, message);
    }

    // Adds a hit for the key and returns how many fall inside the window.
    private int Count(string key)
    {
        var now = DateTime.UtcNow;
        if (_hits.Count > 10_000) Prune(now);        // bound memory under a spray of random keys
        var q = _hits.GetOrAdd(key, _ => new Queue<DateTime>());
        lock (q)
        {
            q.Enqueue(now);
            while (q.Count > 0 && now - q.Peek() > Window) q.Dequeue();
            return q.Count;
        }
    }

    private void Prune(DateTime now)
    {
        foreach (var (k, q) in _hits)
            lock (q) { if (q.Count == 0 || now - q.Last() > Window) _hits.TryRemove(k, out _); }
        foreach (var (k, t) in _alerted)
            if (now - t > Window) _alerted.TryRemove(k, out _);
    }

    private string CurrentUser()
    {
        var u = _http.HttpContext?.User;
        if (u?.Identity?.IsAuthenticated != true) return "anonymous";
        var id = u.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return $"{u.FindFirst(ClaimTypes.Name)?.Value ?? "?"}#{id}";
    }

    // Behind Render's proxy this is the real client because ASPNETCORE_FORWARDEDHEADERS_ENABLED=true.
    private string ClientIp() => _http.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "-";

    private static string Clean(string s)
    {
        var t = new string(s.Where(c => !char.IsControl(c)).ToArray());
        return t.Length > 80 ? t[..80] + "…" : t;
    }
}

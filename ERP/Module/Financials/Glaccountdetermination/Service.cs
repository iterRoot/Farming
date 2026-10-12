using System.Linq.Expressions;

namespace FarmingApi.Modules.Financials.GLAccountDetermination;

// ═══════════════════════════════════════════════════════════════
// GL ACCOUNT RESOLUTION
//
// KGLD is a single settings row that thirteen posting services read
// before they can build a journal. Each of them fetched it directly
// and hand-wrote its own "not configured" message, so the wording a
// user saw depended on which document they happened to post.
//
// This puts the fetch and the null checks in one place. Failing to
// resolve an account is an InvalidOperationException, matching what
// the posting services already throw, so callers keep the behaviour
// they had — only the message improves and stops varying.
// ═══════════════════════════════════════════════════════════════
public interface IGLAccountDeterminationService
{
    /// <summary>The settings row, or throws if it has never been configured.</summary>
    GLAccountDetermination Settings();

    /// <summary>
    /// Resolves one account code, throwing a message that names the field when
    /// it is blank. Usage: <c>Require(g =&gt; g.AccountsPayable)</c>.
    /// </summary>
    string Require(Expression<Func<GLAccountDetermination, string?>> pick);

    /// <summary>Resolves an account code, or null when it is not set.</summary>
    string? Optional(Func<GLAccountDetermination, string?> pick);

    /// <summary>
    /// True when every named account is set. Lets a screen check readiness
    /// without provoking an exception.
    /// </summary>
    bool IsConfigured(params Expression<Func<GLAccountDetermination, string?>>[] picks);
}

public class GLAccountDeterminationService : IGLAccountDeterminationService
{
    private readonly MyDbContext _db;

    public GLAccountDeterminationService(MyDbContext db) => _db = db;

    /// <summary>
    /// Cached per request. A posting service resolves several accounts in a
    /// row, and the settings row cannot change mid-request.
    /// </summary>
    private GLAccountDetermination? _cached;

    public GLAccountDetermination Settings()
    {
        _cached ??= _db.Set<GLAccountDetermination>().FirstOrDefault();

        return _cached ?? throw new InvalidOperationException(
            "GL Account Determination (KGLD) has not been configured. " +
            "Set it up under Administration → GL Account Determination before posting.");
    }

    public string Require(Expression<Func<GLAccountDetermination, string?>> pick)
    {
        var value = pick.Compile()(Settings());
        if (!string.IsNullOrWhiteSpace(value)) return value.Trim();

        throw new InvalidOperationException(
            $"The '{FieldName(pick)}' account is not set in GL Account Determination (KGLD). " +
            "Set it before posting this document.");
    }

    public string? Optional(Func<GLAccountDetermination, string?> pick)
    {
        var value = pick(Settings());
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public bool IsConfigured(params Expression<Func<GLAccountDetermination, string?>>[] picks)
    {
        var settings = _db.Set<GLAccountDetermination>().FirstOrDefault();
        if (settings == null) return false;

        return picks.All(p => !string.IsNullOrWhiteSpace(p.Compile()(settings)));
    }

    /// <summary>
    /// Reads the property name out of the lambda so the error can name the
    /// field the user has to fill in, rather than a generic "account missing".
    /// </summary>
    private static string FieldName(Expression<Func<GLAccountDetermination, string?>> pick)
    {
        var body = pick.Body;
        // A string? property picked from the entity arrives unwrapped, but a
        // conversion node appears when the compiler boxes it.
        if (body is UnaryExpression unary) body = unary.Operand;
        return body is MemberExpression member ? member.Member.Name : "account";
    }
}

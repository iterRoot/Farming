using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════
// Issues and configures validation for the app's bearer tokens.
// One signing key, read from configuration only (user-secrets in
// Development, the Jwt__Key env var elsewhere) — never hardcoded
// and never checked into appsettings*.json.
// ═══════════════════════════════════════════════════════════════
public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) IssueToken(UserEntity user, string? groupName);
    /// <summary>A new token with the same identity and the same session start, so activity keeps a session alive without extending its absolute limit.</summary>
    (string Token, DateTime ExpiresAt) Refresh(System.Security.Claims.ClaimsPrincipal principal);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public static SymmetricSecurityKey GetSigningKey(IConfiguration config)
    {
        var key = config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key is missing or too short. Set it via 'dotnet user-secrets set \"Jwt:Key\" \"<random 32+ byte value>\"' " +
                "in Development, or the Jwt__Key environment variable elsewhere. Refusing to start with a weak/missing signing key.");
        }
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public (string Token, DateTime ExpiresAt) IssueToken(UserEntity user, string? groupName)
    {
        var issuer   = _config["Jwt:Issuer"]   ?? "FarmingApi";
        var audience = _config["Jwt:Audience"] ?? "FarmingApiClients";
        var minutes  = ExpiryMinutes(_config);
        var expires  = DateTime.UtcNow.AddMinutes(minutes);

        return Build(issuer, audience, expires, NewClaims(user, groupName, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
    }

    // A token lives for a short while (default 30 minutes) and is renewed on use, so it works as an idle timeout:
    // leave the app alone for longer than that and the next request needs a fresh sign-in. The session as a whole
    // is capped separately (Jwt:MaxSessionHours, default 12) however busy the user is.
    public static int ExpiryMinutes(IConfiguration c) => c.GetValue<int?>("Jwt:ExpiryMinutes") ?? 30;
    public static int RefreshAfterMinutes(IConfiguration c) => c.GetValue<int?>("Jwt:RefreshAfterMinutes") ?? 10;
    public static int MaxSessionHours(IConfiguration c) => c.GetValue<int?>("Jwt:MaxSessionHours") ?? 12;

    public (string Token, DateTime ExpiresAt) Refresh(ClaimsPrincipal principal)
    {
        var issuer   = _config["Jwt:Issuer"]   ?? "FarmingApi";
        var audience = _config["Jwt:Audience"] ?? "FarmingApiClients";
        var expires  = DateTime.UtcNow.AddMinutes(ExpiryMinutes(_config));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("iatms",  DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()),
            new("authms", principal.FindFirst("authms")?.Value ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()),
        };
        // identity claims carry over unchanged
        foreach (var c in principal.Claims.Where(c => c.Type is JwtRegisteredClaimNames.Sub or "userId" or ClaimTypes.NameIdentifier or ClaimTypes.Name or ClaimTypes.Email or ClaimTypes.Role))
            claims.Add(new Claim(c.Type, c.Value));
        return Build(issuer, audience, expires, claims);
    }

    private (string Token, DateTime ExpiresAt) Build(string issuer, string audience, DateTime expires, List<Claim> claims)
    {
        var creds = new SigningCredentials(GetSigningKey(_config), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private static List<Claim> NewClaims(UserEntity user, string? groupName, long sessionStartMs)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("iatms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()),   // checked against User.SessionsValidFrom
            new("authms", sessionStartMs.ToString()),                                    // when this session began (absolute limit)
            new("userId", user.UserId),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserId),
        };
        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        if (!string.IsNullOrWhiteSpace(groupName))
            claims.Add(new Claim(ClaimTypes.Role, groupName));

        return claims;
    }
}

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
        var minutes  = _config.GetValue<int?>("Jwt:ExpiryMinutes") ?? 480;
        var expires  = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.UserId),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserId),
        };
        if (!string.IsNullOrWhiteSpace(user.Email))
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        if (!string.IsNullOrWhiteSpace(groupName))
            claims.Add(new Claim(ClaimTypes.Role, groupName));

        var creds = new SigningCredentials(GetSigningKey(_config), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

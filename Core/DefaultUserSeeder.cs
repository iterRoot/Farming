using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;
using UserGroupEntity = FarmingApi.Modules.Administration.SetUp.UserGroup.UserGroup;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════
// DEFAULT MANAGER LOGIN
//
// Every environment needs at least one way in. This runs on every
// startup and guarantees a "Manager" login exists — idempotent, so
// it survives a fresh database, a restore from backup, or someone
// deleting it by accident, without ever duplicating itself or
// overwriting a password someone has since changed.
//
// The first password is NOT in the source: it comes from the
// Seed__ManagerPassword setting (environment variable) when that is set
// and strong enough, otherwise a random one is generated and written to
// the startup log once. Either way it is only used when the account is
// created — an existing Manager is never touched.
//
// Uses the exact hashing scheme UserController.HashPassword does
// (SHA-256 of the UTF-8 bytes, Base64), so this account logs in
// through the normal /User/Login endpoint like any other user.
// ═══════════════════════════════════════════════════════════════
public static class DefaultUserSeeder
{

    public const string DefaultUserId = "Manager";
    /// <summary>The only user allowed in without an Authorisations record (see SubjectAuthorizationFilter).</summary>
    public const string ManagerUserId = DefaultUserId;
    public const string DefaultGroup  = "Managers";
    /// <summary>Manager's row Id on a fresh database.</summary>
    public const int ManagerId = 1;

    // The app's own strength rule (see UserController.ValidatePasswordStrength).
    private const string Special = "@$!%*?&";

    public static void Seed(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();
        var db     = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            var group = db.Set<UserGroupEntity>().FirstOrDefault(g => g.GroupName == DefaultGroup);
            if (group == null)
            {
                group = new UserGroupEntity
                {
                    GroupName = DefaultGroup,
                    GroupType = "Manager",
                    CreatedAt = DateTime.UtcNow,
                    InActive  = false,
                };
                db.Set<UserGroupEntity>().Add(group);
                db.SaveChanges(); // group.Id populated after this
            }

            var exists = db.Set<UserEntity>().Any(u => u.UserId == DefaultUserId);
            if (!exists)
            {
                var configured = config["Seed:ManagerPassword"];
                var generated  = false;
                string password;
                if (IsStrong(configured)) password = configured!;
                else
                {
                    if (!string.IsNullOrEmpty(configured))
                        logger.LogWarning("Seed__ManagerPassword is set but too weak (12+ characters with upper, lower, a digit and one of {Special}, and not a common password); a random password is used instead.", Special);
                    password  = Generate();
                    generated = true;
                }

                // Manager always gets Id 1 when that Id is free (a fresh database). The identity counter is moved
                // past it afterwards so the next user doesn't collide. If something else already holds Id 1 the
                // database picks the Id as usual.
                var idOne = !db.Set<UserEntity>().Any(u => u.Id == ManagerId);
                if (!idOne) logger.LogWarning("User Id {Id} is already taken, so {UserId} was given the next free Id.", ManagerId, DefaultUserId);

                db.Set<UserEntity>().Add(new UserEntity
                {
                    Id           = idOne ? ManagerId : 0,
                    UserId       = DefaultUserId,
                    UserName     = "Manager",
                    Email        = "manager@farm-erp.local",
                    PasswordHash = HashPassword(password),
                    UserGroupId  = group.Id,
                    ActiveStatus = 'A',
                    IsLocked     = false,
                    LoginAttempts = 0,
                    CreatedAt    = DateTime.UtcNow,
                    InActive     = false,
                });
                db.SaveChanges();
                if (idOne)
                    db.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"KUSR\"', 'Id'), (SELECT MAX(\"Id\") FROM \"KUSR\"))");
                if (generated)
                    logger.LogWarning("Seeded login {UserId} with the generated password {Password} — it is shown only now. Sign in and change it (Administration > Set Up > General > Security > Change Password).", DefaultUserId, password);
                else
                    logger.LogInformation("Seeded login {UserId} with the password from Seed__ManagerPassword.", DefaultUserId);
            }
        }
        catch (Exception ex)
        {
            // A fresh database with no migrations applied yet would throw
            // here (the tables don't exist) — never let seeding crash startup.
            logger.LogWarning(ex, "Default user seed skipped (database may not be migrated yet).");
        }
    }

    // The same rules new passwords follow everywhere else (Core/PasswordRules.cs), with the default policy.
    private static bool IsStrong(string? p) =>
        PasswordRules.Check(p, new PasswordPolicyView(12, true, true, true, true), DefaultUserId) == null;

    // 20 characters that always satisfy the strength rule; ambiguous ones (0/O, 1/l/I) are left out
    // so it can be read from a log line and typed.
    private static string Generate()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ", lower = "abcdefghijkmnopqrstuvwxyz", digits = "23456789";
        var all = upper + lower + digits + Special;
        static char Pick(string set) => set[RandomNumberGenerator.GetInt32(set.Length)];

        var chars = new List<char> { Pick(upper), Pick(lower), Pick(digits), Pick(Special) };
        while (chars.Count < 20) chars.Add(Pick(all));
        for (var i = chars.Count - 1; i > 0; i--)   // Fisher–Yates, so the required characters aren't always first
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars.ToArray());
    }

    private static string HashPassword(string password) => PasswordHashing.Hash(password);
}

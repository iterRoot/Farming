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
// Uses the exact hashing scheme UserController.HashPassword does
// (SHA-256 of the UTF-8 bytes, Base64), so this account logs in
// through the normal /User/Login endpoint like any other user.
// ═══════════════════════════════════════════════════════════════
public static class DefaultUserSeeder
{
    public const string DefaultUserId   = "Manager";
    public const string DefaultPassword = "Admin@2026"; // meets the app's own strength rule
    public const string DefaultGroup    = "Managers";

    public static void Seed(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<object>>();
        var db     = scope.ServiceProvider.GetRequiredService<MyDbContext>();

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
                db.Set<UserEntity>().Add(new UserEntity
                {
                    UserId       = DefaultUserId,
                    UserName     = "Manager",
                    Email        = "manager@farm-erp.local",
                    PasswordHash = HashPassword(DefaultPassword),
                    UserGroupId  = group.Id,
                    ActiveStatus = 'A',
                    IsLocked     = false,
                    LoginAttempts = 0,
                    CreatedAt    = DateTime.UtcNow,
                    InActive     = false,
                });
                db.SaveChanges();
                logger.LogInformation("Seeded default login: {UserId}", DefaultUserId);
            }
        }
        catch (Exception ex)
        {
            // A fresh database with no migrations applied yet would throw
            // here (the tables don't exist) — never let seeding crash startup.
            logger.LogWarning(ex, "Default user seed skipped (database may not be migrated yet).");
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }
}

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        // Folds any Chart-of-Accounts balance impact of this call's
        // JournalEntry/JournalEntryLine changes into the same atomic write
        // — see Core/GLBalanceSync.cs.
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            GLBalanceSync.ApplyPendingDeltas(this);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            GLBalanceSync.ApplyPendingDeltas(this);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Disable cascade delete
            var relationships = modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys());
            foreach (var r in relationships)
            {
                r.DeleteBehavior = DeleteBehavior.NoAction;
            }

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // Apply all IEntityTypeConfiguration automatically
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            const string prefix = "FarmingApi.Modules";
            var baseType = typeof(Entity);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .ToArray();

            var types = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
                })
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    baseType.IsAssignableFrom(t) &&
                    t.Namespace != null &&
                    t.Namespace.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .ToList();

            foreach (var type in types)
            {
                if (modelBuilder.Model.FindEntityType(type) == null)
                {
                    modelBuilder.Entity(type);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }

    public static class DatabaseInjection
    {
        public static void AddDatabase(this IServiceCollection service)
        {
            var dbConnection = Environment.GetEnvironmentVariable("DB_CONNECTION");
            dbConnection ??= "Host=localhost;Port=5432;Database=Farming;Username=postgres;Password=mypassword;";
            service.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(dbConnection);
            });
        }
    }
}
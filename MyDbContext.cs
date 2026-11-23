using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KamnttraApi.Core;

namespace KamnttraApi
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        // ── Journal-number lock ───────────────────────────────────────
        // Journal numbers are 'highest + 1'. Two postings at once read the same highest and the second one
        // failed on the unique index. JournalEntryNumberService.Next takes this lock first so numbers are handed
        // out one at a time. Inside a transaction it's an xact lock, released at commit or rollback (so a
        // rolled-back posting leaves no gap). Outside one it's a session lock on this context's own connection,
        // released after the next save, or when the context is disposed.
        private const long JournalNumberLockKey = 0x4A524E4E4F; // "JRNNO"
        private bool _holdsJournalLock;

        public void LockJournalNumbering()
        {
            if (Database.CurrentTransaction != null)
            {
                Database.ExecuteSqlRaw($"SELECT pg_advisory_xact_lock({JournalNumberLockKey})");
                return;
            }
            if (_holdsJournalLock) return;
            Database.OpenConnection();                       // keep one connection so the session lock stays ours
            Database.ExecuteSqlRaw($"SELECT pg_advisory_lock({JournalNumberLockKey})");
            _holdsJournalLock = true;
        }

        private void ReleaseJournalLock()
        {
            if (!_holdsJournalLock) return;
            _holdsJournalLock = false;
            try { Database.ExecuteSqlRaw($"SELECT pg_advisory_unlock({JournalNumberLockKey})"); }
            finally { Database.CloseConnection(); }
        }

        public override void Dispose() { try { ReleaseJournalLock(); } catch { /* connection already gone: the server drops the lock */ } base.Dispose(); }

        // Folds any Chart-of-Accounts balance impact of this call's
        // JournalEntry/JournalEntryLine changes into the same atomic write
        // — see Core/GLBalanceSync.cs.
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            DocumentTotalsGuard.Apply(this);      // refuses invoices whose total doesn't match their lines — Core/DocumentTotalsGuard.cs
            DocumentLedgerGuard.Apply(this);      // voids a deleted document's journal entry first — Core/DocumentLedgerGuard.cs
            GLBalanceSync.ApplyPendingDeltas(this);
            try
            {
                ColumnLengthGuard.Apply(this);    // text longer than its column → 400, not a database error — Core/ColumnLengthGuard.cs
                return base.SaveChanges(acceptAllChangesOnSuccess);
            }
            finally { ReleaseJournalLock(); }
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            DocumentTotalsGuard.Apply(this);
            DocumentLedgerGuard.Apply(this);
            GLBalanceSync.ApplyPendingDeltas(this);
            try
            {
                ColumnLengthGuard.Apply(this);
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            finally { ReleaseJournalLock(); }
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

            const string prefix = "KamnttraApi.Modules";
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
            dbConnection ??= "Host=localhost;Port=5432;Database=SBOKAndD;Username=postgres;Password=mypassword;";
            service.AddDbContext<MyDbContext>(options =>
            {
                options.UseNpgsql(dbConnection);
            });
        }
    }
}
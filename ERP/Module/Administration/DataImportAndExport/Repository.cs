using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DataImportAndExport;

public interface IDataTransferJobRepository : IRepository<DataTransferJob>
{
    /// <summary>Next job number in the form DIE-2026-00001.</summary>
    string NextJobNo();

    /// <summary>Create + persist a job row in the Running state.</summary>
    DataTransferJob Start(string direction, string entityKey, string entityLabel,
                          string mode, string? fileName, long fileSize);

    /// <summary>Stamp the outcome onto a job and persist it.</summary>
    void Finish(DataTransferJob job, string status,
                int total, int success, int failed, int skipped,
                IEnumerable<string>? errors = null);
}

public class DataTransferJobRepository
    : Repository<DataTransferJob>, IDataTransferJobRepository
{
    private readonly MyDbContext _db;

    public DataTransferJobRepository(MyDbContext context) : base(context) => _db = context;

    // ── DIE-2026-00001 ────────────────────────────────────────────
    public string NextJobNo()
    {
        var prefix = $"DIE-{DateTime.Now.Year}-";

        var last = _db.Set<DataTransferJob>()
                      .Where(j => j.JobNo.StartsWith(prefix))
                      .OrderByDescending(j => j.Id)
                      .FirstOrDefault();

        if (last == null) return $"{prefix}00001";

        var parts = last.JobNo.Split('-');
        if (parts.Length >= 3 && int.TryParse(parts[2], out var n))
            return $"{prefix}{(n + 1):D5}";

        return $"{prefix}00001";
    }

    public DataTransferJob Start(string direction, string entityKey, string entityLabel,
                                 string mode, string? fileName, long fileSize)
    {
        var job = new DataTransferJob
        {
            JobNo       = NextJobNo(),
            Direction   = direction,
            EntityKey   = entityKey,
            EntityLabel = entityLabel,
            Mode        = mode,
            FileName    = fileName,
            FileSize    = fileSize,
            Format      = "CSV",
            Status      = "R",
            StartedAt   = DateTime.UtcNow,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };

        _db.Set<DataTransferJob>().Add(job);
        _db.SaveChanges();
        return job;
    }

    public void Finish(DataTransferJob job, string status,
                       int total, int success, int failed, int skipped,
                       IEnumerable<string>? errors = null)
    {
        job.Status      = status;
        job.TotalRows   = total;
        job.SuccessRows = success;
        job.FailedRows  = failed;
        job.SkippedRows = skipped;
        job.CompletedAt = DateTime.UtcNow;
        job.DurationMs  = job.StartedAt.HasValue
            ? (int)(job.CompletedAt.Value - job.StartedAt.Value).TotalMilliseconds
            : 0;
        job.UpdatedAt   = DateTime.UtcNow;

        if (errors != null)
        {
            // Cap the stored log — a 10k-row disaster should not blow up the row.
            var list = errors.Take(500).ToList();
            job.ErrorLog = list.Count > 0 ? string.Join("\n", list) : null;
        }

        _db.SaveChanges();
    }
}

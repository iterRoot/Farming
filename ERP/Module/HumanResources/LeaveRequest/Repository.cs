using FarmingApi.Core;

namespace FarmingApi.Modules.HumanResources.LeaveRequest;

public interface ILeaveRequestRepository : IRepository<LeaveRequest>
{
    /// <summary>Next request number in the form LVR-2026-00001.</summary>
    string NextRequestNo();
}

public class LeaveRequestRepository : Repository<LeaveRequest>, ILeaveRequestRepository
{
    private readonly MyDbContext _db;

    public LeaveRequestRepository(MyDbContext ctx) : base(ctx) => _db = ctx;

    public string NextRequestNo()
    {
        var prefix = $"LVR-{DateTime.UtcNow.Year}-";

        var last = _db.Set<LeaveRequest>()
            .Where(x => x.RequestNo.StartsWith(prefix))
            .OrderByDescending(x => x.Id)
            .FirstOrDefault();

        if (last == null) return $"{prefix}00001";

        var parts = last.RequestNo.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out var n)
            ? $"{prefix}{(n + 1):D5}"
            : $"{prefix}00001";
    }
}

using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DashboardManager;

public interface IDashboardManagerRepository : IRepository<DashboardConfig> { }

public class DashboardManagerRepository
    : Repository<DashboardConfig>, IDashboardManagerRepository
{
    public DashboardManagerRepository(MyDbContext ctx) : base(ctx) { }
}
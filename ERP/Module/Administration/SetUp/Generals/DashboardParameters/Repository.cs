using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DashboardParameters;

public interface IDashboardParameterRepository
    : IRepository<DashboardParameter> { }

public class DashboardParameterRepository
    : Repository<DashboardParameter>, IDashboardParameterRepository
{
    public DashboardParameterRepository(MyDbContext ctx) : base(ctx) { }
}
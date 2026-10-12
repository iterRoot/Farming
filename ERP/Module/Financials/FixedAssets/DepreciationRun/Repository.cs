using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FixedAssets.DepreciationRun;

public interface IDepreciationRunRepository : IRepository<DepreciationRun> { }

public class DepreciationRunRepository : Repository<DepreciationRun>, IDepreciationRunRepository
{
    public DepreciationRunRepository(MyDbContext context) : base(context) { }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.FixedAssets.DepreciationRun;

public interface IDepreciationRunRepository : IRepository<DepreciationRun> { }

public class DepreciationRunRepository : Repository<DepreciationRun>, IDepreciationRunRepository
{
    public DepreciationRunRepository(MyDbContext context) : base(context) { }
}

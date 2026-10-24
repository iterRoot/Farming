using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.EditChartOfAccounts;

public interface IChartOfAccountsEditBatchRepository : IRepository<ChartOfAccountsEditBatch> { }

public class ChartOfAccountsEditBatchRepository
    : Repository<ChartOfAccountsEditBatch>, IChartOfAccountsEditBatchRepository
{
    public ChartOfAccountsEditBatchRepository(MyDbContext context) : base(context) { }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.EditChartOfAccounts;

public interface IChartOfAccountsEditBatchRepository : IRepository<ChartOfAccountsEditBatch> { }

public class ChartOfAccountsEditBatchRepository
    : Repository<ChartOfAccountsEditBatch>, IChartOfAccountsEditBatchRepository
{
    public ChartOfAccountsEditBatchRepository(MyDbContext context) : base(context) { }
}

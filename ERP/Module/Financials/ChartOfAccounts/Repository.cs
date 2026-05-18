using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public interface IChartOfAccountsRepository : IRepository<ChartOfAccounts>
{
}

public class ChartOfAccountsRepository : Repository<ChartOfAccounts>, IChartOfAccountsRepository
{
    public ChartOfAccountsRepository(MyDbContext context) : base(context)
    {
    }
}
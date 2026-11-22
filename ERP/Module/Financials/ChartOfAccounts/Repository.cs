using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.ChartOfAccounts;

public interface IChartOfAccountsRepository : IRepository<ChartOfAccounts>
{
}

public class ChartOfAccountsRepository : Repository<ChartOfAccounts>, IChartOfAccountsRepository
{
    public ChartOfAccountsRepository(MyDbContext context) : base(context)
    {
    }
}
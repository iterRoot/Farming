using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.Budget;

public interface IBudgetRepository : IRepository<Budget>
{
}

public class BudgetRepository : Repository<Budget>, IBudgetRepository
{
    public BudgetRepository(MyDbContext context) : base(context)
    {
    }
}
using FarmingApi.Core;

namespace FarmingApi.Modules.Expense;

public interface IExpenseRepository : IRepository<Expense>
{
}

public class ExpenseRepository : Repository<Expense >, IExpenseRepository
{
    public ExpenseRepository(MyDbContext context) : base(context)
    {
    }
}
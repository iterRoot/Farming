using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.RecurringPosting;

public interface IRecurringPostingRepository : IRepository<RecurringPosting>
{
}

public class RecurringPostingRepository : Repository<RecurringPosting>, IRecurringPostingRepository
{
    public RecurringPostingRepository(MyDbContext context) : base(context)
    {
    }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.RecurringPosting;

public interface IRecurringPostingRepository : IRepository<RecurringPosting>
{
}

public class RecurringPostingRepository : Repository<RecurringPosting>, IRecurringPostingRepository
{
    public RecurringPostingRepository(MyDbContext context) : base(context)
    {
    }
}

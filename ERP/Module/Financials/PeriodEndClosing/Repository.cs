using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.PeriodEndClosing;

public interface IPeriodEndClosingRepository : IRepository<PeriodEndClosingRun>
{
}

public class PeriodEndClosingRepository : Repository<PeriodEndClosingRun>, IPeriodEndClosingRepository
{
    public PeriodEndClosingRepository(MyDbContext context) : base(context)
    {
    }
}

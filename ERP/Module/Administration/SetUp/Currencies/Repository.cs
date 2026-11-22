using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.SetUp.Currencies;

public interface ICurrenciesRepository : IRepository<Currencies>
{
}

public class CurrenciesRepository : Repository<Currencies>, ICurrenciesRepository
{
    public CurrenciesRepository(MyDbContext context) : base(context)
    {
    }
}

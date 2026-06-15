using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ExchangeRate;

public interface IExchangeRateRepository : IRepository<ExchangeRate> { }
public interface IPriceIndexRepository   : IRepository<PriceIndex>   { }

public class ExchangeRateRepository
    : Repository<ExchangeRate>, IExchangeRateRepository
{
    public ExchangeRateRepository(MyDbContext ctx) : base(ctx) { }
}

public class PriceIndexRepository
    : Repository<PriceIndex>, IPriceIndexRepository
{
    public PriceIndexRepository(MyDbContext ctx) : base(ctx) { }
}
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ExchangeRateDifferences;

public interface IExchangeRateDifferenceRepository : IRepository<ExchangeRateDifference> { }

public class ExchangeRateDifferenceRepository
    : Repository<ExchangeRateDifference>, IExchangeRateDifferenceRepository
{
    public ExchangeRateDifferenceRepository(MyDbContext context) : base(context) { }
}

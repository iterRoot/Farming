using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.Forecast;

public interface IForecastRepository : IRepository<Forecast> { }

public class ForecastRepository : Repository<Forecast>, IForecastRepository
{
    public ForecastRepository(MyDbContext context) : base(context) { }
}

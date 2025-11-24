using FarmingApi.Core;

namespace FarmingApi.Modules.HouseCow;

public interface IHouseCowRepository : IRepository<HouseCow>
{
}

public class HouseCowRepository : Repository<HouseCow>, IHouseCowRepository
{
    public HouseCowRepository(MyDbContext context) : base(context)
    {
    }
}
using FarmingApi.Core;

namespace FarmingApi.Modules.Trakuons;

public interface ITrakuonsRepository : IRepository<Trakuons>
{
}

public class TrakuonsRepository : Repository<Trakuons>, ITrakuonsRepository
{
    public TrakuonsRepository(MyDbContext context) : base(context)
    {
    }
}
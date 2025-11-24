using FarmingApi.Core;

namespace FarmingApi.Modules.CowFeed;

public interface ICowFeedRepository : IRepository<CowFeed>
{
}

public class CowFeedRepository : Repository<CowFeed>, ICowFeedRepository
{
    public CowFeedRepository(MyDbContext context) : base(context)
    {
    }
}
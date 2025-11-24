using FarmingApi.Core;

namespace FarmingApi.Modules.MilkCow;

public interface ICowRepository : IRepository<Cow>
{
}

public class CowRepository : Repository<Cow>, ICowRepository
{
    public CowRepository(MyDbContext context) : base(context)
    {
    }
}
using FarmingApi.Core;

namespace FarmingApi.Modules.Sheep;

public interface ISheepRepository : IRepository<Sheep>
{
}

public class SheepRepository : Repository<Sheep>, ISheepRepository
{
    public SheepRepository(MyDbContext context) : base(context)
    {
    }
}
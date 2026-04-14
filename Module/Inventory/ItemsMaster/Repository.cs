using FarmingApi.Core;

namespace FarmingApi.Modules.Master.ItemsMaster;

public interface IItemsMasterRepository : IRepository<ItemsMaster>
{
}

public class ItemsMasterRepository : Repository<ItemsMaster>, IItemsMasterRepository
{
    public ItemsMasterRepository(MyDbContext context) : base(context)
    {
    }
}
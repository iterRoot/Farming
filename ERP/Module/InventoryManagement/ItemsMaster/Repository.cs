using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.ItemsMaster;

public interface IItemsMasterRepository : IRepository<ItemsMaster>
{
}

public class ItemsMasterRepository : Repository<ItemsMaster>, IItemsMasterRepository
{
    public ItemsMasterRepository(MyDbContext context) : base(context)
    {
    }
}
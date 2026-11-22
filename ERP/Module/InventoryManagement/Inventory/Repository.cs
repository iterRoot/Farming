using KamnttraApi.Core;

namespace KamnttraApi.Modules.Sale.Inventory;

public interface IInventoryRepository : IRepository<Inventory>
{
}

public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(MyDbContext context) : base(context)
    {
    }
}
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Sale.Warehouse;

public interface IWarehouseRepository : IRepository<Warehouse>
{
}

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(MyDbContext context) : base(context)
    {
    }
}
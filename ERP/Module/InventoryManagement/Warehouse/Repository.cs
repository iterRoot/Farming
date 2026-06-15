using FarmingApi.Core;

namespace FarmingApi.Modules.Sale.Warehouse;

public interface IWarehouseRepository : IRepository<Warehouse>
{
}

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(MyDbContext context) : base(context)
    {
    }
}
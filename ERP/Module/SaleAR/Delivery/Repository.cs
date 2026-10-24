using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.Delivery;

public interface ISaleOrderRepository : IRepository<Delivery>
{
}

public class SaleOrderRepository : Repository<Delivery>, ISaleOrderRepository
{
    public SaleOrderRepository(MyDbContext context) : base(context)
    {
    }
}
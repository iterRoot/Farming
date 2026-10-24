using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.SaleOrder;

public interface ISaleOrderRepository : IRepository<SaleOrder>
{
}

public class SaleOrderRepository : Repository<SaleOrder>, ISaleOrderRepository
{
    public SaleOrderRepository(MyDbContext context) : base(context)
    {
    }
}
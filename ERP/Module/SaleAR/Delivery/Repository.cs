using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.Delivery;

public interface ISaleOrderRepository : IRepository<Delivery>
{
}

public class SaleOrderRepository : Repository<Delivery>, ISaleOrderRepository
{
    public SaleOrderRepository(MyDbContext context) : base(context)
    {
    }
}
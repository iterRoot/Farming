using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.Return;

public interface ISaleOrderRepository : IRepository<Return>
{
}

public class SaleOrderRepository : Repository<Return>, ISaleOrderRepository
{
    public SaleOrderRepository(MyDbContext context) : base(context)
    {
    }
}
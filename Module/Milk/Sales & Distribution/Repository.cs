using FarmingApi.Core;

namespace FarmingApi.Modules.Sales
{
    public interface ISalesRepository : IRepository<Sales>
    {
    }

    public class SalesRepository : Repository<Sales>, ISalesRepository
    {
        public SalesRepository(MyDbContext context) : base(context)
        {
        }
    }
}

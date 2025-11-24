using FarmingApi.Core;

namespace FarmingApi.Modules.Production
{
    public interface IProductionRepository : IRepository<Production>
    {
    }

    public class ProductionRepository : Repository<Production>, IProductionRepository
    {
        public ProductionRepository(MyDbContext context) : base(context)
        {
        }
    }
}

using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.ProductionOrder;

public interface IProductionOrderRepository : IRepository<ProductionOrder> { }

public class ProductionOrderRepository : Repository<ProductionOrder>, IProductionOrderRepository
{
    public ProductionOrderRepository(MyDbContext context) : base(context) { }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.InventoryManagement.ProductionOrder;

public interface IProductionOrderRepository : IRepository<ProductionOrder> { }

public class ProductionOrderRepository : Repository<ProductionOrder>, IProductionOrderRepository
{
    public ProductionOrderRepository(MyDbContext context) : base(context) { }
}

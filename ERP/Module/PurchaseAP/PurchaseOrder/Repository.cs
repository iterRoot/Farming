using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.PurchaseOrder;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder>
{
}

public class PurchaseOrderRepository 
    : Repository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(MyDbContext context) : base(context)
    {
    }
}
using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.PurchaseOrder;

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
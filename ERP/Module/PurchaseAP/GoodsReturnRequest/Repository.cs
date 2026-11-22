using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.GoodsReturnRequest;

public interface IGoodsReturnRequestRepository : IRepository<GoodsReturnRequest>
{
}

public class GoodsReturnRequestRepository : Repository<GoodsReturnRequest>, IGoodsReturnRequestRepository
{
    public GoodsReturnRequestRepository(MyDbContext context) : base(context)
    {
    }
}
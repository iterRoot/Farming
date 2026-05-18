using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

public interface IGoodsReceiptPORepository : IRepository<GoodsReceiptPO>
{
}

public class GoodsReceiptPORepository 
    : Repository<GoodsReceiptPO>, IGoodsReceiptPORepository
{
    public GoodsReceiptPORepository(MyDbContext context) : base(context)
    {
    }
}
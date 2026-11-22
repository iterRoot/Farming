using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.GoodsReceiptPO;

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
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.GoodsReceipt;

public interface IGoodsReceiptRepository : IRepository<GoodsReceipt>
{
}

public class GoodsReceiptRepository : Repository<GoodsReceipt>, IGoodsReceiptRepository
{
    public GoodsReceiptRepository(MyDbContext context) : base(context)
    {
    }
}
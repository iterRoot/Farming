using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.GoodsReceipt;

public interface IGoodsReceiptRepository : IRepository<GoodsReceipt>
{
}

public class GoodsReceiptRepository : Repository<GoodsReceipt>, IGoodsReceiptRepository
{
    public GoodsReceiptRepository(MyDbContext context) : base(context)
    {
    }
}
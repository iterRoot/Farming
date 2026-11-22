using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.GoodsReturn;

public interface IGoodsReturnRepository : IRepository<GoodsReturn>
{
}

public class GoodsReturnRepository : Repository<GoodsReturn>, IGoodsReturnRepository
{
    public GoodsReturnRepository(MyDbContext context) : base(context)
    {
    }
}
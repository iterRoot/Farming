using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.SaleQuotion;

public interface ISaleQuotionRepository : IRepository<SaleQuotion>
{
}

public class SaleQuotionRepository : Repository<SaleQuotion>, ISaleQuotionRepository
{
    public SaleQuotionRepository(MyDbContext context) : base(context)
    {
    }
}
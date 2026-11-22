using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.LandedCostType;

public interface ILandedCostTypeRepository : IRepository<LandedCostType> { }

public class LandedCostTypeRepository : Repository<LandedCostType>, ILandedCostTypeRepository
{
    public LandedCostTypeRepository(MyDbContext ctx) : base(ctx) { }
}

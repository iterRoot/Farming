

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.Weight;

public interface IWeightUomRepository : IRepository<WeightUom> { }

public class WeightUomRepository : Repository<WeightUom>, IWeightUomRepository
{
    public WeightUomRepository(MyDbContext context) : base(context) { }
}
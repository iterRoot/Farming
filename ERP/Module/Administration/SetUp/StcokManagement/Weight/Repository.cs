

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.Weight;

public interface IWeightUomRepository : IRepository<WeightUom> { }

public class WeightUomRepository : Repository<WeightUom>, IWeightUomRepository
{
    public WeightUomRepository(MyDbContext context) : base(context) { }
}
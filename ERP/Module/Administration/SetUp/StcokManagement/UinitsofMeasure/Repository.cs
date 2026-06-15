
// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.UnitOfMeasure;

public interface IUnitOfMeasureRepository : IRepository<UnitOfMeasure> { }

public class UnitOfMeasureRepository : Repository<UnitOfMeasure>, IUnitOfMeasureRepository
{
    public UnitOfMeasureRepository(MyDbContext context) : base(context) { }
}

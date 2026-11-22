
// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.UnitOfMeasure;

public interface IUnitOfMeasureRepository : IRepository<UnitOfMeasure> { }

public class UnitOfMeasureRepository : Repository<UnitOfMeasure>, IUnitOfMeasureRepository
{
    public UnitOfMeasureRepository(MyDbContext context) : base(context) { }
}

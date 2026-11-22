

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.CycleCount;

public interface ICycleCountRepository : IRepository<CycleCountDetermination> { }

public class CycleCountRepository
    : Repository<CycleCountDetermination>, ICycleCountRepository
{
    public CycleCountRepository(MyDbContext context) : base(context) { }
}

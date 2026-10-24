

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.CycleCount;

public interface ICycleCountRepository : IRepository<CycleCountDetermination> { }

public class CycleCountRepository
    : Repository<CycleCountDetermination>, ICycleCountRepository
{
    public CycleCountRepository(MyDbContext context) : base(context) { }
}

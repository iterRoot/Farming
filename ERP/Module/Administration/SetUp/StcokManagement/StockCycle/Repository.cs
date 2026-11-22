

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.StockCycles;

public interface IStockCycleRepository : IRepository<StockCycle> { }

public class StockCycleRepository : Repository<StockCycle>, IStockCycleRepository
{
    public StockCycleRepository(MyDbContext context) : base(context) { }
}



// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.StockCycles;

public interface IStockCycleRepository : IRepository<StockCycle> { }

public class StockCycleRepository : Repository<StockCycle>, IStockCycleRepository
{
    public StockCycleRepository(MyDbContext context) : base(context) { }
}

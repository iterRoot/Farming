

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.ItemProperties;

public interface IItemPropertyRepository : IRepository<ItemProperty> { }

public class ItemPropertyRepository : Repository<ItemProperty>, IItemPropertyRepository
{
    public ItemPropertyRepository(MyDbContext context) : base(context) { }
}

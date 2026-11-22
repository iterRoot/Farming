

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.ItemProperties;

public interface IItemPropertyRepository : IRepository<ItemProperty> { }

public class ItemPropertyRepository : Repository<ItemProperty>, IItemPropertyRepository
{
    public ItemPropertyRepository(MyDbContext context) : base(context) { }
}


// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.ItemGroup;

public interface IItemGroupRepository : IRepository<ItemGroup> { }

public class ItemGroupRepository : Repository<ItemGroup>, IItemGroupRepository
{
    public ItemGroupRepository(MyDbContext context) : base(context) { }
}


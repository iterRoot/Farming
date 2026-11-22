
// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.ItemGroup;

public interface IItemGroupRepository : IRepository<ItemGroup> { }

public class ItemGroupRepository : Repository<ItemGroup>, IItemGroupRepository
{
    public ItemGroupRepository(MyDbContext context) : base(context) { }
}


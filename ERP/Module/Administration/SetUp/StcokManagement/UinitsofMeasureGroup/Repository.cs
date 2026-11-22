
// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.UomGroup;

public interface IUomGroupRepository : IRepository<UomGroup> { }

public class UomGroupRepository : Repository<UomGroup>, IUomGroupRepository
{
    public UomGroupRepository(MyDbContext context) : base(context) { }
}

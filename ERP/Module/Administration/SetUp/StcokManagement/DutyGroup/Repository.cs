


// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.DutyGroups;

public interface IDutyGroupRepository : IRepository<DutyGroup> { }

public class DutyGroupRepository : Repository<DutyGroup>, IDutyGroupRepository
{
    public DutyGroupRepository(MyDbContext context) : base(context) { }
}


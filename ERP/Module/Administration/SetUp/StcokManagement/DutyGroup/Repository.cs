


// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.DutyGroups;

public interface IDutyGroupRepository : IRepository<DutyGroup> { }

public class DutyGroupRepository : Repository<DutyGroup>, IDutyGroupRepository
{
    public DutyGroupRepository(MyDbContext context) : base(context) { }
}


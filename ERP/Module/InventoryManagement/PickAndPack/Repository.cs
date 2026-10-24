using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.PickAndPack;

public interface IPickPackRepository : IRepository<PickPackCriteria> { }

public class PickPackRepository : Repository<PickPackCriteria>, IPickPackRepository
{
    public PickPackRepository(MyDbContext context) : base(context) { }
}

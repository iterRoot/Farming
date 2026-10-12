using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.PickList;

public interface IPickListRepository : IRepository<PickList> { }

public class PickListRepository : Repository<PickList>, IPickListRepository
{
    public PickListRepository(MyDbContext context) : base(context) { }
}

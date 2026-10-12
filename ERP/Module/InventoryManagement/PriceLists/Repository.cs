using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.PriceLists;

public interface IPriceListRepository : IRepository<PriceList> { }

public class PriceListRepository : Repository<PriceList>, IPriceListRepository
{
    public PriceListRepository(MyDbContext context) : base(context) { }
}

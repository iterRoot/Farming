using KamnttraApi.Core;

namespace KamnttraApi.Modules.InventoryManagement.PriceLists;

public interface IPriceListRepository : IRepository<PriceList> { }

public class PriceListRepository : Repository<PriceList>, IPriceListRepository
{
    public PriceListRepository(MyDbContext context) : base(context) { }
}

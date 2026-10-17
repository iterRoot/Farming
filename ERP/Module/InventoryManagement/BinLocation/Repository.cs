using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.BinLocation;

public interface IBinLocationRepository : IRepository<BinLocation> { }

public class BinLocationRepository : Repository<BinLocation>, IBinLocationRepository
{
    public BinLocationRepository(MyDbContext context) : base(context) { }
}

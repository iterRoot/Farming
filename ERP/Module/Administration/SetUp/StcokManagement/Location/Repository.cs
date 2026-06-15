

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.Locations;

public interface ILocationRepository : IRepository<Location> { }

public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(MyDbContext context) : base(context) { }
}


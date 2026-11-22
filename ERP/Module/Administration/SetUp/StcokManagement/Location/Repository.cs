

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.Locations;

public interface ILocationRepository : IRepository<Location> { }

public class LocationRepository : Repository<Location>, ILocationRepository
{
    public LocationRepository(MyDbContext context) : base(context) { }
}


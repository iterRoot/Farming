

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.Manufacturers;

public interface IManufacturerRepository : IRepository<Manufacturer> { }

public class ManufacturerRepository : Repository<Manufacturer>, IManufacturerRepository
{
    public ManufacturerRepository(MyDbContext context) : base(context) { }
}

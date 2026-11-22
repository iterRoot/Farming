

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.Manufacturers;

public interface IManufacturerRepository : IRepository<Manufacturer> { }

public class ManufacturerRepository : Repository<Manufacturer>, IManufacturerRepository
{
    public ManufacturerRepository(MyDbContext context) : base(context) { }
}

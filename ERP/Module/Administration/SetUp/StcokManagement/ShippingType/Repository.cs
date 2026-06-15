

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.ShippingTypes;

public interface IShippingTypeRepository : IRepository<ShippingType> { }

public class ShippingTypeRepository : Repository<ShippingType>, IShippingTypeRepository
{
    public ShippingTypeRepository(MyDbContext context) : base(context) { }
}

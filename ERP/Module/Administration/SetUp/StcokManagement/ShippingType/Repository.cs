

// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.ShippingTypes;

public interface IShippingTypeRepository : IRepository<ShippingType> { }

public class ShippingTypeRepository : Repository<ShippingType>, IShippingTypeRepository
{
    public ShippingTypeRepository(MyDbContext context) : base(context) { }
}

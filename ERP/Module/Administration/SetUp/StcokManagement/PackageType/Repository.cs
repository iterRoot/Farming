
// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.PackageTypes;

public interface IPackageTypeRepository : IRepository<PackageType> { }

public class PackageTypeRepository : Repository<PackageType>, IPackageTypeRepository
{
    public PackageTypeRepository(MyDbContext context) : base(context) { }
}

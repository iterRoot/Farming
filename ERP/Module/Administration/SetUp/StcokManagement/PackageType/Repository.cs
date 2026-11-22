
// ═══════════════════════════════════════════════════════════════
// Repository.cs
// ═══════════════════════════════════════════════════════════════
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.PackageTypes;

public interface IPackageTypeRepository : IRepository<PackageType> { }

public class PackageTypeRepository : Repository<PackageType>, IPackageTypeRepository
{
    public PackageTypeRepository(MyDbContext context) : base(context) { }
}

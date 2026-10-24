using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Licence;

public interface ILicensePackageRepository : IRepository<LicensePackage> { }

public class LicensePackageRepository : Repository<LicensePackage>, ILicensePackageRepository
{
    public LicensePackageRepository(MyDbContext context) : base(context) { }
}

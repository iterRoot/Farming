using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.Licence;

public interface ILicensePackageRepository : IRepository<LicensePackage> { }

public class LicensePackageRepository : Repository<LicensePackage>, ILicensePackageRepository
{
    public LicensePackageRepository(MyDbContext context) : base(context) { }
}

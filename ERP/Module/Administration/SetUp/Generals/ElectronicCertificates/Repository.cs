using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.ElectronicCertificates;

public interface IElectronicCertificateRepository
    : IRepository<ElectronicCertificate> { }

public class ElectronicCertificateRepository
    : Repository<ElectronicCertificate>, IElectronicCertificateRepository
{
    public ElectronicCertificateRepository(MyDbContext ctx) : base(ctx) { }
}
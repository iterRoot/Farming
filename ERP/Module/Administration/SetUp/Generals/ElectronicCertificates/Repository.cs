using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ElectronicCertificates;

public interface IElectronicCertificateRepository
    : IRepository<ElectronicCertificate> { }

public class ElectronicCertificateRepository
    : Repository<ElectronicCertificate>, IElectronicCertificateRepository
{
    public ElectronicCertificateRepository(MyDbContext ctx) : base(ctx) { }
}
using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.SaleBlanketAgreement;

public interface ISaleBlanketAgreementRepository : IRepository<SaleBlanketAgreement>
{
}

public class SaleBlanketAgreementRepository : Repository<SaleBlanketAgreement>, ISaleBlanketAgreementRepository
{
    public SaleBlanketAgreementRepository(MyDbContext context) : base(context)
    {
    }
}
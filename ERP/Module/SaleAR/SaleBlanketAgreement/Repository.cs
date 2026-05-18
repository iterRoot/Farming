using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

public interface ISaleBlanketAgreementRepository : IRepository<SaleBlanketAgreement>
{
}

public class SaleBlanketAgreementRepository : Repository<SaleBlanketAgreement>, ISaleBlanketAgreementRepository
{
    public SaleBlanketAgreementRepository(MyDbContext context) : base(context)
    {
    }
}
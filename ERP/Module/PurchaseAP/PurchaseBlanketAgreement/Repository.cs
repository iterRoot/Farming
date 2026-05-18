using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement;

public interface IPurchaseBlanketAgreementRepository : IRepository<PurchaseBlanketAgreement>
{
}

public class PurchaseBlanketAgreementRepository : Repository<PurchaseBlanketAgreement>, IPurchaseBlanketAgreementRepository
{
    public PurchaseBlanketAgreementRepository(MyDbContext context) : base(context)
    {
    }
}
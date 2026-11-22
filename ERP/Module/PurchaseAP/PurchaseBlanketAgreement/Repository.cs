using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.PurchaseBlanketAgreement;

public interface IPurchaseBlanketAgreementRepository : IRepository<PurchaseBlanketAgreement>
{
}

public class PurchaseBlanketAgreementRepository : Repository<PurchaseBlanketAgreement>, IPurchaseBlanketAgreementRepository
{
    public PurchaseBlanketAgreementRepository(MyDbContext context) : base(context)
    {
    }
}
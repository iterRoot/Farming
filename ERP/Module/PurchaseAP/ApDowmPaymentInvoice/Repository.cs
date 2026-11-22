using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.APDownPaymentInvoice;

public interface ISaladRepository : IRepository<APDownPaymentInvoice>
{
}

public class SaladRepository : Repository<APDownPaymentInvoice>, ISaladRepository
{
    public SaladRepository(MyDbContext context) : base(context)
    {
    }
}
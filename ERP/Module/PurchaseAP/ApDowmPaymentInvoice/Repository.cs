using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

public interface ISaladRepository : IRepository<APDownPaymentInvoice>
{
}

public class SaladRepository : Repository<APDownPaymentInvoice>, ISaladRepository
{
    public SaladRepository(MyDbContext context) : base(context)
    {
    }
}
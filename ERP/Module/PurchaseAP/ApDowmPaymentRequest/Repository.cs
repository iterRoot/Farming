using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentRequest;

public interface IAPDownPaymentInvoiceRepository : IRepository<APDownPaymentRequest>
{
}

public class APDownPaymentInvoiceRepository : Repository<APDownPaymentRequest>, IAPDownPaymentInvoiceRepository
{
    public APDownPaymentInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.APDownPaymentRequest;

public interface IAPDownPaymentInvoiceRepository : IRepository<APDownPaymentRequest>
{
}

public class APDownPaymentInvoiceRepository : Repository<APDownPaymentRequest>, IAPDownPaymentInvoiceRepository
{
    public APDownPaymentInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
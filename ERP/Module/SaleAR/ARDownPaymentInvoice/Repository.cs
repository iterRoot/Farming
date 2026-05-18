using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.ARDownPaymentInvoice;

public interface IARDownPaymentInvoiceRepository : IRepository<ARDownPaymentInvoice>
{
}

public class ARDownPaymentInvoiceRepository : Repository<ARDownPaymentInvoice>, IARDownPaymentInvoiceRepository
{
    public ARDownPaymentInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
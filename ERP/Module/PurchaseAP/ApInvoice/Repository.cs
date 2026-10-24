using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.APInvoice;

public interface IAPInvoiceRepository : IRepository<APInvoice>
{
}

public class APInvoiceRepository 
    : Repository<APInvoice>, IAPInvoiceRepository
{
    public APInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
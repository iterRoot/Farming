using KamnttraApi.Core;

namespace KamnttraApi.Modules.PurchaseAP.APReserveInvoice;

public interface IAPReserveInvoiceRepository : IRepository<APReserveInvoice>
{
}

public class APReserveInvoiceRepository : Repository<APReserveInvoice>, IAPReserveInvoiceRepository
{
    public APReserveInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
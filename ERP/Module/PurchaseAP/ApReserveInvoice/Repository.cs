using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.APReserveInvoice;

public interface IAPReserveInvoiceRepository : IRepository<APReserveInvoice>
{
}

public class APReserveInvoiceRepository : Repository<APReserveInvoice>, IAPReserveInvoiceRepository
{
    public APReserveInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
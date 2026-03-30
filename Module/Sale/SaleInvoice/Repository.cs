using FarmingApi.Core;

namespace FarmingApi.Modules.Sale.SaleInvoice;

public interface ISaleInvoiceRepository : IRepository<SaleInvoice>
{
}

public class SaleInvoiceRepository : Repository<SaleInvoice>, ISaleInvoiceRepository
{
    public SaleInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
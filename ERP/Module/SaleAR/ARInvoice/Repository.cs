using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

public interface IARInvoiceRepository : IRepository<ARInvoice>
{
}

public class ARInvoiceRepository : Repository<ARInvoice>, IARInvoiceRepository
{
    public ARInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
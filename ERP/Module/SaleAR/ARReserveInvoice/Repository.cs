using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.ARReserveInvoice;

public interface IARReserveInvoiceRepository : IRepository<ARReserveInvoice>
{
}

public class ARReserveInvoiceRepository : Repository<ARReserveInvoice>, IARReserveInvoiceRepository
{
    public ARReserveInvoiceRepository(MyDbContext context) : base(context)
    {
    }
}
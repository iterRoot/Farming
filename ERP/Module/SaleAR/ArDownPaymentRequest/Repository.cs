using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.ArDownPaymentRequest;

public interface IArDownPaymentRequestRepository : IRepository<ArDownPaymentRequest>
{
}

public class ArDownPaymentRequestRepository : Repository<ArDownPaymentRequest>, IArDownPaymentRequestRepository
{
    public ArDownPaymentRequestRepository(MyDbContext context) : base(context)
    {
    }
}
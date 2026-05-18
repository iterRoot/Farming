using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.ArDownPaymentRequest;

public interface IArDownPaymentRequestRepository : IRepository<ArDownPaymentRequest>
{
}

public class ArDownPaymentRequestRepository : Repository<ArDownPaymentRequest>, IArDownPaymentRequestRepository
{
    public ArDownPaymentRequestRepository(MyDbContext context) : base(context)
    {
    }
}
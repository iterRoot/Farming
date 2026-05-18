using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.IncomingPayment;

public interface IIncomingPaymentRepository : IRepository<IncomingPayment>
{
}

public class IncomingPaymentRepository : Repository<IncomingPayment>, IIncomingPaymentRepository
{
    public IncomingPaymentRepository(MyDbContext context) : base(context)
    {
    }
}
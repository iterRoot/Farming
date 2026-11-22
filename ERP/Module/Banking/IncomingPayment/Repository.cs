using KamnttraApi.Core;

namespace KamnttraApi.Modules.Banking.IncomingPayment;

public interface IIncomingPaymentRepository : IRepository<IncomingPayment>
{
}

public class IncomingPaymentRepository : Repository<IncomingPayment>, IIncomingPaymentRepository
{
    public IncomingPaymentRepository(MyDbContext context) : base(context)
    {
    }
}
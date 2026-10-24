using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.OutgoingPayment;

public interface IOutgoingPaymentRepository : IRepository<OutgoingPayment>
{
}

public class OutgoingPaymentRepository : Repository<OutgoingPayment>, IOutgoingPaymentRepository
{
    public OutgoingPaymentRepository(MyDbContext context) : base(context)
    {
    }
}
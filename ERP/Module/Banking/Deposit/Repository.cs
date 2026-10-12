using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.Deposit;

public interface IDepositRepository : IRepository<Deposit> { }

public class DepositRepository : Repository<Deposit>, IDepositRepository
{
    public DepositRepository(MyDbContext context) : base(context) { }
}

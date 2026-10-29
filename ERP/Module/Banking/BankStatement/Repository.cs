using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.BankStatement;

public interface IBankStatementRepository : IRepository<BankStatement>
{
}

public class BankStatementRepository : Repository<BankStatement>, IBankStatementRepository
{
    public BankStatementRepository(MyDbContext context) : base(context)
    {
    }
}

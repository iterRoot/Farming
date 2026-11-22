using KamnttraApi.Core;

namespace KamnttraApi.Modules.Banking.BankReconciliation;

public interface IBankReconciliationRepository : IRepository<BankReconciliation>
{
}

public class BankReconciliationRepository : Repository<BankReconciliation>, IBankReconciliationRepository
{
    public BankReconciliationRepository(MyDbContext context) : base(context)
    {
    }
}
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Sales.CommissionGroup;

public interface ICommissionGroupRepository : IRepository<CommissionGroup> { }

public class CommissionGroupRepository
    : Repository<CommissionGroup>, ICommissionGroupRepository
{
    public CommissionGroupRepository(MyDbContext context) : base(context) { }
}
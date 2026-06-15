using FarmingApi.Core;

namespace FarmingApi.Modules.Sales.CommissionGroup;

public interface ICommissionGroupRepository : IRepository<CommissionGroup> { }

public class CommissionGroupRepository
    : Repository<CommissionGroup>, ICommissionGroupRepository
{
    public CommissionGroupRepository(MyDbContext context) : base(context) { }
}
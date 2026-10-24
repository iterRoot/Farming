using FarmingApi.Core;

namespace FarmingApi.Modules.HumanResources.EmployeeMaster;

public interface IEmployeeMasterRepository
    : IRepository<EmployeeMaster> { }

public class EmployeeMasterRepository
    : Repository<EmployeeMaster>, IEmployeeMasterRepository
{
    public EmployeeMasterRepository(MyDbContext ctx) : base(ctx) { }
}

using FarmingApi.Core;

namespace FarmingApi.Modules.Company.Staff;

public interface IStaffRepository : IRepository<Staff>
{
}

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(MyDbContext context) : base(context)
    {
    }
}
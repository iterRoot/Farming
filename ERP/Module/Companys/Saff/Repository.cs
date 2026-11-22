using KamnttraApi.Core;

namespace KamnttraApi.Modules.Company.Staff;

public interface IStaffRepository : IRepository<Staff>
{
}

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(MyDbContext context) : base(context)
    {
    }
}
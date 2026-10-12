using FarmingApi.Core;

namespace FarmingApi.Modules.HumanResources.Attendance;

public interface IAttendanceRepository : IRepository<Attendance> { }

public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(MyDbContext ctx) : base(ctx) { }
}

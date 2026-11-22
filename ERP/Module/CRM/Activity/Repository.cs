using KamnttraApi.Core;

namespace KamnttraApi.Modules.CRM.Activity;

public interface IActivityRepository : IRepository<Activity> { }

public class ActivityRepository : Repository<Activity>, IActivityRepository
{
    public ActivityRepository(MyDbContext ctx) : base(ctx) { }
}
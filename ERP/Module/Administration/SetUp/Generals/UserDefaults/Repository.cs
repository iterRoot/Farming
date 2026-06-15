using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.UserDefaults;

public interface IUserDefaultRepository : IRepository<UserDefault> { }

public class UserDefaultRepository : Repository<UserDefault>, IUserDefaultRepository
{
    public UserDefaultRepository(MyDbContext ctx) : base(ctx) { }
}
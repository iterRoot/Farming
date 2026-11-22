using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.UserDefaults;

public interface IUserDefaultRepository : IRepository<UserDefault> { }

public class UserDefaultRepository : Repository<UserDefault>, IUserDefaultRepository
{
    public UserDefaultRepository(MyDbContext ctx) : base(ctx) { }
}
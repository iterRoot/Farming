using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.SetUp.User;

public interface IUserRepository : IRepository<User>
{
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(MyDbContext context) : base(context)
    {
    }
}
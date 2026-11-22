using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.SetUp.UserGroup;

public interface IUserGroupRepository : IRepository<UserGroup>
{
}

public class UserGroupRepository : Repository<UserGroup>, IUserGroupRepository
{
    public UserGroupRepository(MyDbContext context) : base(context)
    {
    }
}
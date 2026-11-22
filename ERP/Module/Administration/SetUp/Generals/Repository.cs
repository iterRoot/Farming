using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.UserGroups;

public interface IUserGroupRepository : IRepository<UserGroup> { }

public class UserGroupRepository : Repository<UserGroup>, IUserGroupRepository
{
    public UserGroupRepository(MyDbContext ctx) : base(ctx) { }
}
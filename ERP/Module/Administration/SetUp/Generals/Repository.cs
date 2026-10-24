using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.UserGroups;

public interface IUserGroupRepository : IRepository<UserGroup> { }

public class UserGroupRepository : Repository<UserGroup>, IUserGroupRepository
{
    public UserGroupRepository(MyDbContext ctx) : base(ctx) { }
}
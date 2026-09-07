using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Authorisations;

public interface IUserAuthorizationRepository : IRepository<UserAuthorization> { }

public class UserAuthorizationRepository : Repository<UserAuthorization>, IUserAuthorizationRepository
{
    public UserAuthorizationRepository(MyDbContext context) : base(context) { }
}

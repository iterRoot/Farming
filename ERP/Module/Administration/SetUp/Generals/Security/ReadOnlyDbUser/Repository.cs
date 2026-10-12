using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Security;

public interface IReadOnlyDbUserRepository : IRepository<ReadOnlyDbUser> { }

public class ReadOnlyDbUserRepository : Repository<ReadOnlyDbUser>, IReadOnlyDbUserRepository
{
    public ReadOnlyDbUserRepository(MyDbContext ctx) : base(ctx) { }
}

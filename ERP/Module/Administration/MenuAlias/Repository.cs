using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.MenuAlias;

public interface IMenuAliasRepository : IRepository<MenuAliasEntry> { }
public class MenuAliasRepository : Repository<MenuAliasEntry>, IMenuAliasRepository
{
    public MenuAliasRepository(MyDbContext context) : base(context) { }
}

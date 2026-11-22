using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.MenuAlias;

public interface IMenuAliasRepository : IRepository<MenuAliasEntry> { }
public class MenuAliasRepository : Repository<MenuAliasEntry>, IMenuAliasRepository
{
    public MenuAliasRepository(MyDbContext context) : base(context) { }
}

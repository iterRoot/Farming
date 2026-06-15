using FarmingApi.Core;

namespace FarmingApi.Modules.Sales.Territories;

public interface ITerritoryRepository : IRepository<Territory> { }

public class TerritoryRepository : Repository<Territory>, ITerritoryRepository
{
    public TerritoryRepository(MyDbContext ctx) : base(ctx) { }
}
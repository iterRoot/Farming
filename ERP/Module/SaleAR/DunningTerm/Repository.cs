using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.DunningTerm;

public interface IDunningTermRepository : IRepository<DunningTerm> { }

public class DunningTermRepository : Repository<DunningTerm>, IDunningTermRepository
{
    public DunningTermRepository(MyDbContext context) : base(context) { }
}

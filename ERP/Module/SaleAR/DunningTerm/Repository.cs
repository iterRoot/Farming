using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.DunningTerm;

public interface IDunningTermRepository : IRepository<DunningTerm> { }

public class DunningTermRepository : Repository<DunningTerm>, IDunningTermRepository
{
    public DunningTermRepository(MyDbContext context) : base(context) { }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.Dunning;

public interface IDunningLetterRepository : IRepository<DunningLetter> { }

public class DunningLetterRepository : Repository<DunningLetter>, IDunningLetterRepository
{
    public DunningLetterRepository(MyDbContext context) : base(context) { }
}

using FarmingApi.Core;

namespace FarmingApi.Modules.Quality
{
    public interface IRawMilkRepository : IRepository<Quality>
    {
    }

    public class RawMilkRepository : Repository<Quality>, IRawMilkRepository
    {
        public RawMilkRepository(MyDbContext context) : base(context)
        {
        }
    }
}

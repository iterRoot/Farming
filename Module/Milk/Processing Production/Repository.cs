using FarmingApi.Core;

namespace FarmingApi.Modules.ProcessingProduction
{
    public interface IRawMilkRepository : IRepository<ProcessingProduction>
    {
    }

    public class RawMilkRepository : Repository<ProcessingProduction>, IRawMilkRepository
    {
        public RawMilkRepository(MyDbContext context) : base(context)
        {
        }
    }
}

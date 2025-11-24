using FarmingApi.Core;

namespace FarmingApi.Modules.RawMilk
{
    public interface IRawMilkRepository : IRepository<RawMilk>
    {
    }

    public class RawMilkRepository : Repository<RawMilk>, IRawMilkRepository
    {
        public RawMilkRepository(MyDbContext context) : base(context)
        {
        }
    }
}

using FarmingApi.Core;

namespace FarmingApi.Modules.House;

public interface IHouseRepository : IRepository<House>
{
}

public class HouseRepository : Repository<House>, IHouseRepository
{
    public HouseRepository(MyDbContext context) : base(context)
    {
    }
}
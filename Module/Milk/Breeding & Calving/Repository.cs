using FarmingApi.Core;

namespace FarmingApi.Modules.Breeding;

public interface IBreedingRepository : IRepository<Breeding>
{
}

public class BreedingRepository : Repository<Breeding>, IBreedingRepository
{
    public BreedingRepository(MyDbContext context) : base(context)
    {
    }
}
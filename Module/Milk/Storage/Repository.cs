using FarmingApi.Core;

namespace FarmingApi.Modules.Storage
{
    public interface IStorageRepository : IRepository<Storage>
    {
    }

    public class StorageRepository : Repository<Storage>, IStorageRepository
    {
        public StorageRepository(MyDbContext context) : base(context)
        {
        }
    }
}

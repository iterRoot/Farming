using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SetUp.Generals.SavedSearch;

public interface ISavedSearchRepository : IRepository<SavedSearch> { }

public class SavedSearchRepository : Repository<SavedSearch>, ISavedSearchRepository
{
    public SavedSearchRepository(MyDbContext context) : base(context) { }
}

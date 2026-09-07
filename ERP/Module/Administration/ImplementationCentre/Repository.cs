using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ImplementationCentre;

public interface IImplementationTaskRepository : IRepository<ImplementationTask> { }
public class ImplementationTaskRepository : Repository<ImplementationTask>, IImplementationTaskRepository
{
    public ImplementationTaskRepository(MyDbContext context) : base(context) { }
}

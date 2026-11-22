using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.ImplementationCentre;

public interface IImplementationTaskRepository : IRepository<ImplementationTask> { }
public class ImplementationTaskRepository : Repository<ImplementationTask>, IImplementationTaskRepository
{
    public ImplementationTaskRepository(MyDbContext context) : base(context) { }
}

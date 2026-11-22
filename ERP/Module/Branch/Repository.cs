using KamnttraApi.Core;

namespace KamnttraApi.Modules.Branch;

public interface IBranchRepository : IRepository<Branch>
{
}

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(MyDbContext context) : base(context)
    {
    }
}
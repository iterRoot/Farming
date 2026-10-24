using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.IssueForProduction;

public interface IIssueForProductionRepository : IRepository<IssueForProduction> { }

public class IssueForProductionRepository : Repository<IssueForProduction>, IIssueForProductionRepository
{
    public IssueForProductionRepository(MyDbContext context) : base(context) { }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.GoodsIssue;

public interface IGoodsIssueRepository : IRepository<GoodsIssue> { }

public class GoodsIssueRepository : Repository<GoodsIssue>, IGoodsIssueRepository
{
    public GoodsIssueRepository(MyDbContext context) : base(context) { }
}
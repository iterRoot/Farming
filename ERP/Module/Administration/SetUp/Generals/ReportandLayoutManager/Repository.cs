using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ReportLayoutManager;

public interface IReportLayoutRepository : IRepository<ReportLayout> { }

public class ReportLayoutRepository
    : Repository<ReportLayout>, IReportLayoutRepository
{
    public ReportLayoutRepository(MyDbContext ctx) : base(ctx) { }
}
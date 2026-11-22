using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.CrystalReportDefaults;

public interface ICrystalReportDefaultRepository
    : IRepository<CrystalReportDefault> { }

public class CrystalReportDefaultRepository
    : Repository<CrystalReportDefault>, ICrystalReportDefaultRepository
{
    public CrystalReportDefaultRepository(MyDbContext ctx) : base(ctx) { }
}
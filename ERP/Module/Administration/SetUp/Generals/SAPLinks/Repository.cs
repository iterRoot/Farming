using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SAPLinks;

public interface ISAPLinkRepository : IRepository<SAPLink> { }

public class SAPLinkRepository : Repository<SAPLink>, ISAPLinkRepository
{
    public SAPLinkRepository(MyDbContext ctx) : base(ctx) { }
}
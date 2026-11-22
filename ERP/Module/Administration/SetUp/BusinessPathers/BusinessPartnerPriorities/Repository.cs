using KamnttraApi.Core;

namespace KamnttraApi.Modules.BusinessPartners.BPProperties;

public interface IBPPropertyRepository : IRepository<BPProperty> { }

public class BPPropertyRepository : Repository<BPProperty>, IBPPropertyRepository
{
    public BPPropertyRepository(MyDbContext ctx) : base(ctx) { }
}
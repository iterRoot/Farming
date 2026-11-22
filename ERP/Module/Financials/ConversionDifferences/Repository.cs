using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.ConversionDifferences;

public interface IConversionDifferenceRepository : IRepository<ConversionDifference> { }

public class ConversionDifferenceRepository
    : Repository<ConversionDifference>, IConversionDifferenceRepository
{
    public ConversionDifferenceRepository(MyDbContext context) : base(context) { }
}

using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.PredefinedText;

public interface IPredefinedTextRepository : IRepository<PredefinedText> { }

public class PredefinedTextRepository
    : Repository<PredefinedText>, IPredefinedTextRepository
{
    public PredefinedTextRepository(MyDbContext ctx) : base(ctx) { }
}
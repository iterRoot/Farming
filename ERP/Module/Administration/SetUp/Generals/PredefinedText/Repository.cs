using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.PredefinedText;

public interface IPredefinedTextRepository : IRepository<PredefinedText> { }

public class PredefinedTextRepository
    : Repository<PredefinedText>, IPredefinedTextRepository
{
    public PredefinedTextRepository(MyDbContext ctx) : base(ctx) { }
}
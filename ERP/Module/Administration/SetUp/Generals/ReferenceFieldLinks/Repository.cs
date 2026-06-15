using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.ReferenceFieldLink;

public interface IReferenceFieldLinkRepository
    : IRepository<ReferenceFieldLink> { }

public class ReferenceFieldLinkRepository
    : Repository<ReferenceFieldLink>, IReferenceFieldLinkRepository
{
    public ReferenceFieldLinkRepository(MyDbContext ctx) : base(ctx) { }
}
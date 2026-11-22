using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.ReferenceFieldLink;

public interface IReferenceFieldLinkRepository
    : IRepository<ReferenceFieldLink> { }

public class ReferenceFieldLinkRepository
    : Repository<ReferenceFieldLink>, IReferenceFieldLinkRepository
{
    public ReferenceFieldLinkRepository(MyDbContext ctx) : base(ctx) { }
}
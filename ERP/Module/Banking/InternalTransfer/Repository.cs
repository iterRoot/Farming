using KamnttraApi.Core;

namespace KamnttraApi.Modules.Banking.InternalTransfer;

public interface IInternalTransferRepository : IRepository<InternalTransfer>
{
}

public class InternalTransferRepository : Repository<InternalTransfer>, IInternalTransferRepository
{
    public InternalTransferRepository(MyDbContext context) : base(context)
    {
    }
}

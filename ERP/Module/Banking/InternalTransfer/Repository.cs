using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.InternalTransfer;

public interface IInternalTransferRepository : IRepository<InternalTransfer>
{
}

public class InternalTransferRepository : Repository<InternalTransfer>, IInternalTransferRepository
{
    public InternalTransferRepository(MyDbContext context) : base(context)
    {
    }
}

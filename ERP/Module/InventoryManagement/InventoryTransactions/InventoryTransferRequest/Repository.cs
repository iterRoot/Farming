using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.StockTransferRequest;

public interface IStockTransferRequestRepository : IRepository<StockTransferRequest>
{
}

public class StockTransferRequestRepository : Repository<StockTransferRequest>, IStockTransferRequestRepository
{
    public StockTransferRequestRepository(MyDbContext context) : base(context)
    {
    }
}
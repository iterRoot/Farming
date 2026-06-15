using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.StockTransferRequest;

public interface IStockTransferRequestRepository : IRepository<StockTransferRequest>
{
}

public class StockTransferRequestRepository : Repository<StockTransferRequest>, IStockTransferRequestRepository
{
    public StockTransferRequestRepository(MyDbContext context) : base(context)
    {
    }
}
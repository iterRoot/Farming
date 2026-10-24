using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.StockTransfer;

public interface IStockTransferRepository : IRepository<StockTransfer>
{
}

public class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(MyDbContext context) : base(context)
    {
    }
}
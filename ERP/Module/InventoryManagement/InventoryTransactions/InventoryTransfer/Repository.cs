using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.StockTransfer;

public interface IStockTransferRepository : IRepository<StockTransfer>
{
}

public class StockTransferRepository : Repository<StockTransfer>, IStockTransferRepository
{
    public StockTransferRepository(MyDbContext context) : base(context)
    {
    }
}
using FarmingApi.Core;

namespace FarmingApi.Modules.Sale.StockMovement;

public interface IStockMovementRepository : IRepository<StockMovement>
{
}

public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(MyDbContext context) : base(context)
    {
    }
}
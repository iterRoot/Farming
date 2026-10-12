using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.StockRevaluation;

public interface IStockRevaluationRepository : IRepository<StockRevaluation> { }

public class StockRevaluationRepository : Repository<StockRevaluation>, IStockRevaluationRepository
{
    public StockRevaluationRepository(MyDbContext context) : base(context) { }
}

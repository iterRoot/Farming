using KamnttraApi.Core;

namespace KamnttraApi.Modules.InventoryManagement.StockRevaluation;

public interface IStockRevaluationRepository : IRepository<StockRevaluation> { }

public class StockRevaluationRepository : Repository<StockRevaluation>, IStockRevaluationRepository
{
    public StockRevaluationRepository(MyDbContext context) : base(context) { }
}

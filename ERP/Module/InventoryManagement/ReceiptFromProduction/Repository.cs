using KamnttraApi.Core;

namespace KamnttraApi.Modules.InventoryManagement.ReceiptFromProduction;

public interface IReceiptFromProductionRepository : IRepository<ReceiptFromProduction> { }

public class ReceiptFromProductionRepository : Repository<ReceiptFromProduction>, IReceiptFromProductionRepository
{
    public ReceiptFromProductionRepository(MyDbContext context) : base(context) { }
}

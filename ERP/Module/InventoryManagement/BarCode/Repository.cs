using KamnttraApi.Core;

namespace KamnttraApi.Modules.Inventory.BarCode;

public interface IBarCodeRepository : IRepository<BarCode> { }

public class BarCodeRepository : Repository<BarCode>, IBarCodeRepository
{
    public BarCodeRepository(MyDbContext context) : base(context)
    {
        
    }
}


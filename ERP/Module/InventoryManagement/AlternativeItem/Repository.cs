using KamnttraApi.Core;

namespace KamnttraApi.Modules.InventoryManagement.AlternativeItem;

public interface IAlternativeItemRepository : IRepository<AlternativeItem> { }

public class AlternativeItemRepository : Repository<AlternativeItem>, IAlternativeItemRepository
{
    public AlternativeItemRepository(MyDbContext context) : base(context) { }
}

using KamnttraApi.Core;

namespace KamnttraApi.Modules.InventoryManagement.BillOfMaterial;

public interface IBillOfMaterialRepository : IRepository<BillOfMaterial> { }

public class BillOfMaterialRepository : Repository<BillOfMaterial>, IBillOfMaterialRepository
{
    public BillOfMaterialRepository(MyDbContext context) : base(context) { }
}

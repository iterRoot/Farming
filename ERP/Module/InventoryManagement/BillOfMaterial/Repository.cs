using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.BillOfMaterial;

public interface IBillOfMaterialRepository : IRepository<BillOfMaterial> { }

public class BillOfMaterialRepository : Repository<BillOfMaterial>, IBillOfMaterialRepository
{
    public BillOfMaterialRepository(MyDbContext context) : base(context) { }
}

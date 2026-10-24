using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FixedAssets;

public interface IAssetMasterRepository : IRepository<AssetMaster> { }
public class AssetMasterRepository : Repository<AssetMaster>, IAssetMasterRepository
{
    public AssetMasterRepository(MyDbContext context) : base(context) { }
}

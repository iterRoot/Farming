using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.CrystalServer;

public interface ICrystalServerConfigRepository
    : IRepository<CrystalServerConfig> { }

public class CrystalServerConfigRepository
    : Repository<CrystalServerConfig>, ICrystalServerConfigRepository
{
    public CrystalServerConfigRepository(MyDbContext ctx) : base(ctx) { }
}
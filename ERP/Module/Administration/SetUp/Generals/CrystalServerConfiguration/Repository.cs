using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.CrystalServer;

public interface ICrystalServerConfigRepository
    : IRepository<CrystalServerConfig> { }

public class CrystalServerConfigRepository
    : Repository<CrystalServerConfig>, ICrystalServerConfigRepository
{
    public CrystalServerConfigRepository(MyDbContext ctx) : base(ctx) { }
}
using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.ServerPrintConfig;

public interface IServerPrintConfigRepository
    : IRepository<ServerPrintConfig> { }

public class ServerPrintConfigRepository
    : Repository<ServerPrintConfig>, IServerPrintConfigRepository
{
    public ServerPrintConfigRepository(MyDbContext ctx) : base(ctx) { }
}